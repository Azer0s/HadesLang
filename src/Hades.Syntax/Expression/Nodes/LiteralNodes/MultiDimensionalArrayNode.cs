using System.Collections.Generic;

namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class MultiDimensionalArrayNode : LiteralNode<List<Node>>
    {
        public MultiDimensionalArrayNode() : base(Classifier.MultiDimensionalArrayAccess)
        {
            Value = new List<Node>();
        }

        public override string ToString()
        {
            return string.Join(",", Value);
        }
    }
}