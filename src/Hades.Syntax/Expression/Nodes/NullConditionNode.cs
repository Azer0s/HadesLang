namespace Hades.Syntax.Expression.Nodes
{
    public class NullConditionNode : Node
    {
        public Node Condition { get; set; }
        public Node Operation { get; set; }
        
        public NullConditionNode() : base(Classifier.NullCondition){}

        protected override string ToStr()
        {
            return $"Return ({Operation}) if ({Condition}) evaluates to null";
        }
    }
}