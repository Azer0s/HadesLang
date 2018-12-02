using System.Collections.Generic;
using System.Linq;

namespace Hades.Syntax.Expression.Nodes
{
    public class RootNode : BlockNode
    {
        public RootNode() : base(Classifier.Root){}

        public override string ToString()
        {
            return $"ROOT\n{base.ToStr()}";
        }
    }
}