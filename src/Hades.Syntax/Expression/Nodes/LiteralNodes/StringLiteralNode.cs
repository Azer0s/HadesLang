using Hades.Syntax.Lexeme;

namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class StringLiteralNode : LiteralNode<string>
    {
        public StringLiteralNode(Token token) : base(Classifier.StringLiteral)
        {
            Value = token.Value;
        }
    }
}