namespace Hades.Syntax.Expression.Nodes
{
    public class AssignmentNode : Node
    {
        public Node Variable { get; set; }
        public Node Value { get; set; }
        
        public AssignmentNode() : base(Classifier.Assignment){}

        protected override string ToStr()
        {
            return $"({Value}) to ({Variable})";
        }
    }
}