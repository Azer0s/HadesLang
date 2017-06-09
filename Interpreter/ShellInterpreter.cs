﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CustomFunctions;
using NCalc;
using StringExtension;
using Variables;

namespace Interpreter
{
    public class ShellInterpreter
    {
        public KeyValuePair<bool, string> StartAsScript;

        public ShellInterpreter()
        {
            StartAsScript = new KeyValuePair<bool, string>(false,"");
        }

        public ShellInterpreter(string path)
        {
            StartAsScript = new KeyValuePair<bool, string>(true,path);
        }

        public void Start()
        {
            var interpreter = new Interpreter(new ConsoleOutput());
            interpreter.RegisterFunction(new Function("print", () =>
            {
                interpreter.GetFunctionValues().ForEach(Console.WriteLine);
            }));
            Cache.Instance.Variables = new Dictionary<Tuple<string,string>, Types>();

            if (StartAsScript.Key)
            {
                var fi = new FileInterpreter(StartAsScript.Value, interpreter.Evaluator.Output);
                fi.LoadAll();
                try
                {
                    Console.WriteLine(fi.Return.Value.Value);
                }
                catch (Exception)
                {
                    // ignored
                }
                Console.ReadKey();
                return;
            }

            while (true)
            {
                Console.Write(">");
                var res = Console.ReadLine();

                if (res != null && res.Split(':')[0] == "scriptOutput")
                {
                    int toggle;
                    int.TryParse(res.Split(':')[1], out toggle);
                    switch (toggle)
                    {
                        case 0:
                            interpreter.Evaluator.Output = new NoOutput();
                            Console.WriteLine("Script-output disabled!");
                            break;
                        case 1:
                            interpreter.Evaluator.Output = new ConsoleOutput();
                            Console.WriteLine("Script-output enabled!");
                            break;
                    }
                    continue;
                }

                string op;
                var returnVar = interpreter.InterpretLine(res,"console",out op).Key;

                if (interpreter.Clear)
                {
                    interpreter.Clear = false;
                    Console.Clear();
                }
                else
                {
                    if (returnVar != null) Console.WriteLine(returnVar);
                }
            }
        }
    }
}
