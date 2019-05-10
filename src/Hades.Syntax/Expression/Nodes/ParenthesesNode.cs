namespace Hades.Syntax.Expression.Nodes
{
    public class ParenthesesNode : Node
    {
        public ParenthesesNode() : base(Classifier.Misc)
        {
        }

        public Node Node { get; set; }

        protected override string ToStr()
        {
            return "";
        }

        public override string ToString()
        {
            return $"[{Node}]";
        }
    }
}