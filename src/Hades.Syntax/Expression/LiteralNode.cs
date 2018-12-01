namespace Hades.Syntax.Expression
{
    public abstract class LiteralNode<T> : Node
    {
        public T Value { get; set; }
        
        protected LiteralNode(Classifier classifier) : base(classifier)
        {
        }
        
        public override string ToString()
        {
            return $"Value: {Value}";
        }
    }
}