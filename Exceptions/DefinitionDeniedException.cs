using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Exceptions
{
    public class DefinitionDeniedException : Exception
    {
        public DefinitionDeniedException(string message) : base(typeof(DefinitionDeniedException).Name + ": " + message) { }
    }
}
