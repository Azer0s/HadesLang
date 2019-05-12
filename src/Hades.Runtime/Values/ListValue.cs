using System.Collections.Generic;

namespace Hades.Runtime.Values
{
    //A scope can have be an array variable
    //An array variable can be n-dimensional
    public class ListValue : LiteralValue<List<Scope>>, ScopeValue
    {
        public int Size { get; set; }
    }
}