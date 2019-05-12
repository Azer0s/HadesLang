using System.Collections.Generic;
using System.Linq;

namespace Hades.Syntax.Expression
{
    public abstract class BlockNode : Node
    {
        protected BlockNode(Classifier classifier) : base(classifier)
        {
        }

        public List<Node> Children { get; set; } = new List<Node>();

        protected override string ToStr()
        {
            var str = "";
            foreach (var child in Children)
            {
                str += string.Join('\n', child.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
            }

            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(0, str.Length - 1);
            }

            return str;
        }
    }
}