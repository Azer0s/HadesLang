namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class OperationNodeNode : LiteralNode<Lexeme.Classifier>
    {
        public readonly string Representation;

        public OperationNodeNode(Lexeme.Classifier classifier, string val) : base(Classifier.Operation)
        {
            Value = classifier;
            Representation = val;
        }
    }
}