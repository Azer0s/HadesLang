using System;

namespace Variables
{
    public class TypeParser
    {
        public static AccessTypes ParseAccessType(string type)
        {
            switch (type)
            {
                case "reachable":
                    return AccessTypes.REACHABLE;
                case "reachable_all":
                    return AccessTypes.REACHABLE_ALL;
                case "closed":
                    return AccessTypes.CLOSED;
                case "":
                    return AccessTypes.CLOSED;
                default:
                    throw new Exception("Invalid access operator!");
            }
        }

        public static DataTypes ParseDataType(string type)
        {
            switch (type)
            {
                case "num":
                    return DataTypes.NUM;
                case "dec":
                    return DataTypes.DEC;
                case "word":
                    return DataTypes.WORD;
                case "bit":
                    return DataTypes.BIT;
                case "object":
                    return DataTypes.OBJECT;
                default:
                    throw new Exception("Invalid data type!");
            }
        }
    }
}