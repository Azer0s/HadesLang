using Hades.Syntax.Lexeme;

namespace Hades.Syntax.Expression.LiteralNodes
{
    public class StringLiteralNode : LiteralNode<string>
    {
        public StringLiteralNode(Token token) : base(Classifier.StringLiteral)
        {
            Value = token.Value;
        }
    }
}