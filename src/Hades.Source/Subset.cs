using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Hades.Source
{
    public class Subset<T> : IEnumerable<T>
    {
        private readonly int _end;
        private readonly IEnumerable<T> _set;

        private readonly int _start;

        private struct SubsetEnumerator : IEnumerator<T>
        {
            private bool _disposed;
            private int _index;
            private readonly Subset<T> _subset;

            public T Current => _subset._set.ElementAt(_index);

            object IEnumerator.Current => _subset._set.ElementAt(_index);

            public SubsetEnumerator(Subset<T> subset)
            {
                _disposed = false;
                _index = subset._start - 1; // MoveNext() appears to be called before get_Current.
                _subset = subset;
            }

            public void Dispose()
            {
                _disposed = true;
            }

            public bool MoveNext()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Subset");
                }
                if (_index == _subset._end)
                {
                    return false;
                }
                _index++;
                return true;
            }

            public void Reset()
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException("Subset");
                }
                _index = _subset._start;
            }
        }

        public Subset(IEnumerable<T> collection, int start, int end)
        {
            _set = collection;
            _start = start;
            _end = end;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new SubsetEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new SubsetEnumerator(this);
        }
    }
}