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
        
        public VariableDeclarationNode() : base(Classifier.VariableDeclaration)
        {
        }

        public override string ToString()
        {
            var array = Array ? " array" : "";
            var mutable = Mutable ? " mutable" : " imutable";
            var assignment = Assignment != null ? "'" + Assignment + "'" : "";
            var withAssignment = assignment != "" ? " with assignment " : ""; 
            return Datatype == null ? $"Create{mutable}{array} variable {Name}{withAssignment}{assignment}" : $"Create{mutable}{array} {Datatype.ToString().ToLower()} variable {Name}{withAssignment}{assignment}";
        }
    }
}