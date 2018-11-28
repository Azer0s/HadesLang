using System;
using System.Linq;
using Hades.Language.Lexer;
using Hades.Language.Parser;

namespace Hades.Core
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                var lexer = new Lexer();
                var tokens = lexer.LexFile(Console.ReadLine()).Where(a => a.Kind != Classifier.WhiteSpace).ToList();
                var parser = new Parser(tokens);
                var instructions = parser.Parse();
                
                foreach (var token in tokens.Where(a => a.Kind != Classifier.WhiteSpace))
                {
                    Console.WriteLine($"{token.Kind} : {token.Value}");
                }
                
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