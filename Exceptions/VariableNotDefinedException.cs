using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class VariableNotDefinedException : Exception
    {
        public VariableNotDefinedException(string message) : base(typeof(VariableNotDefinedException).Name + ": " + message) { }
    }
}
