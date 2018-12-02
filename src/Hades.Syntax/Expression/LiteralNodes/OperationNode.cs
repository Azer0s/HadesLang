namespace Hades.Syntax.Expression.LiteralNodes
{
    public class OperationNode : LiteralNode<Hades.Syntax.Lexeme.Classifier>
    {
        public readonly string Representation;
        
        public OperationNode(Hades.Syntax.Lexeme.Classifier classifier, string val) : base(Classifier.Operation)
        {
            Value = classifier;
            Representation = val;
        }
    }
}