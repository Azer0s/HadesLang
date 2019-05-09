using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Hades.Syntax.Expression.Nodes.BlockNodes
{
    public class ClassNode : BlockNode
    {
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

        public ClassNode() : base(Classifier.Class)
        {
        }
        
        protected override string ToStr()
        {
            var str = string.Empty;

            var privateVars = string.Join("\n    ", PrivateVariables);

            if (privateVars != string.Empty)
            {
                privateVars = "\n  Private variables:\n    " + privateVars;
            }
            
            var protectedVars = string.Join("\n", ProtectedVariables);

            if (protectedVars != string.Empty)
            {
                protectedVars = "\n  Protected variables:\n    " + protectedVars;
            }
            
            var publicVars = string.Join("\n", PublicVariables);

            if (publicVars != string.Empty)
            {
                publicVars = "\n  Public variables:\n    " + publicVars;
            }
            
            return privateVars + protectedVars;
        }
        //TODO: ToString
    }
}