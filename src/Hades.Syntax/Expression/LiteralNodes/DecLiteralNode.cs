using Hades.Syntax.Lexeme;

namespace Hades.Syntax.Expression.LiteralNodes
{
    public class DecLiteralNode : LiteralNode<decimal>
    {
        public DecLiteralNode(Token token) : base(Classifier.DecLiteral)
        {
        }
    }
}