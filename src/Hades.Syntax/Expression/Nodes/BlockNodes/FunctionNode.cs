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
        public AccessModifier AccessModifier { get; set; }
        public bool Extension { get; set; }
        public (string specificType, Datatype dt) ExtensionType { get; set; }

        protected override string ToStr()
        {
            var args = ParameterWriter.PrintParameters(Parameters);

            if (!string.IsNullOrEmpty(args))
            {
                args = args.Substring(0, args.Length - 1);
                args = $" with parameters {args}";
            }

            var accessModifier = AccessModifier.ToString().ToLower();
            var guard = Guard != null ? " with guard (" + Guard + ")" : "";
            var over = Override ? "override " : "";
            var fix = Fixed ? " fixed " : " ";

            var extends = Extension ? (string.IsNullOrEmpty(ExtensionType.specificType) ? $" extends {ExtensionType.dt.ToString().ToLower()}" : $" extends {ExtensionType.dt.ToString().ToLower()}::{ExtensionType.specificType}") : " <";
            
            var str = $"{accessModifier}{fix}{over}{Name}{extends}{args}{guard}\n{base.ToStr()}";
            return str;
        }
    }
}