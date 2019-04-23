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
        Other,
        Assignment,
        LeftHand,
        RightHand
    }
}