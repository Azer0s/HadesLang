using System;
using System.Linq;
using Hades.Common;
using Hades.Syntax;

namespace Hades.Source
{
    public sealed class SourceCode
    {
        private readonly Lazy<string[]> _lines;
        private readonly string _sourceCode;

        public string[] Lines => _lines.Value;

        public char this[int index] => _sourceCode.CharAt(index);

        public SourceCode(string sourceCode)
        {
            _sourceCode = sourceCode;
            _lines = new Lazy<string[]>(() => _sourceCode.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        }

        public string[] GetLines(int start, int end)
        {
            if (end < start)
            {
                throw new IndexOutOfRangeException("Cannot retrieve negative range!");
            }
            if (start < 1)
            {
                throw new IndexOutOfRangeException($"{nameof(start)} must not be less than 1!");
            }
            if (end > Lines.Length)
            {
                throw new IndexOutOfRangeException("Cannot retrieve more lines than exist in file!");
            }

            // Line indexes are offset by +1 compared to array indexes.
            return new Subset<string>(Lines, start - 1, end - 1).ToArray();
        }
    }
}