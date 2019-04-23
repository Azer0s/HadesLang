using System.Collections.Generic;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class LambdaNode : BlockNode
    {
        public LambdaNode() : base(Classifier.Lambda)
        {
        }

        public List<Node> Parameters { get; } = new List<Node>();
        public bool Complex { get; set; }

        protected override string ToStr()
        {
            var args = "";

            foreach (var parameter in Parameters)
            {
                args += $"({parameter}),";
            }

            if (!string.IsNullOrEmpty(args))
            {
                args = args.Substring(0, args.Length - 1);
                args = $" with parameters {args}";
            }

            var complex = Complex ? "Complex" : "Simple";
            var str = $"{complex} lambda{args}\n{base.ToStr()}";
            return str;
        }
    }
}