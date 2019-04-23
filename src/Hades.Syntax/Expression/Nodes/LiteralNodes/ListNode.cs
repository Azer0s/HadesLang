using System.Collections.Generic;
using System.Linq;

namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class ListNode : LiteralNode<List<Node>>
    {
        public ListNode() : base(Expression.Classifier.ListLiteral)
        {
        }
        
        public override string ToString()
        {
            return $"Value: {{{string.Join(",", Value.Select(a => $"({a})").ToList())}}}";
        }
    }
}