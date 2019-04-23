namespace Hades.Syntax.Expression.Nodes
{
    public class PutNode : Node
    {
        public PutNode() : base(Classifier.Put)
        {
        }

        public Node Statement { get; set; }

        protected override string ToStr()
        {
            return $"Statement: ({Statement})";
        }
    }
}