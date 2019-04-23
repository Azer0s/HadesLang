using System.Collections.Generic;
using Hades.Common;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class LambdaNode : BlockNode
    {
        public LambdaNode() : base(Classifier.Lambda)
        {
        }

        public List<(Node Key, Datatype? Value)> Parameters { get; } = new List<(Node Key, Datatype? Value)>();
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