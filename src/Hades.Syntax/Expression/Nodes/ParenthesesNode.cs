namespace Hades.Syntax.Expression.Nodes
{
    public class ParenthesesNode : Node
    {
        public Node Node { get; set; }
        
        public ParenthesesNode() : base(Classifier.Misc)
        {
        }

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