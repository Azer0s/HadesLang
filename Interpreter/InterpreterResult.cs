namespace Interpreter
{
    public class InterpreterResult
    {
        public InterpreterResult(string message, bool shouldPrint)
        {
            Message = message;
            ShouldPrint = shouldPrint;
        }

        public string Message;
        public bool ShouldPrint;
    }
}