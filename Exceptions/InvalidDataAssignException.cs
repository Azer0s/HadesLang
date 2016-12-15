using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class InvalidDataAssignException : Exception
    {
        public InvalidDataAssignException(string message) : base(message){ }
    }
}
