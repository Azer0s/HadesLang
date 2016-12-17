using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class InvalidDataTypeException : Exception
    {
        public InvalidDataTypeException (string message) : base(typeof(InvalidDataTypeException).Name + ": " + message) { }
    }
}
