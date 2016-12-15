using System;

namespace Variables
{
    public class Types
    {
        public Types(AccessTypes access, string value)
        {
            Access = access;
            Value = value;
        }

        public AccessTypes Access { get; set; }
        public string Value { get; set; }
    }
}
