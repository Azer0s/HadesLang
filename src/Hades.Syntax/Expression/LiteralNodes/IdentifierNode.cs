namespace Hades.Syntax.Expression.LiteralNodes
{
    public class IdentifierNode : LiteralNode<string>
    {
        public IdentifierNode(string identifier) : base(Classifier.Identifier)
        {
            Value = identifier;
        }
    }
}