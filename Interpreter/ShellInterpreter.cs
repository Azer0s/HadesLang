using System;
using System.Collections.Generic;
using System.Linq;
using Output;
using Variables;
using static System.String;

namespace Interpreter
{
    public class ShellInterpreter
    {
        public ShellInterpreter(string path = null)
        {
            var interpreter = new Interpreter(new ConsoleOutput(),new ConsoleOutput());
            interpreter.RegisterFunction(new Function("print", a =>
            {
                a.ToList().ForEach(Console.WriteLine);
                return "";
            }));

            interpreter.RegisterFunction(new Function("helloworld", a => "Hello world"));

            if (path != null)
            {
                //File interpreter
                //TODO: support constructor params
                interpreter.ExplicitOutput.WriteLine(new FileInterpreter(path).Execute(interpreter, new List<string> {"console"}).Value);
                return;
            }

            while (true)
            {
                Console.Write(">");

                var input = Console.ReadLine();

                if (input == "\t")
                {
                    var lines = new List<string>();
                    var lineInput = Empty;
                    while (lineInput != "\t")
                    {
                        Console.WriteLine(":");
                        lineInput = Console.ReadLine();
                        if (lineInput != "\t")
                        {
                            lines.Add(lineInput);
                        }
                    }

                    var fi = new FileInterpreter(null,lines);
                    fi.Execute(interpreter, new List<string>{"console"});
                }
                else
                {
                    interpreter.InterpretLine(input, new List<string> { "console" }, null,true);
                }
            }
        }
    }
}