using System;
using System.Collections.Generic;

namespace Variables
{
    public class Array : IVariable
    {
        public Dictionary<int,string> Values;
        public int Capacity = int.MaxValue;
    }
}