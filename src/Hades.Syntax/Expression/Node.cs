namespace Hades.Syntax.Expression
{
    public abstract class Node
    {
        public Classifier Classifier { get;  }
        public Node Child { set; get; }

        protected abstract string ToStr();
        
        public override string ToString()
        {
            return $"{GetType().Name.Replace("Node", "")} => {ToStr()}";
        }

        protected Node(Classifier classifier)
        {
            Classifier = classifier;
        }
    }
}