using System.Collections.Generic;

namespace Hades.Syntax.Expression.Nodes
{
    public class RootNode : Node
    {
        public RootNode() : base(Classifier.Root){}
        
        public List<Node> Children { get; } = new List<Node>();

        protected override string ToStr(){return string.Empty;}

        public override string ToString()
        {
            var str = "ROOT\n";
            foreach (var child in Children)
            {
                str += $"  {child}\n";
            }

            return str;
        }
    }
}