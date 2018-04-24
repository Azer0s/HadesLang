using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Output;
using Variables;
using static System.String;
using Console = Colorful.Console;

namespace Interpreter
{
    public class ShellInterpreter
    {
        private static readonly Color BLUE = Color.FromArgb(240, 6, 153);
        private static readonly Color YELLOW = Color.FromArgb(247, 208, 2);
        private static readonly Color PURPLE = Color.FromArgb(69, 78, 158);
        private static readonly Color RED = Color.FromArgb(191, 26, 47);
        private static readonly Color GREEN = Color.FromArgb(1, 142, 66);

        public ShellInterpreter(string path = null)
        {
            Console.Clear();
            Console.WriteAscii("HadesLang",BLUE);
            var interpreter = new Interpreter(new ConsoleOutput(), new ConsoleOutput());
            Console.WriteLine();
            Console.WriteLine("\t======================================",YELLOW);
            Console.Write("\t||", YELLOW);
            Console.Write("           Hades - v0.4           ", PURPLE);
            Console.WriteLine("||",YELLOW);
            Console.WriteLine("\t======================================", YELLOW);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("\t\t      Press key...", GREEN);
            Console.ReadKey();
            Console.Clear();

            interpreter.RegisterFunction(new Function("print", a =>
            {
                a.ToList().ForEach(Console.WriteLine);
                return "";
            }));
            Evaluator.AliasManager.Add("clear","cls");

            if (path != null)
            {
                //File interpreter
                //TODO: support constructor params
                interpreter.ExplicitOutput.WriteLine(new FileInterpreter(path,Cache.Instance.GetOrder(), interpreter).Execute(interpreter, new List<string> {"console"}).Value);
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
                        Console.Write(":");
                        lineInput = Console.ReadLine();
                        if (lineInput != "\t")
                        {
                            lines.Add(lineInput);
                        }
                    }

                    var fi = new FileInterpreter(null,Cache.Instance.GetOrder(),interpreter,lines);
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