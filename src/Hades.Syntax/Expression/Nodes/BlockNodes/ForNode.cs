namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class ForNode : BlockNode
    {
        public Node Variable { get; set; }
        public Node Source { get; set; }
        
        public ForNode() : base(Classifier.For){}

        protected override string ToStr()
        {
            return $"Source: ({Source}) into Variable: ({Variable})\n{base.ToStr()}";
        }
    }
}