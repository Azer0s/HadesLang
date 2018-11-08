using Hades.Common;
using Hades.Source;

namespace Hades.Error
{
    public sealed class Entry
    {
        public string[] Lines { get; }

        public string Message { get; }

        public Severity Severity { get; }

        public Span Span { get; }

        public Entry(string message, string[] lines, Severity severity, Span span)
        {
            Message = message;
            Lines = lines;
            Span = span;
            Severity = severity;
        }
    }
}