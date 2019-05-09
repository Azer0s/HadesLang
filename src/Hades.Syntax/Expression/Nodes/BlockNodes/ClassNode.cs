using System.Collections.Generic;

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
        
        //TODO: ToString
    }
}