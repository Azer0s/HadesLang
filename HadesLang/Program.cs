using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hades.Interpreter;

namespace HadesLang
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            Console.OutputEncoding = Encoding.UTF8;

            var s = args.ToList().Count != 0 ? new ShellInterpreter(args.First()) : new ShellInterpreter();
        }
    }
}
