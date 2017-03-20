using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public interface IScriptOutput
    {
        void Write(string input);
        void WriteLine(string input);
        void Clear();
    }
}
