using System;

namespace Hades.Common.Source
{
    public sealed class SourceCode
    {
        private readonly Lazy<string[]> _lines;
        private readonly string _sourceCode;

        public SourceCode(string sourceCode)
        {
            _sourceCode = sourceCode;
            _lines = new Lazy<string[]>(() => _sourceCode.Split(new[] {Environment.NewLine}, StringSplitOptions.None));
        }

        public string[] Lines => _lines.Value;

        public char this[int index] => _sourceCode.CharAt(index);
    }
}