using System;
using System.Linq;
using System.Reflection;
using Hades.Common.Extensions;
using Hades.Core.Tools;
using Hades.Language.Lexer;
using Hades.Language.Parser;

namespace Hades.Core
{
    public static class Program
    {
        private const string VERSION = "0.0.1";

        public static int Main(string[] args)
        {
            if (args.Length != 0)
            {
                switch (args.First())
                {
                    case "new":
                        return ProjectInitializer.Run(args.Skip(1).ToList());
                    case "package":
                        return 0;
                }
            }

            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Hades (Hades Interactive Console). Built {Assembly.GetExecutingAssembly().GetBuildDate():M/d/yy h:mm:ss tt}");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Hades version {VERSION}");
            Console.WriteLine($"Running {Environment.OSVersion}");

            while (true)
            {
                var lexer = new Lexer();
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.Write("hd>");
                Console.ResetColor();

                try
                {
                    var tokens = lexer.LexFile(Console.ReadLine());
                    Console.WriteLine();
                    var parser = new Parser(tokens);
                    var root = parser.Parse();
                    Console.WriteLine(root);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}