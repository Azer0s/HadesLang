using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser.Nodes
{
    public class VariableDeclarationNode : AstNode
    {
        public bool IsConstant { get; set; }
        public AstNode Datatype { get; set; }
        public bool IsArray { get; set; }
        public AstNode ArraySize { get; set; }
        public bool Nullable { get; set; }
        public IdentifierNode Name { get; set; }

        public VariableDeclarationNode() : base(Type.AstVariableDeclaration)
        {
        }

        protected override string DoToString()
        {
            throw new System.NotImplementedException();
        }
    }
}