namespace Hades.Syntax.Expression.Nodes
{
    public class InlineIf : Node
    {
        public Node Condition;
        public Node Truthy;
        public Node Falsy;

        public InlineIf() : base(Classifier.InlineIf)
        {
        }

        protected override string ToStr()
        {
            return $"(if ({Condition}), ({Truthy}), otherwise ({Falsy}) )";
        }
    }
}