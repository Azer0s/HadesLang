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
            //ignore
        }

        public string ReadLine()
        {
            return "";
        }
    }
}
