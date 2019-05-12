using System.Collections.Generic;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class VarBlockNode : Node
    {
        public AccessModifier AccessModifier { get; set; }
        public List<VariableDeclarationNode> VariableDeclarationNodes { get; } = new List<VariableDeclarationNode>();
        
        public VarBlockNode() : base(Classifier.Misc)
        {
        }

        protected override string ToStr()
        {
            return "";
        }
    }
}