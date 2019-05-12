namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class NullValueNode : LiteralNode<string>
    {
        public NullValueNode() : base(Classifier.NullLiteral)
        {
            Value = "NULL";
        }
    }
}