using Hades.Syntax.Expression.Nodes.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public enum Side
    {
        LEFT,
        RIGHT
    }
    
    public class RightHandNode : Node
    {
        public Node BaseNode;
        public OperationNodeNode Operation;
        public Side side;
        
        public RightHandNode() : base(Classifier.RightHand){}

        protected override string ToStr()
        {
            return $"{Operation.Representation} on ({BaseNode})";
        }
    }
}