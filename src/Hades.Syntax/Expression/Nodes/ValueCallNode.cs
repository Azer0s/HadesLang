using Hades.Syntax.Expression.Nodes.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public class ValueCallNode : Node
    {
        public ValueCallNode() : base(Classifier.ValueCall)
        {
        }

        public Node Source { get; set; }
        public IdentifierNode Target { get; set; }

        protected override string ToStr()
        {
            return $"Variable ({Target}) from ({Source})";
        }
    }
}