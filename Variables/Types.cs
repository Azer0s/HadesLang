using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.SqlServer.Server;

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
        public List<string> Lines { get; set; }
        public List<Methods> Methods { get; set; }
    }
}
