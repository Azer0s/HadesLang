using Hades.Syntax.Expression.Nodes.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public class ValueCallNode : Node
    {
        public Node Source { get; set; }
        public IdentifierNode Target { get; set; }

        public ValueCallNode() : base(Classifier.ValueCall){}

        protected override string ToStr()
        {
            return $"Variable ({Target}) from ({Source})";
        }
    }
}