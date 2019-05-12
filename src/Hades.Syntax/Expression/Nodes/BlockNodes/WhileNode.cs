namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class WhileNode : BlockNode
    {
        public WhileNode() : base(Classifier.While)
        {
        }

        public Node Condition { get; set; }

        protected override string ToStr()
        {
            return $"Condition: ({Condition})\n{base.ToStr()}";
        }
    }
}