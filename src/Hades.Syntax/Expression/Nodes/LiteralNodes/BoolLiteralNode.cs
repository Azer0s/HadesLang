using Hades.Syntax.Lexeme;

namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class BoolLiteralNode : LiteralNode<bool>
    {
        public BoolLiteralNode(Token token) : base(Classifier.BoolLiteral)
        {
            Value = bool.Parse(token.Value);
        }
    }
}