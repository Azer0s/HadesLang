using Hades.Common;

namespace Hades.Syntax
{
    public abstract class Node
    {
        public Classifier Classifier { get;  }
        public new abstract string ToString();

        protected Node(Classifier classifier)
        {
            Classifier = classifier;
        }
    }
}