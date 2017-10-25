using System;
using System.Collections.Generic;

namespace Variables
{
    public class Methods
    {
        public Methods(string name, Tuple<int,int> postition, Dictionary<string, DataTypes> parameters, string guard)
        {
            Name = name;
            Postition = postition;
            Parameters = parameters;
            Guard = guard;
        }

        public string Name { get; set; }
        public Tuple<int,int> Postition { get; set; }
        public Dictionary<string,DataTypes> Parameters { get; set; }
        public string Guard { get; set; }
    }
}
