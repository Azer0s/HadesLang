using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class IfNode : Node
    {
        public IfNode() : base(Classifier.If)
        {
        }

        public Node Condition { get; set; }
        public BlockNode If { get; set; }
        public List<IfNode> ElseIfNodes { get; } = new List<IfNode>();
        public BlockNode Else { get; set; }

        protected override string ToStr()
        {
            var str = "";
            foreach (var child in If.Children)
            {
                str += string.Join('\n', child.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
            }

            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(0, str.Length - 1);
            }

            var elseIfStr = string.Empty;
            
            foreach (var elseIfNode in ElseIfNodes)
            {
                elseIfStr += $"\nElse {elseIfNode}";
            }

            var elseStr = string.Empty;

            if (Else != null)
            {
                foreach (var elseChild in Else.Children)
                {
                    elseStr += string.Join('\n', elseChild.ToString().Split('\n').Select(a => $"  {a}")) + "\n";
                }
            }

            if (!string.IsNullOrEmpty(elseStr))
            {
                elseStr = $"\nElse => \n{elseStr.Substring(0, elseStr.Length - 1)}";
            }
            
            return $"Condition: ({Condition})\n{str}{elseIfStr}{elseStr}";
        }
    }
}