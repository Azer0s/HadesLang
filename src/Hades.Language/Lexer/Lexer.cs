// ReSharper disable once CheckNamespace

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hades.Common.Source;
using Hades.Syntax.Lexeme;

namespace Hades.Language.Lexer
{
    public class Lexer
    {
        private readonly StringBuilder _builder;
        private int _column;
        private int _index;
        private int _line;
        private SourceCode _sourceCode;
        private SourceLocation _tokenStart;

        public Lexer()
        {
            _builder = new StringBuilder();
            _sourceCode = null;
        }

        private char Ch => _sourceCode[_index];

        // ReSharper disable once UnusedMember.Local
        private char Last => Peek(-1);
        private char Next => Peek(1);

        #region Keywords

        private static readonly string[] BlockKeywords =
        {
            Keyword.Class,
            Keyword.Func,
            Keyword.Args,
            Keyword.Requires,
            Keyword.If,
            Keyword.Else,
            Keyword.While,
            Keyword.For,
            Keyword.In,
            Keyword.Stop,
            Keyword.Skip,
            Keyword.Raise,
            Keyword.Try,
            Keyword.Catch,
            Keyword.Struct,
            Keyword.Match,
            Keyword.End
        };

        private static readonly string[] VarKeywords =
        {
            Keyword.Var,
            Keyword.Let,
            Keyword.Null,
            Keyword.Undefined
        };

        private static readonly string[] AccessModifierKeywords =
        {
            Keyword.Protected,
            Keyword.Public,
            Keyword.Private
        };

        private static readonly string[] ImportKeywords =
        {
            Keyword.With,
            Keyword.From,
            Keyword.As,
            Keyword.Sets,
            Keyword.Fixed
        };

        private static readonly string[] MiscKeywords =
        {
            Keyword.Put
        };

        private static List<string> _keywordList = new List<string>();

        private static List<string> GetKeywordList()
        {
            if (_keywordList.Count != 0) return _keywordList;

            var list = BlockKeywords.ToList();
            list.AddRange(VarKeywords.ToList());
            list.AddRange(AccessModifierKeywords.ToList());
            list.AddRange(ImportKeywords.ToList());
            list.AddRange(MiscKeywords.ToList());

            _keywordList = list;
            return _keywordList;
        }

        public static List<string> Keywords => GetKeywordList();

        #endregion

        #region Helper

        private void Advance()
        {
            _index++;
            _column++;
        }

        private void Consume()
        {
            _builder.Append(Ch);
            Advance();
        }

        private void Clear()
        {
            _builder.Clear();
        }

        private Token CreateToken(Classifier kind)
        {
            var contents = _builder.ToString();
            var end = new SourceLocation(_index, _line, _column);
            var start = _tokenStart;

            _tokenStart = end;
            _builder.Clear();

            return new Token(kind, contents, start, end);
        }

        private void DoNewLine()
        {
            _line++;
            _column = 0;
        }

        private char Peek(int ahead)
        {
            return _sourceCode[_index + ahead];
        }

        #endregion

        #region Checks

        private bool IsDigit()
        {
            return char.IsDigit(Ch);
        }

        // ReSharper disable once InconsistentNaming
        private bool IsEOF()
        {
            return Ch == '\0';
        }

        private bool IsIdentifier()
        {
            return IsLetterOrDigit() || Ch == '_';
        }

        private bool IsKeyword()
        {
            return Keywords.Contains(_builder.ToString());
        }

        private bool IsBoolLiteral()
        {
            var builder = _builder.ToString();
            return builder == "true" || builder == "false";
        }

        private bool IsLetter()
        {
            return char.IsLetter(Ch);
        }

        private bool IsLetterOrDigit()
        {
            return char.IsLetterOrDigit(Ch);
        }

        private bool IsNewLine()
        {
            return Ch == '\n';
        }

        private bool IsPunctuation()
        {
            return "<>{}()[]!%^&*+-=/,?:|~#@.".Contains(Ch);
        }

        private bool IsWhiteSpace()
        {
            return (char.IsWhiteSpace(Ch) || IsEOF() || Ch == 8203) && !IsNewLine();
        }

        #endregion

        #region Lexing

        public IEnumerable<Token> LexFile(string sourceCode)
        {
            return LexFile(new SourceCode(sourceCode));
        }

        public IEnumerable<Token> LexFile(SourceCode source)
        {
            _sourceCode = source;
            _builder.Clear();
            _line = 1;
            _index = 0;
            _column = 0;
            CreateToken(Classifier.EndOfFile);

            return LexContents();
        }

        private IEnumerable<Token> LexContents()
        {
            while (!IsEOF())
            {
                yield return LexToken();
            }

            yield return CreateToken(Classifier.EndOfFile);
        }

        private Token LexToken()
        {
            if (IsEOF())
            {
                return CreateToken(Classifier.EndOfFile);
            }

            if (IsNewLine())
            {
                return ScanNewLine();
            }

            if (IsWhiteSpace())
            {
                return ScanWhiteSpace();
            }

            if (IsDigit())
            {
                return ScanInteger();
            }

            if (Ch == '/' && (Next == '/' || Next == '*'))
            {
                return ScanComment();
            }

            if (IsLetter() || Ch == '_' && Ch != '@')
            {
                return ScanIdentifier();
            }

            if (Ch == '"')
            {
                return ScanStringLiteral();
            }

            return IsPunctuation() ? ScanPunctuation() : ScanWord();
        }

        private Token ScanBlockComment()
        {
            bool IsEndOfComment()
            {
                return Ch == '*' && Next == '/';
            }

            while (!IsEndOfComment())
            {
                if (IsEOF())
                {
                    return CreateToken(Classifier.Error);
                }

                if (IsNewLine())
                {
                    DoNewLine();
                }

                Consume();
            }

            Consume();
            Consume();

            return CreateToken(Classifier.BlockComment);
        }

        private Token ScanComment()
        {
            Consume();
            if (Ch == '*')
            {
                return ScanBlockComment();
            }

            Consume();

            while (!IsNewLine() && !IsEOF())
            {
                Consume();
            }

            return CreateToken(Classifier.LineComment);
        }

        private Token ScanDec()
        {
            if (Ch == '.')
            {
                Consume();
            }

            while (IsDigit())
            {
                Consume();
            }

            if (Ch == 'f')
            {
                Consume();
            }

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF() && !IsNewLine())
            {
                if (IsLetter())
                {
                    return ScanWord("'{0}' is an invalid float value");
                }

                return ScanWord();
            }

            return CreateToken(Classifier.DecLiteral);
        }

        private Token ScanIdentifier()
        {
            while (IsIdentifier())
            {
                Consume();
            }

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF() && !IsNewLine())
            {
                return ScanWord();
            }

            if (IsBoolLiteral())
            {
                return CreateToken(Classifier.BoolLiteral);
            }

            switch (_builder.ToString())
            {
                case "and":
                    return CreateToken(Classifier.BooleanAnd);
                case "or":
                    return CreateToken(Classifier.BooleanOr);
                case "not":
                    return CreateToken(Classifier.Not);
                case "is":
                    return CreateToken(Classifier.Equal);
            }

            return CreateToken(IsKeyword() ? Classifier.Keyword : Classifier.Identifier);
        }

        private Token ScanInteger()
        {
            var i = 0;
            var idx = _index;

            //TODO: Support for hex and binary digits

            while (IsDigit())
            {
                Consume();
                i++;
            }

            if (Ch == '.')
            {
                return i > 0 ? ScanDec() : ScanWord("Literal can't start with .");
            }

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF() && !IsNewLine())
            {
                _index = idx;
                Clear();
                return ScanIdentifier();
            }

            return CreateToken(Classifier.IntLiteral);
        }

        private Token ScanNewLine()
        {
            Consume();

            DoNewLine();

            return CreateToken(Classifier.NewLine);
        }

        private Token ScanPunctuation()
        {
            switch (Ch)
            {
                case ':':
                    Consume();
                    if (Ch != ':') return CreateToken(Classifier.Colon);
                    Consume();
                    return CreateToken(Classifier.NullCondition);

                case '{':
                    Consume();
                    return CreateToken(Classifier.LeftBracket);

                case '}':
                    Consume();
                    return CreateToken(Classifier.RightBracket);

                case '[':
                    Consume();
                    return CreateToken(Classifier.LeftBrace);

                case ']':
                    Consume();
                    return CreateToken(Classifier.RightBrace);

                case '(':
                    Consume();
                    return CreateToken(Classifier.LeftParenthesis);

                case ')':
                    Consume();
                    return CreateToken(Classifier.RightParenthesis);

                case '>':
                    Consume();
                    switch (Ch)
                    {
                        case '=':
                            Consume();
                            return CreateToken(Classifier.GreaterThanOrEqual);
                        case '>':
                            Consume();
                            if (Ch == '=')
                            {
                                Consume();
                                return CreateToken(Classifier.BitShiftRightEqual);
                            }

                            return CreateToken(Classifier.BitShiftRight);
                        default:
                            return CreateToken(Classifier.GreaterThan);
                    }

                case '<':
                    Consume();
                    switch (Ch)
                    {
                        case '=':
                            Consume();
                            return CreateToken(Classifier.LessThanOrEqual);
                        case '<':
                            Consume();
                            if (Ch != '=') return CreateToken(Classifier.BitShiftLeft);
                            Consume();
                            return CreateToken(Classifier.BitShiftLeftEqual);
                        default:
                            return CreateToken(Classifier.LessThan);
                    }

                case '+':
                    Consume();
                    switch (Ch)
                    {
                        case '=':
                            Consume();
                            return CreateToken(Classifier.PlusEqual);
                        case '+':
                            Consume();
                            return CreateToken(Classifier.PlusPlus);
                        default:
                            return CreateToken(Classifier.Plus);
                    }

                case '-':
                    Consume();
                    switch (Ch)
                    {
                        case '=':
                            Consume();
                            return CreateToken(Classifier.MinusEqual);
                        case '>':
                            Consume();
                            return CreateToken(Classifier.Arrow);
                        case '-':
                            Consume();
                            return CreateToken(Classifier.MinusMinus);
                        default:
                            return CreateToken(Classifier.Minus);
                    }

                case '=':
                    Consume();
                    switch (Ch)
                    {
                        case '=':
                            Consume();
                            return CreateToken(Classifier.Equal);
                        case '>':
                            Consume();
                            return CreateToken(Classifier.FatArrow);
                        default:
                            return CreateToken(Classifier.Assignment);
                    }

                case '!':
                    Consume();
                    if (Ch != '=') return CreateToken(Classifier.Not);
                    Consume();
                    return CreateToken(Classifier.NotEqual);

                case '*':
                    Consume();
                    if (Ch != '=') return CreateToken(Classifier.Mul);
                    Consume();
                    return CreateToken(Classifier.MulEqual);

                case '/':
                    Consume();
                    if (Ch != '=') return CreateToken(Classifier.Div);
                    Consume();
                    return CreateToken(Classifier.DivEqual);

                case ',':
                    Consume();
                    return CreateToken(Classifier.Comma);

                case '&':
                    Consume();
                    switch (Ch)
                    {
                        case '&':
                            Consume();
                            return CreateToken(Classifier.BooleanAnd);
                        case '=':
                            Consume();
                            return CreateToken(Classifier.BitwiseAndEqual);
                        default:
                            return CreateToken(Classifier.BitwiseAnd);
                    }

                case '|':
                    Consume();
                    switch (Ch)
                    {
                        case '|':
                            Consume();
                            return CreateToken(Classifier.BooleanOr);
                        case '=':
                            Consume();
                            return CreateToken(Classifier.BitwiseOrEqual);
                        case '>':
                            Consume();
                            return CreateToken(Classifier.Pipeline);
                        default:
                            return CreateToken(Classifier.BitwiseOr);
                    }

                case '%':
                    Consume();
                    if (Ch != '=') return CreateToken(Classifier.Mod);
                    Consume();
                    return CreateToken(Classifier.ModEqual);

                case '^':
                    Consume();
                    if (Ch != '=') return CreateToken(Classifier.BitwiseXor);
                    Consume();
                    return CreateToken(Classifier.BitwiseXorEqual);

                case '~':
                    Consume();
                    if (Ch != '=') return CreateToken(Classifier.BitwiseNegate);
                    Consume();
                    return CreateToken(Classifier.BitwiseNegateEqual);

                case '@':
                    Consume();
                    return CreateToken(Classifier.At);

                case '#':
                    Consume();
                    return CreateToken(Classifier.Tag);

                case '?':
                    Consume();
                    if (Ch != '?') return CreateToken(Classifier.Question);
                    Consume();
                    return CreateToken(Classifier.DoubleQuestion);

                case '.':
                    Consume();
                    return CreateToken(Classifier.Dot);

                default: return ScanWord();
            }
        }

        private Token ScanStringLiteral()
        {
            Advance();

            var multiLine = Peek(0) == '"' && Peek(1) == '"';
            var hasError = false;

            if (multiLine)
            {
                _index++;
                _index++;
            }

            while (true)
            {
                if (IsEOF())
                {
                    throw new Exception("Unexpected End Of File");
                }

                if (IsNewLine() && !multiLine)
                {
                    throw new Exception("No newline in strings allowed!");
                }

                var consume = true;

                if (Ch == '\\' && Next == '"')
                {
                    Consume();
                    Consume();
                    consume = false;
                }

                if (Ch == '"')
                {
                    if (multiLine)
                    {
                        if (Peek(1) == '"' && Peek(2) == '"')
                        {
                            _index++;
                            _index++;

                            Advance();
                            return CreateToken(Classifier.MultiLineStringLiteral);
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (consume)
                {
                    Consume();
                }
            }

            Advance();

            return hasError ? CreateToken(Classifier.Error) : CreateToken(Classifier.StringLiteral);
        }

        private Token ScanWhiteSpace()
        {
            while (IsWhiteSpace() && Ch != '\0')
            {
                Consume();
            }

            return CreateToken(Classifier.WhiteSpace);
        }

        private Token ScanWord(string message = "Unexpected Token '{0}'")
        {
            while (!IsWhiteSpace() && !IsEOF() && !IsPunctuation())
            {
                Consume();
            }

            throw new Exception(string.Format(message, _builder));
        }

        #endregion
    }
}