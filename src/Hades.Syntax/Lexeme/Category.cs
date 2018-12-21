namespace Hades.Syntax.Lexeme
{
    public enum Category
    {
        Unknown,
        WhiteSpace,
        Comment,

        Literal,
        Identifier,
        Grouping,
        Punctuation,
        Operator,

        Invalid,
        LeftHand,
        Other,
        Assignment,
        RightHand
    }
}