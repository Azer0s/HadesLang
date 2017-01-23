using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Variables
{
    public class Methods
    {
        public Methods(string name, Tuple<int,int> postition)
        {
            Name = name;
            Postition = postition;
        }

        public string Name { get; set; }
        public Tuple<int, int> Postition { get; set; }
    }
}
