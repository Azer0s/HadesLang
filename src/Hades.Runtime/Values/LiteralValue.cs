namespace Hades.Runtime.Values
{
    public abstract class LiteralValue<T>
    {
        public T Value { get; set; }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}