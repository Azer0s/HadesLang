namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class WhileNode : BlockNode
    {
        public Node Condition { get; set; }
        
        public WhileNode() : base(Classifier.While){}

        protected override string ToStr()
        {
            return $"Condition: ({Condition})\n{base.ToStr()}";
        }
    }
}