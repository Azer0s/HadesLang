using System.Collections;
using System.Collections.Generic;
using Hades.Common;
using Hades.Source;

namespace Hades.Error
{
    public class Collector : IEnumerable<Entry>
    {
        private readonly List<Entry> _errors = new List<Entry>();
        
        public IEnumerable<Entry> Errors => _errors.AsReadOnly();
        public bool HasErrors => _errors.Count > 0;

        public void Add(string message, Code sourceCode, Severity severity, Span span)
        {
            _errors.Add(new Entry(message,sourceCode.GetLines(span.Start.Line, span.End.Line), severity, span));
        }
        public void Clear()
        {
            _errors.Clear();
        }
        
        public IEnumerator<Entry> GetEnumerator()
        {
            return _errors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}