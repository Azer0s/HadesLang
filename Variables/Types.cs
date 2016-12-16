using System;
using System.ComponentModel;
using Microsoft.SqlServer.Server;

namespace Variables
{
    public class Types
    {
        public Types(AccessTypes access, DataTypes dataType, string value, string owner)
        {
            Access = access;
            DataType = dataType;
            Value = value;
            Owner = owner;
        }

        public AccessTypes Access { get; set; }
        public DataTypes DataType { get; set; }
        public string Value { get; set; }
        public string Owner { get; set; }
    }
}
