namespace Hades.Syntax.Expression.Nodes
{
    public class AssignmentNode : Node
    {
        public AssignmentNode() : base(Classifier.Assignment)
        {
        }

        public Node Variable { get; set; }
        public Node Value { get; set; }

        protected override string ToStr()
        {
            return $"({Value}) to ({Variable})";
        }
    }
}