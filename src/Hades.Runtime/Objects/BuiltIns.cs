using System.Collections.Generic;
using System.Text;
using Hades.Common;
using Hades.Runtime.Values;
using Hades.Syntax.Expression.Nodes;
using Hades.Syntax.Expression.Nodes.LiteralNodes;
// ReSharper disable InconsistentNaming

namespace Hades.Runtime.Objects
{
    public class BuiltIns
    {
        private static string CreateMD5(string input)
        {
            // Use input string to calculate MD5 hash
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                var inputBytes = Encoding.ASCII.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);

                // Convert the byte array to hexadecimal string
                var sb = new StringBuilder();
                foreach (var t in hashBytes)
                {
                    sb.Append(t.ToString("X2"));
                }
                return sb.ToString();
            }
        }
        
        public static readonly Dictionary<string, List<Scope>> OBJECT_BUILT_INS = new Dictionary<string, List<Scope>>
        {
            {
                "toString", 
                new List<Scope>
                {
                    new Scope
                    {
                        Name = "toString",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>{{"dst", Datatype.NONE}}, 
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new CallNode{Source = new IdentifierNode("this"), Target = new IdentifierNode("toString")}, scopes[0])
                    }
                }
                
                //toString has to be implemented specifically for each value scope; this is why I call toString on the parameter
                //I am doing virtual abstract methods...pretty f-ing cool 😎
            },
            {
                "hash", 
                new List<Scope>
                {
                    new Scope
                    {
                        Name = "hash",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>{{"dst", Datatype.NONE}}, 
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new CallNode{Source = new IdentifierNode("this"), Target = new IdentifierNode("hash")}, scopes[0])
                    },
                    new Scope
                    {
                        Name = "hash",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>(),
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new StringLiteralNode(CreateMD5((HadesRuntime.RunStatement(new CallNode{Source = new IdentifierNode("this"), Target = new IdentifierNode("toString")}, scope).Value as StringValue)?.Value)), scope)
                    }
                }                
            },
            {
                "nameof",
                new List<Scope>
                {
                    new Scope
                    {
                        Name = "nameof",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>{{"dst", Datatype.NONE}},
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new StringLiteralNode(scopes[0].Name), scope)
                    },
                    new Scope
                    {
                        Name = "nameof",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>(),
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new StringLiteralNode(scope.Name), scope)
                    }
                }
            },
            {
                "type",
                new List<Scope>
                {
                    new Scope
                    {
                        Name = "type",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>{{"dst", Datatype.NONE}},
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new StringLiteralNode(scopes[0].Datatype.ToString().ToLower()), scope)
                    },
                    new Scope
                    {
                        Name = "type",
                        IsNativeFunction = true,
                        NativeFunctionSignature = new Dictionary<string, Datatype>(),
                        NativeFunction = (scopes, scope) => HadesRuntime.RunStatement(new StringLiteralNode(scope.Datatype.ToString().ToLower()), scope)
                    }
                }
                //TODO: equals
            }
        };
        
        private static Dictionary<string, List<Scope>> _bool_built_ins;
        
        public static Dictionary<string, List<Scope>> BOOL_BUILT_INS
        {
            get
            {
                if (_bool_built_ins != null) return _bool_built_ins;
                
                _bool_built_ins = OBJECT_BUILT_INS;
                    
                //TODO: Equals
                    
                _bool_built_ins["toString"].Add(new Scope
                {
                    IsNativeFunction = true,
                    NativeFunctionSignature = new Dictionary<string, Datatype>(),
                    NativeFunction = (scopes, scope) =>
                    {
                        /*
                         * Okay...let's unroll what happens here. toString returns a string.
                         * As we all know, a string has built-ins.
                         * So does the string that toString returns (obviously)
                         * So to get all of these built-ins, we have to run a literal node through the runtime
                         */
                        // ReSharper disable once ConvertToLambdaExpression
                        return HadesRuntime.RunStatement(
                            new StringLiteralNode((scope.Value as BoolValue)?.Value.ToString().ToLower()),
                            scope);
                    }
                });

                return _bool_built_ins;
            }
        }
    }
}