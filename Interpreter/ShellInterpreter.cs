using System;
using System.Collections.Generic;
using System.Linq;
using Hades.Output;
using Hades.Variables;

namespace Hades.Interpreter
{
    public class ShellInterpreter
    {
        public ShellInterpreter(string path = null)
        {
            var interpreter = new Interpreter(new ConsoleOutput(),new ConsoleOutput());
            interpreter.RegisterFunction(new Function("print", a =>
            {
                a.ToList().ForEach(Console.WriteLine);
            }));

            if (path != null)
            {
                //File interpreter
            }

            while (true)
            {
                Console.Write(">");
                interpreter.InterpretLine(Console.ReadLine(), "console");
            }
        }
    }
}