using System;
using System.Globalization;
using System.Text;
using System.Threading;
using Interpreter;

namespace HadesLang
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            Console.OutputEncoding = Encoding.UTF8;

            var s = new ShellInterpreter();
            s.Start();
        }
    }
}
