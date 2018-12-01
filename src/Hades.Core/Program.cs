using System;
using System.Linq;
using Hades.Core.Tools;
using Hades.Language.Lexer;
using Hades.Language.Parser;

namespace Hades.Core
{
    public static class Program
    {
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
            
            while (true)
            {
                var lexer = new Lexer();
                var tokens = lexer.LexFile(Console.ReadLine());
                var parser = new Parser(tokens);
                var instructions = parser.Parse();
                
                foreach (var instruction in instructions)
                {
                    if (instruction != null)
                    {
                        Console.WriteLine($"{instruction.Classifier} => {instruction}");
                    }
                }
            }
        }
    }
}