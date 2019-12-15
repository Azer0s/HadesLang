using Hades.Language.Parser.Ast;

namespace Hades.Language.Parser.Nodes
{
    public class UnmatchedNode : AstNode
    {
        public UnmatchedNode() : base(Type.AstUnmatched)
        {
            Matches = false;
        }

        protected override string DoToString()
        {
            throw new System.NotImplementedException();
        }
    }
}