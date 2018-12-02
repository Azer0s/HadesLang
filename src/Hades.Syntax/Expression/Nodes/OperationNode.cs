using System.Collections.Generic;
using Hades.Syntax.Expression.Nodes.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public class OperationNode : Node
    {
        public List<Node> Operations { get; set; } = new List<Node>();
        
        public OperationNode() : base(Classifier.Operation){}

        protected override string ToStr()
        {
            var str = "";
            
            foreach (var operation in Operations)
            {
                switch (operation)
                {
                    case OperationNode _:
                        str += $"[{operation}]";
                        break;
                    case OperationNodeNode operationNode:
                        str += $" {operationNode.Representation} ";
                        break;
                    default:
                        str += $"({operation})";
                        break;
                }
            }
            
            return str;
        }
    }
}