using System.Collections.Generic;
using Hades.Common;
using Hades.Syntax.Expression.Nodes.BlockNodes.Util;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class LambdaNode : BlockNode
    {
        public LambdaNode() : base(Classifier.Lambda)
        {
        }

        public List<(Node Key, Datatype? Value, string SpecificType)> Parameters { get; } = new List<(Node Key, Datatype? Value, string SpecificType)>();
        public bool Complex { get; set; }

        protected override string ToStr()
        {
            var args = ParameterWriter.PrintParameters(Parameters);

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