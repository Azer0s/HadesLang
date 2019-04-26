﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Hades.Common;
using Hades.Common.Extensions;
using Hades.Common.Util;
using Hades.Core.Tools;
using Hades.Language.Lexer;
using Hades.Language.Parser;
using Hades.Syntax.Lexeme;

namespace Hades.Core
{
    public static class Program
    {
        private const string VERSION = "0.0.1";

        private static void HighLight(IEnumerable<Token> tks)
        {
            Console.ForegroundColor = ConsoleColor.White;
            var tokens = tks.ToList();
            for(var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                
                if (token.Kind == Classifier.Identifier && char.IsUpper(token.Value[0]))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                if (i + 1 < tokens.Count)
                {
                    if (token.Kind == Classifier.Identifier && (tokens[i+1].Kind == Classifier.LeftParenthesis || tokens[i+1].Kind == Classifier.Colon))
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                    }
                }
                
                if (token.Kind == Classifier.Identifier && token.Value == "super")
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;
                }
                
                if (token.Kind == Classifier.Keyword)
                {
                    Console.ForegroundColor = ConsoleColor.DarkCyan;             
                }

                if (token.Kind == Classifier.StringLiteral)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    token.Value = $"\"{token.Value}\"";
                }

                if (token.Kind == Classifier.IntLiteral || token.Kind == Classifier.DecLiteral)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }

                if (token.Kind == Classifier.BoolLiteral)
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }

                if (token.Category == Category.Operator || token.Kind == Classifier.Question)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                }

                if (Enum.TryParse<Datatype>(token.Value.ToUpper(),out _) && !token.Value.All(char.IsDigit))
                {
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                }

                if (token.Kind == Classifier.LeftBracket || token.Kind == Classifier.RightBracket || token.Kind == Classifier.LeftBrace || token.Kind == Classifier.RightBrace)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                }

                if (token.Category == Category.Comment)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                }

                if (token.Value == "std")
                {
                    Console.ForegroundColor = ConsoleColor.White;
                }
                
                Console.Write(token.Value);
                Console.ForegroundColor = ConsoleColor.White;
            }
        }
        
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
                    var line = Console.ReadLine();
                    
                    ConsoleFunctions.ClearCurrentConsoleLine();
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write("hd>");
                    var tokens = lexer.LexFile(line);
                    HighLight(tokens);
                    Console.WriteLine();

                    tokens = lexer.LexFile(line);
                    var parser = new Parser(tokens);
                    var root = parser.Parse();
                    Console.WriteLine(root);
                    Console.WriteLine();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }
    }
}