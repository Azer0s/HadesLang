using System;
using System.Linq;
using Hades.Error;
using Hades.Runtime.Values;
using Hades.Syntax.Expression;
using Hades.Syntax.Expression.Nodes;

namespace Hades.Runtime
{
    public static class HadesRuntime
    {
        #region Helpers

        private static void Error(string error, params object[] format)
        {
            Error(string.Format(error, format));
        }
        
        private static void Error(string error)
        {
            throw new Exception(error);
        }
        
        #endregion
        
        public static Scope Run(RootNode rootNode, Scope scope)
        {
            foreach (var node in rootNode.Children)
            {
                if (node is VariableDeclarationNode)
                {
                    var child = RunStatement(node, scope);

                    if (scope.PrivateVariables.Any(a => a.Name == child.Name))
                    {
                        Error(ErrorStrings.MESSAGE_DUPLICATE_VARIABLE_DECLARATION, child.Name);
                    }
                    
                    scope.PrivateVariables.Add(child);
                }
            }

            return scope;
        }

        private static Scope RunStatement(Node node, Scope parent)
        {
            Scope scope = null;
            
            if (node is VariableDeclarationNode variableDeclaration)
            {
                scope = new Scope
                {
                    Datatype = variableDeclaration.Datatype,
                    Name = variableDeclaration.Name,
                    SpecificType = variableDeclaration.SpecificType,
                    Mutable = variableDeclaration.Mutable,
                    Dynamic = variableDeclaration.Dynamic,
                    Nullable = variableDeclaration.Nullable
                };

                if (variableDeclaration.Array)
                {
                    var size = -1;
                    if (variableDeclaration.InfiniteArray)
                    {
                        //TODO: Multidimensional  array
                        var result = RunStatement(variableDeclaration.ArraySize, parent);
                        if (!(result.Value is IntValue))
                        {
                            throw new Exception();
                        }
                    }
                    scope.Value = new ListValue {Size = size};
                }
            }

            return scope;
        }
    }
}