using System;

namespace Hades.Source
{
    public struct Location : IEquatable<Location>
    {
        public int Column { get; }
        
        public int Index { get; }
        
        public int Line { get; }

        public Location(int index, int line, int column)
        {
            Index = index;
            Line = line;
            Column = column;
        }

        public static bool operator !=(Location left, Location right)
        {
            return !left.Equals(right);
        }

        public static bool operator ==(Location left, Location right)
        {
            return left.Equals(right);
        }

        public override bool Equals(object obj)
        {
            if (obj is Location location)
            {
                return Equals(location);
            }
            return base.Equals(obj);
        }

        public bool Equals(Location other)
        {
            return other.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return 0xB1679EE ^ Index ^ Line ^ Column;
        }
    }
}