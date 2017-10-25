using System;
using static System.String;

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
            if (!IsNullOrEmpty(input))
            {
                Console.Write(input.Replace("\\n", "\n").Replace("\\t", "\t"));
            }
        }

        public void WriteLine(string input)
        {
            if (!IsNullOrEmpty(input))
            {
                Console.WriteLine(input.Replace("\\n", "\n").Replace("\\t", "\t"));
            }
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
