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
        public string readAllText(string path)
	{
	    return $"'{File.ReadAllText(path.TrimStart('\'').TrimEnd('\''))}'";
	}

	public string readAllLines(string path)
        {
            var lines = File.ReadAllLines(path.TrimStart('\'').TrimEnd('\''));
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = lines[i].Replace("'", "\"");
            }
            return $"{{'{string.Join("','",lines)}'}}";
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
