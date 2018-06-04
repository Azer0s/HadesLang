using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Threading;
using Output;
using Variables;
using static System.String;
using Console = Colorful.Console;

namespace Interpreter
{
    public class ShellInterpreter
    {
        public ShellInterpreter(string path = null)
        {
            if (IsNullOrEmpty(path))
            {
                //PrintStart(new decimal(0.4));
            }

            var interpreter = new Interpreter(new ConsoleOutput(), new ConsoleOutput());
            interpreter.RegisterFunction(new Function("print", a =>
            {
                a.ToList().ForEach(Console.WriteLine);
                return "";
            }));
            Evaluator.AliasManager.Add("clear","cls");

            if (path != null)
            {
                //File interpreter
                //TODO: support constructor params
                interpreter.ExplicitOutput.WriteLine(new FileInterpreter(path,Cache.Instance.GetOrder(), interpreter).Execute(interpreter, new List<string> {"console"}).Value);
                return;
            }

            while (true)
            {
                Console.Write(">");

                var input = Console.ReadLine();

                if (input == "\t")
                {
                    var lines = new List<string>();
                    var lineInput = Empty;
                    while (lineInput != "\t")
                    {
                        Console.Write(":");
                        lineInput = Console.ReadLine();
                        if (lineInput != "\t")
                        {
                            lines.Add(lineInput);
                        }
                    }

                    var fi = new FileInterpreter(null,Cache.Instance.GetOrder(),interpreter,lines);
                    fi.Execute(interpreter, new List<string>{"console"});
                }
                else
                {
                    interpreter.InterpretLine(input, new List<string> { "console" }, null,true);
                }
            }
        }

        #region Title

        private static readonly Color Purple = Color.Fuchsia;
        private static readonly Color LightYellow = Color.Yellow;
        private static readonly Color Orange = Color.DarkOrange;
        private static readonly Color LightBlue = Color.LightSkyBlue;
        private static readonly Color White = Color.White;

        private static void PrintStart(decimal version)
        {
            Console.Title = "HadesLang - REPL";
            Console.Clear();
            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t           /--\n", LightBlue);
            Console.Write("\t\t           +:", LightBlue); Console.Write("      ", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(1500);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(90);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t            ::", LightYellow); Console.Write("-..`", White); Console.Write(".::\n", Orange);
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(90);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t            /:", Orange); Console.Write("--`.-", White); Console.Write("::\n", LightYellow);
            Console.Write("\t\t            ::", LightYellow); Console.Write("-..`", White); Console.Write(".::\n", Orange);
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(90);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t            :::", LightYellow); Console.Write("---", White); Console.Write("::+\n", Orange);
            Console.Write("\t\t            /:", Orange); Console.Write("--`.-", White); Console.Write("::\n", LightYellow);
            Console.Write("\t\t            ::", LightYellow); Console.Write("-..`", White); Console.Write(".::\n", Orange);
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(90);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t              ::::/:\n", LightYellow);
            Console.Write("\t\t            :::", LightYellow); Console.Write("---", White); Console.Write("::+\n", Orange);
            Console.Write("\t\t            /:", Orange); Console.Write("--`.-", White); Console.Write("::\n", LightYellow);
            Console.Write("\t\t            ::", LightYellow); Console.Write("-..`", White); Console.Write(".::\n", Orange);
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(90);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t\n");
            Console.Write("\t\t\n");
            Console.Write("\t\t               :::-\n", Orange);
            Console.Write("\t\t              ::::/:\n", LightYellow);
            Console.Write("\t\t            :::", LightYellow); Console.Write("---", White); Console.Write("::+\n", Orange);
            Console.Write("\t\t            /:", Orange); Console.Write("--`.-", White); Console.Write("::\n", LightYellow);
            Console.Write("\t\t            ::", LightYellow); Console.Write("-..`", White); Console.Write(".::\n", Orange);
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Thread.Sleep(90);
            Console.Clear();

            Console.WriteAscii(" HadesLang", Purple);
            Console.WriteLine();
            Console.Write("\t\t                /:+\n", Orange);
            Console.Write("\t\t                ::-\n", LightYellow);
            Console.Write("\t\t               :::-\n", Orange);
            Console.Write("\t\t              ::::/:\n", LightYellow);
            Console.Write("\t\t            :::", LightYellow); Console.Write("---", White); Console.Write("::+\n", Orange);
            Console.Write("\t\t            /:", Orange); Console.Write("--`.-", White); Console.Write("::\n", LightYellow);
            Console.Write("\t\t            ::", LightYellow); Console.Write("-..`", White); Console.Write(".::\n", Orange);
            Console.Write("\t\t           /--", LightBlue); Console.Write("..```", White); Console.Write(".-+\n", LightYellow);
            Console.Write("\t\t           +:", LightBlue); Console.Write(".`````", White); Console.Write(".-+\n", LightBlue);
            Console.Write("\t\t             +:--..:/\n", LightBlue);
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("     =======================================================", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Hades - v{version.ToString(CultureInfo.InvariantCulture)}                    ", LightBlue);
            Console.WriteLine("||", LightYellow);
            Console.Write("     ||", LightYellow);
            Console.Write($"                   Press any key...                ", Color.LimeGreen);
            Console.WriteLine("||", LightYellow);
            Console.WriteLine("     =======================================================", LightYellow);
            Console.WriteLine();
            Console.ReadKey();
            Console.Clear();
        }

        #endregion
    }
}
