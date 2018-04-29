#pragma warning disable 168

namespace Variables
{
    public class Library : IVariable
    {
        public dynamic LibObject;

        public Library(int order) : base(order)
        {
        }
    }
}
