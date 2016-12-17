using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class FileInterpreter
    {
        public string FileName { get; set; }
        private List<string> lines = new List<string>();
        private Evaluator refEvaluator;

        public FileInterpreter(string fileName,ref Interpreter refEvaluator)
        {
            fileName = fileName.Replace("'", "");
            FileName = fileName;
            this.refEvaluator = refEvaluator;

            int counter = 0;
            string line;

            var file = new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                lines.Add(line);
                counter++;
            }

            file.Close();
        }

        public void LoadAll()
        {
            foreach (var variable in lines)
            {
                
            }
        }

        public void LoadFunctions()
        {
            throw new NotImplementedException();
        }

        public void LoadReachableVars()
        {
            throw new NotImplementedException();
        }
    }
}
