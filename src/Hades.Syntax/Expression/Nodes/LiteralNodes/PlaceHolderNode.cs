namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class PlaceHolderNode : LiteralNode<string>
    {
        public PlaceHolderNode() : base(Classifier.Placeholder)
        {
            Value = "Placeholder";
        }
    }
}