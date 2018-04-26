using System.Threading;
using Console = Colorful.Console;
using static HadesWeb.Util.Log;
// ReSharper disable InconsistentNaming

namespace HadesWeb
{
    class Program
    {
        public static void Main(string[] args)
        {
            Title();
            var startup = new Startup(args);
            startup.Start();
            // ReSharper disable once FunctionNeverReturns
        }

        private static void Title()
        {
            Console.Clear();
            Console.Title = "HadesWeb Server";
            var name = string.Empty;
            foreach (var c in "HadesWeb Server")
            {
                name += c;
                Console.Clear();
                Console.WriteAscii(name, Blue);
                Thread.Sleep(70);
            }

            Info("Initializing Hades Interpreter...");
        }
    }
}