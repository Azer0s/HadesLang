using System.Collections.Generic;

namespace Hades.Language
{
    public static class Datatypes
    {
        private const string Int = "int";
        private const string Dec = "dec";
        private const string Bool = "bool";
        private const string String = "string";
        private const string Object = "object";
        private const string Proto = "proto";
        private const string Lambda = "lambda";

        public static readonly List<string> All = new List<string>{Int, Dec, Bool, String, Object, Proto, Lambda};
    }
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
        PipelinePlaceholder,

        #endregion

        #region Ast

        AstRoot,
        AstDatatype,
        AstVariableDeclaration,
        AstArraySize,
        AstIdentifier,
        AstUnmatched,
        AstStatement,
        
        #endregion

        #region Parser Symbols

        PARSER_DONE,
        PARSER_DONE_SUBNODE

        #endregion
    }
}