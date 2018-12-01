namespace Hades.Syntax.Expression.Nodes
{
    public class RootNode : Node
    {
        public RootNode() : base(Classifier.Root){}

        protected override string ToStr()
        {
            return "ROOT";
        }
    }
}