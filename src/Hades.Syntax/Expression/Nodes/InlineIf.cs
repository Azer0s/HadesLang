namespace Hades.Syntax.Expression.Nodes
{
    public class InlineIf : Node
    {
        public Node Condition;
        public Node Falsy;
        public Node Truthy;

        public InlineIf() : base(Classifier.InlineIf)
        {
        }

        protected override string ToStr()
        {
            return $"(if ({Condition}), ({Truthy}), otherwise ({Falsy}))";
        }
    }
}