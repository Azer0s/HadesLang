using System;
using System.Globalization;
using System.Linq;
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

            var s = args.ToList().Count != 0 ? new ShellInterpreter(args.First()) : new ShellInterpreter();
            s.Start();
        }
    }
}
