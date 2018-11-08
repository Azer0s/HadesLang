using System;
using System.Linq;
using Hades.Common;
using Hades.Syntax;

namespace Hades.Source
{
    public sealed class Code
    {
        private readonly Lazy<string[]> _lines;
        private readonly string _sourceCode;

        public string Contents => _sourceCode;

        public string[] Lines => _lines.Value;

        public char this[int index] => _sourceCode.CharAt(index);

        public Code(string sourceCode)
        {
            _sourceCode = sourceCode;
            _lines = new Lazy<string[]>(() => _sourceCode.Split(new[] { Environment.NewLine }, StringSplitOptions.None));
        }

        public string GetLine(int line)
        {
            if (line < 1)
            {
                throw new IndexOutOfRangeException($"{nameof(line)} must not be less than 1!");
            }
            if (line > Lines.Length)
            {
                throw new IndexOutOfRangeException($"No line {line}!");
            }

            // Lines start at 1; array indexes start at 0.
            return Lines[line - 1];
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

        public string GetSpan(Span span)
        {
            int start = span.Start.Index;
            int length = span.Length;
            return _sourceCode.Substring(start, length);
        }

        public string GetSpan(Node node)
        {
            return GetSpan(node.Span);
        }
    }
}