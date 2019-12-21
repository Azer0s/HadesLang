using System.Collections.Generic;
using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser.Nodes
{
    public class ArraySizeNode : AstNode
    {
        public bool IsMultiDimensional;
        public List<AstNode> Tokens = new List<AstNode>();
        
        public ArraySizeNode() : base(Type.AstArraySize)
        {
        }

        protected override string DoToString()
        {
            return "";
        }
    }
}