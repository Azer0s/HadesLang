using Hades.Common;

namespace Hades.Syntax.Expression.Nodes
{
    public class VariableDeclarationNode : Node
    {
        public bool Mutable { get; set; }
        public string Name { get; set; }
        // ReSharper disable once MemberCanBePrivate.Global
        public Datatype? Datatype { get; set; } = null;
        public bool Array { get; set; }
        public int ArraySize { get; set; }
        public bool InfiniteArray { get; set; }
        public Node Assignment { get; set; }
        public bool Dynamic { get; set; }
        public bool Nullable { get; set; }

        public VariableDeclarationNode() : base(Classifier.VariableDeclaration)
        {
        }

        public override string ToString()
        {
            var nullable = Nullable ? " nullable" : "";
            var dynamic = Dynamic ? " dynamic" : "";
            var mutable = Mutable ? " mutable" : " imutable";
            var array = Array ? " array" : "";
            var datatype = Datatype == null ? "" : " " + Datatype.ToString().ToLower();
            var assignment = Assignment != null ? "'" + Assignment + "'" : "";
            var withAssignment = assignment != "" ? " with assignment " : ""; 
            return $"Create{nullable}{dynamic}{mutable}{array}{datatype} variable {Name}{withAssignment}{assignment}";
        }
    }
}