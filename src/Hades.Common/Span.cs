using System;
using Hades.Source;

namespace Hades.Common
{
    public struct Span : IEquatable<Span>
    {
        public SourceLocation End { get; }
        
        public int Length => End.Index - Start.Index;
        
        public SourceLocation Start { get; }

        public Span(SourceLocation start, SourceLocation end)
        {
            Start = start;
            End = end;
        }

        public static bool operator !=(Span left, Span right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(Span left, Span right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Span span)
            {
                return Equals(span);
            }
            return base.Equals(obj);
        }

        public bool Equals(Span other)
        {
            return other.Start == Start && other.End == End;
        }

        public override int GetHashCode()
        {
            return 0x509CE ^ Start.GetHashCode() ^ End.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Start.Line} {Start.Column} {Length}";
        }
    }
}