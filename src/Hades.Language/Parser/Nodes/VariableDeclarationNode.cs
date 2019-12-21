using System.Linq;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser.Nodes
{
    public class VariableDeclarationNode : AstNode
    {
        public bool IsConstant { get; set; }
        public GenericNode Datatype { get; set; }
        public bool IsArray { get; set; }
        public ArraySizeNode ArraySize { get; set; }
        public bool IsNullable { get; set; }
        public IdentifierNode Name { get; set; }

        public VariableDeclarationNode() : base(Type.AstVariableDeclaration)
        {
        }

        protected override string DoToString()
        {
            var mutable = IsConstant ? "Immutable" : "Mutable";
            var nullable = IsNullable ? "nullable" : "";
            var variable =  IsArray ? "array [" + 
                                      string.Join("x", 
                                          ArraySize.Tokens
                                              .Select(x =>
                                              {
                                                  return x switch
                                                  {
                                                      GenericNode c when c.Type == Type.Multiplication => c.Value.Value, 
                                                      GenericNode c when c.Type == Type.Integer => c.Value.Value, 
                                                      _ => $"({x})"
                                                  };
                                              })) + "]" : "variable";
            var datatype = Datatype == null ? "" : Datatype.Value.Value;
            return string.Join(" ", $"{mutable} {nullable} {datatype} {variable} {Name.Identifier.Value}".Split(" ").Where(a => a != string.Empty));
        }
    }
}