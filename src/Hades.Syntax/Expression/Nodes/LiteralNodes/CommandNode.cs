namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class CommandNode : LiteralNode<string>
    {
        public CommandNode(string command) : base(Classifier.Command)
        {
            Value = command;
        }

        public override string ToString()
        {
            return $"Command: {Value}";
        }
    }
}