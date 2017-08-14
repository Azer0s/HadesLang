using System.Collections.Generic;

namespace Variables
{
    public class Object : IVariable
    {
        public List<string> Lines { get; set; }
        public List<Methods> Methods { get; set; }
    }
}
