namespace Hades.Syntax.Expression.Nodes
{
    public class PipelineNode : Node
    {
        public Node Source;
        public Node Destination;
        
        public PipelineNode() : base(Classifier.Pipeline)
        {
        }

        protected override string ToStr()
        {
            return "";
        }
    }
}