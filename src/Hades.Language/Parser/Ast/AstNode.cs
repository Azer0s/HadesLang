using System.Collections.Generic;
using Hades.Language.Lexer;

namespace Hades.Language.Parser.Ast
{
    public abstract class AstNode
    {
        public virtual Type Type { get; }
        public List<Token> Tokens;

        protected AstNode(Type type)
        {
            Type = type;
        }

        protected abstract string DoToString();

        public override string ToString()
        {
            return $"[{Type.ToString().ToUpper()}] {DoToString()}";
        }
    }
}