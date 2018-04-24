using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Output
{
    public class WebOutput : IScriptOutput
    {
        public StringBuilder Output;

        public WebOutput()
        {
            Output = new StringBuilder("<!DOCTYPE html>");
        }

        public void Write(string input)
        {
            Output.Append(input.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\/","/"));
        }

        public void WriteLine(string input)
        {
            Output.Append(input.Replace("\\n", "\n").Replace("\\t", "\t").Replace("\\/", "/"));
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
