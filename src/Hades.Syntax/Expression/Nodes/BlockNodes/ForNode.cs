namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class ForNode : BlockNode
    {
        public ForNode() : base(Classifier.For)
        {
        }

        public Node Variable { get; set; }
        public Node Source { get; set; }

        protected override string ToStr()
        {
            return $"Source: ({Source}) into Variable: ({Variable})\n{base.ToStr()}";
        }
    }
}