namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class IdentifierNode : LiteralNode<string>
    {
        public IdentifierNode(string identifier) : base(Classifier.Identifier)
        {
            Value = identifier;
        }
    }
}