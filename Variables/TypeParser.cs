using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Exceptions;

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
                default:
                    throw new InvalidAccessTypeException("Invalid access operator!");
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
                case "binary":
                    return DataTypes.BINARY;
                default:
                    throw new InvalidDataTypeException("Invalid data type!");
            }
        }
    }
}
