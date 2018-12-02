namespace Hades.Syntax.Expression.Nodes
{
    public class RaiseNode : Node
    {
        public Node Exception { get; set; }
        
        public RaiseNode() : base(Classifier.Exception)
        {
        }

        protected override string ToStr()
        {
            return $"Raise exception ({Exception})";
        }
    }
}