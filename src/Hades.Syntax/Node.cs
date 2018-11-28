using Hades.Common;

namespace Hades.Syntax
{
    public abstract class Node
    {
        public Classifier Classifier { get;  }
        public Node Child { set; get; }

        public abstract override string ToString();

        protected Node(Classifier classifier)
        {
            Classifier = classifier;
        }
    }
}