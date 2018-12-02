namespace Hades.Syntax.Expression
{
    public class RootNode : BlockNode
    {
        public RootNode() : base(Classifier.Root){}

        public override string ToString()
        {
            return $"ROOT\n{base.ToStr()}";
        }
    }
}