using System.Collections.Generic;

namespace Hades.Syntax.Expression
{
    public abstract class Node
    {
        protected Node(Classifier classifier)
        {
            Classifier = classifier;
        }

        public Classifier Classifier { get; }
        public Dictionary<string, Node> Annotations { get; } = new Dictionary<string, Node>();

        protected abstract string ToStr();

        public override string ToString()
        {
            return $"{GetType().Name.Replace("Node", "")} => {ToStr()}";
        }
    }
}