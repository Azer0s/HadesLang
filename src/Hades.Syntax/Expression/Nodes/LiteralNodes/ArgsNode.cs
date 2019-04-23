namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class ArgsNode : LiteralNode<string>
    {
        public ArgsNode(string identifier) : base(Classifier.Identifier)
        {
            Value = identifier;
        }
        
        public override string ToString()
        {
            return $"Varargs Value: {Value}";
        }
    }
}