using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCalc;
using StringExtension;
using Variables;

namespace Interpreter
{
    public class ShellInterpreter
    {
        public void Start()
        {
            var interpreter = new Interpreter();
            Cache.Instance.Variables = new Dictionary<Tuple<string,string>, Types>();
            while (true)
            {
                Console.Write(">");
                var res = Console.ReadLine();
                string op;
                var returnVar = interpreter.InterpretLine(res,"console",out op).Key;

                if (interpreter.Clear)
                {
                    interpreter.Clear = false;
                    Console.Clear();
                }
                else
                {
                    Console.WriteLine(returnVar);
                }
            }
        }
    }
}
