// ReSharper disable once CheckNamespace

using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hades.Common;
using Hades.Error;
using Hades.Source;

namespace Hades.Language.Lexer
{
    public class Lexer
    {
        #region Keywords

        private static readonly string[] BlockKeywords =
        {
            "class",
            "func",
            "args",
            "requires",
            "if",
            "else",
            "while",
            "for",
            "in",
            "stop",
            "skip",
            "try",
            "catch",
            "default",
            "end"
        };

        private static readonly string[] VarKeywords =
        {
            "var",
            "let",
            "null",
            "undefined"
        };

        private static readonly string[] AccessModifierKeywords =
        {
            "global",
            "public",
            "private"
        };

        private static readonly string[] ComparisonKeywords =
        {
            "is",
            "not",
            "and",
            "or" 
        };
        
        private static readonly string[] ImportKeywords =
        {
            "with",
            "from",
            "as",
            "sets"
        };

        private static readonly string[] MiscKeywords =
        {
            "put"
        };
        
        private static List<string> _keywordList = new List<string>();

        private static List<string> GetKeywordList()
        {
            if (_keywordList.Count != 0) return _keywordList;
            
            var list = BlockKeywords.ToList();
            list.AddRange(VarKeywords.ToList());
            list.AddRange(AccessModifierKeywords.ToList());
            list.AddRange(ComparisonKeywords.ToList());
            list.AddRange(ImportKeywords.ToList());
            list.AddRange(MiscKeywords.ToList());

            _keywordList = list;
            return _keywordList;
        }

        public static List<string> Keywords => GetKeywordList();

        #endregion

        private readonly StringBuilder _builder;
        private int _column;
        private int _index;
        private int _line;
        private SourceCode _sourceCode;
        private SourceLocation _tokenStart;
        private char Ch => _sourceCode[_index];
        // ReSharper disable once UnusedMember.Local
        private char Last => Peek(-1);
        private char Next => Peek(1);
        
        public Collector Collector { get; }
        
        public Lexer(): this(new Collector()){}

        public Lexer(Collector collector)
        {
            _builder = new StringBuilder();
            _sourceCode = null;
            Collector = collector;
        }

        public IEnumerable<Token> LexFile(string sourceCode) => LexFile(new SourceCode(sourceCode));
        
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
        
        private void AddError(string message, Severity severity)
        {
            var span = new Span(_tokenStart, new SourceLocation(_index, _line, _column));
            Collector.Add(message, _sourceCode, severity, span);
        }

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
            return "<>{}()[]!%^&*+-=/,?:|~#".Contains(Ch);
        }

        private bool IsWhiteSpace()
        {
            return (char.IsWhiteSpace(Ch) || IsEOF()) && !IsNewLine();
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

            if (IsLetter() || Ch == '_')
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
            bool IsEndOfComment() => Ch == '*' && Next == '/';
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

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF())
            {
                if (IsLetter())
                {
                    return ScanWord(message: "'{0}' is an invalid float value");
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

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF())
            {
                return ScanWord();
            }

            return CreateToken(IsKeyword() ? Classifier.Keyword : Classifier.Identifier);
        }

        private Token ScanInteger()
        {
            var i = 0;
            
            while (IsDigit())
            {
                Consume();
                i++;
            }

            if (Ch == '.')
            {
                return i > 0 ? ScanDec() : ScanWord(message:"Literal can't start with .");
            }

            if (!IsWhiteSpace() && !IsPunctuation() && !IsEOF())
            {
                return ScanWord();
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

                    return CreateToken(Classifier.Question);

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
                    AddError("Unexpected End Of File", Severity.Fatal);
                    return CreateToken(Classifier.Error);
                }

                if (IsNewLine() && !multiLine)
                {
                    AddError("No newline in strings allowed!", Severity.Fatal);
                    hasError = true;
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
            while (IsWhiteSpace())
            {
                Consume();
            }
            return CreateToken(Classifier.WhiteSpace);
        }

        private Token ScanWord(Severity severity = Severity.Error, string message = "Unexpected Token '{0}'")
        {
            while (!IsWhiteSpace() && !IsEOF() && !IsPunctuation())
            {
                Consume();
            }
            AddError(string.Format(message, _builder.ToString()), severity);
            return CreateToken(Classifier.Error);
        }
    }
}