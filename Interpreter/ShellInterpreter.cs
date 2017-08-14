using System;
using System.Linq;
using Output;
using Variables;

namespace Interpreter
{
    public class ShellInterpreter
    {
        public ShellInterpreter(string path = null)
        {
            var interpreter = new global::Interpreter.Interpreter(new ConsoleOutput(),new ConsoleOutput());
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