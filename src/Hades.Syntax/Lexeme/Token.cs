using System;
using Hades.Common;
using Hades.Common.Source;
using Hades.Language.Lexer;

namespace Hades.Syntax.Lexeme
{
    public sealed class Token : IEquatable<Token>
    {
        private readonly Lazy<Category> _category;

        public Category Category => _category.Value;

        public Classifier Kind { get; }

        public Span Span { get; }

        public string Value { get; }

        public Token(Classifier kind, string contents, SourceLocation start, SourceLocation end)
        {
            Kind = kind;
            Value = contents;
            Span = new Span(start, end);

            _category = new Lazy<Category>(GetTokenCategory);
        }

        public static bool operator !=(Token left, string right)
        {
            return left?.Value != right;
        }

        public static bool operator !=(string left, Token right)
        {
            return right?.Value != left;
        }

        public static bool operator !=(Token left, Classifier right)
        {
            return left?.Kind != right;
        }

        public static bool operator !=(Classifier left, Token right)
        {
            return right?.Kind != left;
        }

        public static bool operator ==(Token left, string right)
        {
            return left?.Value == right;
        }

        public static bool operator ==(string left, Token right)
        {
            return right?.Value == left;
        }

        public static bool operator ==(Token left, Classifier right)
        {
            return left?.Kind == right;
        }

        public static bool operator ==(Classifier left, Token right)
        {
            return right?.Kind == left;
        }

        public override bool Equals(object obj)
        {
            if (obj is Token token)
            {
                return Equals(token);
            }
            return base.Equals(obj);
        }

        public bool Equals(Token other)
        {
            if (other == null)
            {
                return false;
            }
            return other.Value == Value &&
                   other.Span == Span &&
                   other.Kind == Kind;
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode() ^ Span.GetHashCode() ^ Kind.GetHashCode();
        }

        public bool IsTrivia()
        {
            return Category == Category.WhiteSpace || Category == Category.Comment;
        }

        private Category GetTokenCategory()
        {
            switch (Kind)
            {
                case Classifier.Arrow:
                case Classifier.FatArrow:
                case Classifier.Colon:
                case Classifier.Comma:
                case Classifier.Underscore:
                case Classifier.At:
                    return Category.Punctuation;

                case Classifier.Equal:
                case Classifier.NotEqual:
                case Classifier.Not:
                case Classifier.LessThan:
                case Classifier.LessThanOrEqual:
                case Classifier.GreaterThan:
                case Classifier.GreaterThanOrEqual:
                case Classifier.Minus:
                case Classifier.MinusEqual:
                case Classifier.MinusMinus:
                case Classifier.Mod:
                case Classifier.ModEqual:
                case Classifier.Mul:
                case Classifier.MulEqual:
                case Classifier.Plus:
                case Classifier.PlusEqual:
                case Classifier.PlusPlus:
                case Classifier.Question:
                case Classifier.DivEqual:
                case Classifier.Div:
                case Classifier.BooleanOr:
                case Classifier.BooleanAnd:
                case Classifier.BitwiseXorEqual:
                case Classifier.BitwiseXor:
                case Classifier.BitwiseOrEqual:
                case Classifier.BitwiseOr:
                case Classifier.BitwiseAndEqual:
                case Classifier.BitwiseAnd:
                case Classifier.BitShiftLeft:
                case Classifier.BitShiftRight:
                case Classifier.BitShiftLeftEqual:
                case Classifier.BitShiftRightEqual:
                case Classifier.BitwiseNegate:
                case Classifier.BitwiseNegateEqual:
                case Classifier.Assignment:
                case Classifier.NullCondition:
                case Classifier.Pipeline:
                case Classifier.Tag:
                    return Category.Operator;

                case Classifier.BlockComment:
                case Classifier.LineComment:
                    return Category.Comment;

                case Classifier.NewLine:
                case Classifier.WhiteSpace:
                    return Category.WhiteSpace;

                case Classifier.LeftBrace:
                case Classifier.LeftBracket:
                case Classifier.LeftParenthesis:
                case Classifier.RightBrace:
                case Classifier.RightBracket:
                case Classifier.RightParenthesis:
                    return Category.Grouping;

                case Classifier.Identifier:
                case Classifier.Keyword:
                    return Category.Identifier;

                case Classifier.StringLiteral:
                case Classifier.DecLiteral:
                case Classifier.IntLiteral:
                case Classifier.BoolLiteral:
                case Classifier.MultiLineStringLiteral:
                    return Category.Literal;

                case Classifier.Error:
                    return Category.Invalid;
                
                default: return Category.Unknown;
            }
        }
    }
}