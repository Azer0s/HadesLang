namespace Hades.Language.Lexer
{
    public enum Type
    {
        Identifier,

        #region Literal

        Integer,
        Float,
        Bool,
        Atom,
        String,

        #endregion

        #region Punctuation

        Dot,
        Comma,
        Semicolon,

        #endregion

        #region Brackets

        OpenParentheses,
        ClosedParentheses,
        OpenBrace,
        ClosedBrace,
        OpenBracket,
        ClosedBracket,

        #endregion

        #region Keywords

        Class,
        Func,
        Args,
        Requires,
        If,
        Else,
        While,
        For,
        In,
        Stop,
        Skip,
        Try,
        Catch,
        End,
        Var,
        Let,
        Null,
        Protected,
        Public,
        Private,
        With,
        From,
        As,
        Sets,
        Put,
        Raise,
        Fixed,
        Match,
        Struct,
        Receive,

        #endregion

        #region Operators

        Equals,
        NotEquals,
        LogicalAnd,
        LogicalOr,
        Smaller,
        Bigger,
        
        Increment,
        Decrement,
        
        CompoundIncrement,
        CompoundDecrement,
        CompoundMultiplication,
        CompoundDivision,
        CompoundMod,
        CompoundLeftShift,
        CompoundRightShift,
        CompoundBitwiseAnd,
        CompoundBitwiseOr,
        CompoundBitwiseXor,
        
        Plus,
        Minus,
        Multiplication,
        Division,
        Mod,
        LeftShift,
        RightShift,
        BitwiseAnd,
        BitwiseOr,
        BitwiseXor,
        BitwiseNegate,
        
        MatchAssign,
        Assign,
        
        FatArrow,
        
        ExclamationMark,
        Nullable,
        Pipeline,
        PipelinePlaceholder

        #endregion
    }
}