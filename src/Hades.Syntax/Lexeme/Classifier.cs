namespace Hades.Syntax.Lexeme
{
    public enum Classifier
    {
        EndOfFile,
        Error,

        #region WhiteSpace

        WhiteSpace,
        NewLine,

        #endregion

        #region Comments

        LineComment,
        BlockComment,

        #endregion

        #region Literal

        IntLiteral,
        MultiLineStringLiteral,
        StringLiteral,
        DecLiteral,
        BoolLiteral,

        #endregion

        #region Identifiers

        Identifier,
        Keyword,

        #endregion

        #region Groupings

        LeftBracket, // {
        RightBracket, // }
        RightBrace, // ]
        LeftBrace, // [
        LeftParenthesis, // (
        RightParenthesis, // )

        #endregion Groupings

        #region Operators

        GreaterThanOrEqual, //>=
        GreaterThan, //>

        LessThanOrEqual, //<=
        LessThan, //<

        PlusEqual, //+=
        PlusPlus, //++
        Plus, //+

        MinusEqual, // -=
        MinusMinus, // --
        Minus, // -

        Assignment, // =

        Not, // !
        NotEqual, // !=

        Mul, // *
        MulEqual, // *=

        Div, // /
        DivEqual, // /=

        BooleanAnd, // &&
        BooleanOr, // ||

        BitwiseAnd, // &
        BitwiseOr, // |

        BitwiseAndEqual, // &=
        BitwiseOrEqual, // |=

        ModEqual, // %=
        Mod, // %

        BitwiseXorEqual, // ^=
        BitwiseXor, // ^

        BitwiseNegate, //~
        BitwiseNegateEqual, //~=

        Equal, //==

        BitShiftLeft, // <<
        BitShiftRight, // >>
        BitShiftLeftEqual, // <<=
        BitShiftRightEqual, // >>=

        Question, //?
        NullCondition, //::

        Pipeline, //|>
        Tag, //#

        #endregion

        #region Punctuation

        Comma, //,
        Colon, //:
        Arrow, // ->
        FatArrow, // =>
        Underscore, //_
        At, //@
        Dot, //.

        #endregion Punctuation 
    }
}