using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StringExtension;
// ReSharper disable InconsistentNaming

namespace file
{
    public class Library
    {
        public string readAllLines(string path)
        {
            return $"{{'{string.Join("','",File.ReadAllLines(path.TrimStart('\'').TrimEnd('\'')))}'}}";
        }

        public string writeAllLines(string path, string data)
        {
            try
            {
                File.WriteAllLines(path.Trim('\''), data.Trim('{', '}').StringSplit(','));
                return "true";
            }
            catch (Exception e)
            {
                return "false";
            }
        }

        public string exists(string path)
        {
            return File.Exists(path.Trim('\'')).ToString().ToLower();
        }
    }
}
