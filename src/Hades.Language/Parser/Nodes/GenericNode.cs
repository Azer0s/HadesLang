using Hades.Language.Lexer;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser.Nodes
{
    public class GenericNode : AstNode
    {
        public Token Value { get; set; }
        
        public GenericNode(Token value) : base(value.Type)
        {
            Value = value;
        }

        protected override string DoToString()
        {
            return Value.Value;
        }
    }
}