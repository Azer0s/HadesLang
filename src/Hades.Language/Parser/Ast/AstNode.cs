using System;
using System.Collections.Generic;
using Hades.Language.Lexer;
using Hades.Language.Parser.Nodes;

namespace Hades.Language.Parser.Ast
{
    public abstract class AstNode
    {
        public Type Type { get; }
        public bool Matches = true;

        protected AstNode(Type type)
        {
            Type = type;
        }

        protected abstract string DoToString();

        public override string ToString()
        {
            return $"[{Type.ToString().ToUpper().Replace("AST","") + "NODE"}] {DoToString()}";
        }

        public static AstNode Parse(Func<AstNode> statement)
        {
            var result = statement();
            return result ?? new UnmatchedNode();
        }

        public AstNode Or(Func<AstNode> statement)
        {
            return Matches ? this : statement();
        }
    }
}