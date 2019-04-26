using System;
using Hades.Common.Source;

namespace Hades.Syntax.Lexeme
{
    public sealed class Token : IEquatable<Token>
    {
        private readonly Lazy<Category> _category;

        public Token(Classifier kind, string contents, SourceLocation start, SourceLocation end)
        {
            Kind = kind;
            Value = contents;
            Span = new Span(start, end);

            _category = new Lazy<Category>(GetTokenCategory);
        }

        public Category Category => _category.Value;

        public Classifier Kind { get; }

        public Span Span { get; }

        public string Value { get; }

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
                case Classifier.Dot:
                    return Category.Punctuation;

                case Classifier.Equal:
                case Classifier.NotEqual:
                case Classifier.LessThan:
                case Classifier.LessThanOrEqual:
                case Classifier.GreaterThan:
                case Classifier.GreaterThanOrEqual:
                case Classifier.Mod:
                case Classifier.Mul:
                case Classifier.Plus:
                case Classifier.Div:
                case Classifier.BooleanOr:
                case Classifier.BooleanAnd:
                case Classifier.BitwiseXor:
                case Classifier.BitwiseOr:
                case Classifier.BitwiseAnd:
                case Classifier.BitShiftLeft:
                case Classifier.BitShiftRight:
                case Classifier.BitwiseNegate:
                case Classifier.Minus:
                case Classifier.Not:
                    return Category.Operator;

                case Classifier.MinusMinus:
                case Classifier.PlusPlus:
                    return Category.RightHand;

                case Classifier.Assignment:
                case Classifier.MulEqual:
                case Classifier.MinusEqual:
                case Classifier.ModEqual:
                case Classifier.PlusEqual:
                case Classifier.BitwiseXorEqual:
                case Classifier.BitwiseOrEqual:
                case Classifier.BitwiseAndEqual:
                case Classifier.BitShiftLeftEqual:
                case Classifier.BitShiftRightEqual:
                case Classifier.BitwiseNegateEqual:
                case Classifier.DivEqual:
                    return Category.Assignment;

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

                case Classifier.Pipeline:
                case Classifier.Tag:
                case Classifier.Question:
                    return Category.Other;


                default: return Category.Unknown;
            }
        }

        public override string ToString()
        {
            return $"{Kind} : {Value}";
        }
    }
}