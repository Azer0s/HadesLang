﻿using System;
using Hades.Language.Lexer;

namespace Hades.Core
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var lexer = new Lexer();
                var result = lexer.LexFile(Console.ReadLine());
            
                foreach (var token in result)
                {
                    Console.WriteLine($"{token.Kind} : {token.Value}");
                }
            }
            
        }
    }
}