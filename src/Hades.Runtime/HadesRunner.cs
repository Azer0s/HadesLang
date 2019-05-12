using Hades.Syntax.Expression;
using Hades.Syntax.Expression.Nodes;

namespace Hades.Runtime
{
    public class HadesRunner
    {
        public static void Run(RootNode rootNode, Scope scope)
        {
            foreach (var node in rootNode.Children)
            {
                if (node is VariableDeclarationNode variableDeclaration)
                {
                }
            }
        }
    }
}