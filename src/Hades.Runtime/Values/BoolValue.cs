namespace Hades.Runtime.Values
{
    public class BoolValue : LiteralValue<bool>, ScopeValue
    {
        public override string ToString()
        {
            return Value.ToString().ToLower();
        }
    }
}