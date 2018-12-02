namespace Hades.Syntax.Expression.LiteralNodes
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