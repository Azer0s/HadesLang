using System;
using System.Collections.Generic;
using System.Linq;
using Hades.Common;
using Hades.Error;
using Hades.Runtime.Objects;
using Hades.Runtime.Values;
using Hades.Syntax.Expression;
using Hades.Syntax.Expression.Nodes;
using Hades.Syntax.Expression.Nodes.LiteralNodes;
using Hades.Syntax.Lexeme;

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
        
        public static Scope Run(RootNode rootNode, ref Scope scope)
        {
            foreach (var node in rootNode.Children)
            {
                if (node is VariableDeclarationNode)
                {
                    var child = RunStatement(node, scope);

                    if (scope.Variables.Any(a => a.Key == child.Name))
                    {
                        Error(ErrorStrings.MESSAGE_DUPLICATE_VARIABLE_DECLARATION, child.Name);
                    }
                    
                    scope.Variables.Add(child.Name, (child, AccessModifier.Private));
                    return child;
                }
            }

            if (rootNode.Children.Count == 1)
            {
                return RunStatement(rootNode.Children.First(), scope);
            }

            return new Scope();
        }

        public static Scope RunStatement(Node node, Scope parent)
        {
            Scope scope = null;
            
            //Here are the literal values. These return a literal scope + all the built-ins the literal values have
            if (node is BoolLiteralNode boolLiteral)
            {
                // ReSharper disable once UseObjectOrCollectionInitializer
                scope = new Scope();
                scope.Value = new BoolValue{Value = boolLiteral.Value};
                scope.Functions = BuiltIns.BOOL_BUILT_INS;
                scope.Datatype = Datatype.BOOL;
                return scope;
            }

            //NOTE/TODO: Btw.: Annotations become important when dealing with var declarations, functions, classes and structs
            
            if (node is VariableDeclarationNode variableDeclaration)
            {
                scope = new Scope
                {
                    Datatype = variableDeclaration.Datatype.GetValueOrDefault(Datatype.NONE),
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

                        if (result == null)
                        {
                            Error(ErrorStrings.MESSAGE_UNKNOWN_RUNTIME_EXCEPTION, variableDeclaration.ToString());
                        }
                        
                        if (!(result.Value is IntValue))
                        {
                            throw new Exception();
                        }
                    }
                    scope.Value = new ListValue {Size = size};
                }

                scope.Value = RunStatement(variableDeclaration.Assignment, parent);
            }
                        
            return scope;
        }
    }
}