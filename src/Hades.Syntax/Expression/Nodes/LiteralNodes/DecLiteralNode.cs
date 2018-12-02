using System.Globalization;
using Hades.Syntax.Lexeme;

namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class DecLiteralNode : LiteralNode<decimal>
    {
        public DecLiteralNode(Token token) : base(Classifier.DecLiteral)
        {
            Value = decimal.Parse(token.Value, CultureInfo.InvariantCulture);
        }
    }
}