namespace Hades.Syntax.Expression.Nodes
{
    public class NoVariableNode : LiteralNode<string>

    {
        public NoVariableNode() : base(Classifier.Misc)
        {
            Value = "NONE";
        }
    }
}