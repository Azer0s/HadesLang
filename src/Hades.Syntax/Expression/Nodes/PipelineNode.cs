namespace Hades.Syntax.Expression.Nodes
{
    public class PipelineNode : Node
    {
        public Node Destination;
        public Node Source;

        public PipelineNode() : base(Classifier.Pipeline)
        {
        }

        protected override string ToStr()
        {
            return "";
        }
    }
}