namespace Hades.Syntax.Expression.Nodes
{
    public class RootNode : Node
    {
        public RootNode() : base(Classifier.Root){}

        public override string ToString()
        {
            return "ROOT";
        }
    }
}