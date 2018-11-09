using System;

namespace Hades.Source
{
    public struct SourceLocation : IEquatable<SourceLocation>
    {
        public int Column { get; }
        
        public int Index { get; }
        
        public int Line { get; }

        public SourceLocation(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public static bool operator !=(SourceLocation left, SourceLocation right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(SourceLocation left, SourceLocation right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is SourceLocation location)
            {
                return Equals(location);
            }
            return base.Equals(obj);
        }

        public bool Equals(SourceLocation other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return 0xB1679EE ^ Index ^ Line ^ Column;
        }
    }
}