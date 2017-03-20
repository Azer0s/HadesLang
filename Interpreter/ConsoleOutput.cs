using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class ConsoleOutput : IScriptOutput
    {
        public void Write(string input)
        {
            Console.Write(input);
        }

        public void WriteLine(string input)
        {
            Console.WriteLine(input);
        }

        public void Clear()
        {
            Console.Clear();
        }
    }
}
