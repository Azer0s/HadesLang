using System;

namespace Output
{
    public class NoOutput : IScriptOutput
    {
        public void Write(string input)
        {
            //ignore
        }

        public void WriteLine(string input)
        {
            //ignore
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
