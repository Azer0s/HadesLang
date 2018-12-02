using System.Collections.Generic;
using System.Linq;

namespace Hades.Syntax.Expression.Nodes
{
    public abstract class BlockNode : Node
    {
        public List<Node> Children { get; set; } = new List<Node>();
        
        protected override string ToStr()
        {
            var str = "";
            foreach (var child in Children)
            {
                str += string.Join('\n',child.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
            }

            return str;
        }

        protected BlockNode(Classifier classifier) : base(classifier){}
    }
}