using System.Collections.Generic;
using Hades.Common;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class FunctionNode : BlockNode
    {
        public FunctionNode() : base(Classifier.Function){}
        public bool Override { get; set; }
        public string Name { get; set; }
        public Dictionary<Node, Datatype?> Parameters { get; } = new Dictionary<Node, Datatype?>();
        public Node Guard { get; set; }

        protected override string ToStr()
        {
            var args = "";
            
            foreach (var parameter in Parameters)
            {
                if (parameter.Value != null)
                {
                    args += $"(({parameter.Key}):{parameter.Value.ToString().ToLower()}),";
                }
                else
                {
                    args += $"({parameter}),";
                }
            }

            if (!string.IsNullOrEmpty(args))
            {
                args = args.Substring(0, args.Length - 1);
                args = $" with parameters {args}";
            }

            var guard = Guard != null ? " with guard (" + Guard + ")" : "";
            var over = Override ? " override " : "";
            var str = $"{Name}{over}{args}{guard}\n{base.ToStr()}";
            return str;
        }
    }
}