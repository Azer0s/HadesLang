using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    class FileInterpreter
    {
        public string FileName { get; set; }

        public FileInterpreter(string fileName)
        {
            FileName = fileName;
        }
    }
}
