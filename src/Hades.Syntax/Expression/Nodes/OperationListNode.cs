using System.Collections.Generic;
using Hades.Syntax.Expression.LiteralNodes;

namespace Hades.Syntax.Expression.Nodes
{
    public class OperationListNode : Node
    {
        public List<Node> Operations { get; set; } = new List<Node>();
        
        public OperationListNode() : base(Classifier.Operation){}

        protected override string ToStr()
        {
            var str = "";
            
            foreach (var operation in Operations)
            {
                switch (operation)
                {
                    case OperationListNode _:
                        str += $"[{operation}]";
                        break;
                    case OperationNode operationNode:
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