using System.Collections.Generic;
using Hades.Common;
using Hades.Syntax.Expression.Nodes.BlockNodes.Util;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class FunctionNode : BlockNode
    {
        public FunctionNode() : base(Classifier.Function)
        {
        }

        public bool Override { get; set; }
        public bool Fixed { get; set; }
        public string Name { get; set; }
        public List<(Node Key, Datatype? Value, string SpecificType)> Parameters { get; set; } = new List<(Node Key, Datatype? Value, string SpecificType)>();
        public Node Guard { get; set; }

        protected override string ToStr()
        {
            var args = ParameterWriter.PrintParameters(Parameters);

            if (!string.IsNullOrEmpty(args))
            {
                args = args.Substring(0, args.Length - 1);
                args = $" with parameters {args}";
            }

            var guard = Guard != null ? " with guard (" + Guard + ")" : "";
            var over = Override ? " override " : "";
            var fix = Fixed ? "fixed " : "";
            var str = $"{fix}{Name}{over}{args}{guard}\n{base.ToStr()}";
            return str;
        }
    }
}