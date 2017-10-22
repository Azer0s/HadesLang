﻿using System;

namespace Output
{
    /// <inheritdoc />
    /// <summary>
    /// Console IO
    /// </summary>
    public class ConsoleOutput : IScriptOutput
    {
        public void Write(string input)
        {
            Console.Write(input);
        }

        public void WriteLine(string input)
        {
            Console.WriteLine(input.Replace("\\n","\n").Replace("\\t","\t"));
        }

        public void Clear()
        {
            Console.Clear();
        }

        public string ReadLine()
        {
            return Console.ReadLine();
        }
    }
}
