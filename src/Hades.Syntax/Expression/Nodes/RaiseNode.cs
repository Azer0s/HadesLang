namespace Hades.Syntax.Expression.Nodes
{
    public class RaiseNode : Node
    {
        public RaiseNode() : base(Classifier.Exception)
        {
        }

        public Node Exception { get; set; }

        protected override string ToStr()
        {
            return $"Raise exception ({Exception})";
        }
    }
}