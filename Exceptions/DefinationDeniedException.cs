using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class DefinationDeniedException : Exception
    {
        public DefinationDeniedException(string message) : base(typeof(DefinationDeniedException).Name + ": " + message) { }
    }
}
