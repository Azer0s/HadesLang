using System;
using System.Linq;
using Hades.Language.Lexer;

namespace Hades.Core
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            while (true)
            {
                var lexer = new Lexer();
                var result = lexer.LexFile(Console.ReadLine()).ToList();
            
                foreach (var token in result)
                {
                    Console.WriteLine($"{token.Kind} : {token.Value}");
                }
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}