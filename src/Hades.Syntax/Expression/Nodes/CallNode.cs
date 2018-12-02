using System.Collections.Generic;
using Hades.Syntax.Expression.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public class CallNode : Node
    {
        public Node Source { get; set; }
        public IdentifierNode Target { get; set; }
        public Dictionary<Node,string> Parameters { get; } = new Dictionary<Node, string>();
        
        public CallNode() : base(Classifier.Call){}

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
            
            str = str.Substring(0, str.Length - 1);
            return $"{Target.Value} on ({Source}) with parameters {str}";
        }
    }
}