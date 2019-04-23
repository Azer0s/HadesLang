using Hades.Syntax.Expression.Nodes.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public enum Side
    {
        LEFT,
        RIGHT
    }

    public class SideNode : Node
    {
        private readonly Node BaseNode;
        private readonly OperationNodeNode Operation;
        private readonly Side Side;

        public SideNode(Node baseNode, OperationNodeNode operation, Side side) : base(side == Side.LEFT ? Classifier.LeftHand : Classifier.RightHand)
        {
            Side = side;
            BaseNode = baseNode;
            Operation = operation;
        }

        protected override string ToStr()
        {
            var side = Side == Side.LEFT ? "LeftHand" : "RightHand";
            return $"{side} {Operation.Representation} on ({BaseNode})";
        }
    }
}