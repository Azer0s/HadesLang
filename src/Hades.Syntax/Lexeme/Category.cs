namespace Hades.Language.Lexer
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
    }
}