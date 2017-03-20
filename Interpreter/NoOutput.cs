using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class NoOutput : IScriptOutput
    {
        public void Write(string input)
        {
            //ignore
        }

        public void WriteLine(string input)
        {
            //ignore
        }

        public void Clear()
        {
            //ignore
        }
    }
}
