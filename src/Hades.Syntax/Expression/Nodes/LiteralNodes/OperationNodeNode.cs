namespace Hades.Syntax.Expression.Nodes.LiteralNodes
{
    public class OperationNodeNode : LiteralNode<Hades.Syntax.Lexeme.Classifier>
    {
        public readonly string Representation;
        
        public OperationNodeNode(Hades.Syntax.Lexeme.Classifier classifier, string val) : base(Classifier.Operation)
        {
            Value = classifier;
            Representation = val;
        }
    }
}