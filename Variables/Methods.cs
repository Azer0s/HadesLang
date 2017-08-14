using System;
using System.Collections.Generic;

namespace Hades.Variables
{
    public class Methods
    {
        public Methods(string name, Tuple<int,int> postition, List<Tuple<string,DataTypes>> parameters)
        {
            Name = name;
            Postition = postition;
            Parameters = parameters;
        }

        public string Name { get; set; }
        public Tuple<int,int> Postition { get; set; }
        public List<Tuple<string, DataTypes>> Parameters { get; set; }
    }
}
