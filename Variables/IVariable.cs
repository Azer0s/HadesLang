using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hades.Variables
{
    public abstract class IVariable
    {
        public AccessTypes Access { get; set; }
        public DataTypes DataType { get; set; }
    }
}
