using Hades.Language.Lexer;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser.Nodes
{
    public class IdentifierNode : AstNode
    {
        public Token Identifier;
        
        public IdentifierNode(Token identifier) : base(Type.AstIdentifier)
        {
            Identifier = identifier;
        }

        protected override string DoToString()
        {
            throw new System.NotImplementedException();
        }
    }
}