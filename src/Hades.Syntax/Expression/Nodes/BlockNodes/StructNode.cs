using System.Collections.Generic;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class StructNode : Node
    {
        public StructNode() : base(Classifier.Struct)
        {
        }

        public string Name { get; set; }
        public AccessModifier AccessModifier { get; set; }
        public List<VariableDeclarationNode> PublicVariables { get; } = new List<VariableDeclarationNode>();
        public List<VariableDeclarationNode> PrivateVariables { get; } = new List<VariableDeclarationNode>();

        protected override string ToStr()
        {
            var privateVars = string.Join("\n    ", PrivateVariables);

            if (privateVars != string.Empty)
            {
                privateVars = $"\n  Private variables:\n    {privateVars}";
            }

            var publicVars = string.Join("\n    ", PublicVariables);

            if (publicVars != string.Empty)
            {
                publicVars = $"\n  Public variables:\n    {publicVars}";
            }

            return $"{Name}{privateVars}{publicVars}";
        }
    }
}