namespace Hades.Output
{
    /// <summary>
    /// Interface for IO - Can be implemented for custom IO
    /// </summary>
    public interface IScriptOutput
    {
        void Write(string input);
        void WriteLine(string input);
        void Clear();
        string ReadLine();
    }
}
