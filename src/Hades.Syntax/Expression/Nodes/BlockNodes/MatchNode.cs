using System.Collections.Generic;
using System.Linq;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class MatchNode : Node
    {
        public MatchNode() : base(Classifier.Match)
        {
        }

        public Node Match { get; set; }
        public bool First { get; set; }
        public Dictionary<Node, Node> Statements { get; set; } = new Dictionary<Node, Node>();

        protected override string ToStr()
        {
            var str = Match == null ? "any" : $"({Match})";

            var functions = Statements.Select(a => $"* ({a.Key})" + " => " + a.Value.ToString().Replace("\n", "\n    ")).ToList();
            var fn = string.Empty;

            if (functions.Count != 0)
            {
                fn = $"\n  {string.Join("\n  ", functions)}";
            }

            var first = First ? " first" : "";
            return str + first + " to " + fn;
        }
    }
}