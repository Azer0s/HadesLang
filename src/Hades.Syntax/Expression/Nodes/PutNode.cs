namespace Hades.Syntax.Expression.Nodes
{
    public class PutNode : Node
    {
        public Node Statement { get; set; }
        
        public PutNode() : base(Classifier.Put){}

        protected override string ToStr()
        {
            return $"Statement: ({Statement})";
        }
    }
}