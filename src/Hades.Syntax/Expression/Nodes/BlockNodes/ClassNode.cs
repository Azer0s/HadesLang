using System.Collections.Generic;
using System.Linq;
using Hades.Common.Extensions;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class ClassNode : Node
    {
        public ClassNode() : base(Classifier.Class)
        {
        }

        public AccessModifier AccessModifier { get; set; }
        public bool Fixed { get; set; }
        public List<string> Parents { get; set; } = new List<string>();
        public string Name { get; set; }
        public List<VariableDeclarationNode> PublicVariables { get; set; } = new List<VariableDeclarationNode>();
        public List<VariableDeclarationNode> PrivateVariables { get; set; } = new List<VariableDeclarationNode>();
        public List<VariableDeclarationNode> ProtectedVariables { get; set; } = new List<VariableDeclarationNode>();
        public List<FunctionNode> Functions { get; set; } = new List<FunctionNode>();
        public List<ClassNode> Classes { get; set; } = new List<ClassNode>();
        public List<FunctionNode> Constructors { get; set; } = new List<FunctionNode>();

        protected override string ToStr()
        {
            var privateVars = string.Join("\n    ", PrivateVariables);

            if (privateVars != string.Empty)
            {
                privateVars = $"\n  Private variables:\n    {privateVars}";
            }

            var protectedVars = string.Join("\n    ", ProtectedVariables);

            if (protectedVars != string.Empty)
            {
                protectedVars = $"\n  Protected variables:\n    {protectedVars}";
            }

            var publicVars = string.Join("\n    ", PublicVariables);

            if (publicVars != string.Empty)
            {
                publicVars = $"\n  Public variables:\n    {publicVars}";
            }

            var functions = Functions.Map(a => a.ToString().Replace("\n", "\n    ")).ToList();
            var fn = string.Empty;

            if (functions.Count != 0)
            {
                fn = $"\n  Functions:\n    {string.Join("\n    ", functions)}";
            }

            var constructors = Constructors.Map(a => a.ToString().Replace("\n", "\n    ")).ToList();
            var ctor = string.Empty;

            if (constructors.Count != 0)
            {
                ctor = $"\n  Constructors:\n    {string.Join("\n    ", constructors)}";
            }

            var inherits = string.Empty;
            if (Parents.Count != 0)
            {
                inherits = $" inherits from {string.Join(", ", Parents)}";
            }

            return $"{Name}{inherits}{privateVars}{protectedVars}{publicVars}{ctor}{fn}";
        }
    }
}