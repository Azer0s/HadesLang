using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Output
{
    class WebOutput : IScriptOutput
    {
        public StringBuilder Output;

        public WebOutput()
        {
            Output = new StringBuilder("<!DOCTYPE html>");
        }

        public void Write(string input)
        {
            Output.Append(input);
        }

        public void WriteLine(string input)
        {
            Output.AppendLine(input);
        }

        public void Clear()
        {
            Output.Clear();
        }

        public string ReadLine()
        {
            return "";
        }
    }
}
