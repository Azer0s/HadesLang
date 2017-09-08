using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace file
{
    public class Library
    {
        public string readAllLines(string path)
        {
            return $"{{'{string.Join("','",File.ReadAllLines(path.TrimStart('\'').TrimEnd('\'')))}'}}";
        }
    }
}
