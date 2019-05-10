namespace Hades.Syntax.Expression.Nodes
{
    public class ArrayAccessNode : Node
    {
        public Node Index { get; set; }
        public Node BaseNode { get; set; }
        
        public ArrayAccessNode() : base(Classifier.ArrayAccess)
        {
        }

        protected override string ToStr()
        {
            return $"Index [{Index}] from ({BaseNode})";
        }
    }
}