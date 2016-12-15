using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class InvalidAccessTypeException : Exception
    {
        public InvalidAccessTypeException(string message) : base(message) { }
    }
}
