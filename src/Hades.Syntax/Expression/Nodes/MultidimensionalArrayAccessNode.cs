using System.Collections.Generic;
using System.Linq;

namespace Hades.Syntax.Expression.Nodes
{
    public class MultidimensionalArrayAccessNode : Node
    {
        public readonly List<int> Dimensions;

        public MultidimensionalArrayAccessNode(string values) : base(Classifier.MultidimensionalArrayAccess)
        {
            Dimensions = values.Split('.').Select(a => int.Parse(a)).ToList();
        }

        protected override string ToStr()
        {
            return string.Empty;
        }

        public override string ToString()
        {
            return $"Multidimensional: {string.Join('x', Dimensions)}";
        }
    }
}