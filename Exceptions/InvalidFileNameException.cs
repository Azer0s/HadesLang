using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class InvalidFileNameException : Exception
    {
        public InvalidFileNameException(string message) : base(typeof(InvalidFileNameException).Name + ": " + message) { }
    }
}
