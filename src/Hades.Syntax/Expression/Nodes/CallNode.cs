using System.Collections.Generic;
using Hades.Syntax.Expression.Nodes.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public class CallNode : Node
    {
        public CallNode() : base(Classifier.Call)
        {
        }

        public Node Source { get; set; }
        public IdentifierNode Target { get; set; }
        public Dictionary<Node, string> Parameters { get; } = new Dictionary<Node, string>();

        protected override string ToStr()
        {
            var str = "";

            foreach (var keyValuePair in Parameters)
            {
                if (!string.IsNullOrEmpty(keyValuePair.Value))
                {
                    str += $"({keyValuePair.Value}=";
                }
                else
                {
                    str += "(";
                }

                str += $"{keyValuePair.Key}),";
            }

            if (!string.IsNullOrEmpty(str))
            {
                str = str.Substring(0, str.Length - 1);
                str = $" with parameters {str}";
            }

            return $"{Target.Value} on ({Source}){str}";
        }
    }
}