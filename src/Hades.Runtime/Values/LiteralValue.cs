using Hades.Common;

namespace Hades.Runtime.Values
{
    public abstract class LiteralValue<T>
    {
        public T Value { get; set; }
    }
}