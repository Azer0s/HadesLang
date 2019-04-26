using Hades.Common;

namespace Hades.Syntax.Expression.Nodes
{
    public class VariableDeclarationNode : Node
    {
        public VariableDeclarationNode() : base(Classifier.VariableDeclaration)
        {
        }

        public bool Mutable { get; set; }
        public string Name { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public Datatype? Datatype { get; set; }
        public bool Array { get; set; }
        public Node ArraySize { get; set; }
        public bool InfiniteArray { get; set; }
        public Node Assignment { get; set; }
        public bool Dynamic { get; set; }
        public bool Nullable { get; set; }
        public string SpecificType { get; set; }

        protected override string ToStr()
        {
            var nullable = Nullable ? " nullable" : "";
            var dynamic = Dynamic ? " dynamic" : "";
            var mutable = Mutable ? " mutable" : " imutable";
            var array = Array ? " array" : "";
            var arraySize = ArraySize != null ? " (" + ArraySize + ")" : "";
            var datatype = Datatype != null ? " " + Datatype.ToString().ToLower() : "";

            if (SpecificType != null)
            {
                datatype += $"[{SpecificType}]";
            }
            
            var assignment = Assignment != null ? "(" + Assignment + ")" : "";
            var withAssignment = assignment != "" ? " with assignment " : "";
            return $"Create{nullable}{dynamic}{mutable}{datatype}{array}{arraySize} variable {Name}{withAssignment}{assignment}";
        }
    }
}