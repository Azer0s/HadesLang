using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hades.Variables
{
    public class Object : IVariable
    {
        public List<string> Lines { get; set; }
        public List<Methods> Methods { get; set; }
    }
}
