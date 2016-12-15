using System;
using System.ComponentModel;

namespace Variables
{
    public class Types
    {
        public Types(AccessTypes access, DataTypes dataType, string value)
        {
            Access = access;
            DataType = dataType;
            Value = value;
        }

        public AccessTypes Access { get; set; }
        public DataTypes DataType { get; set; }
        public string Value { get; set; }
    }
}
