using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Variables;

namespace Interpreter
{
    public class FileInterpreter
    {
        public string FileName { get; set; }
        public List<string> Lines = new List<string>();
        public List<Methods> Methods = new List<Methods>();
        private Interpreter _interpreter = new Interpreter();

        public FileInterpreter(string fileName)
        {
            fileName = fileName.Replace("'", "");
            FileName = fileName;

            int counter = 0;
            string line;

            var file = new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                Lines.Add(line);
                counter++;
            }

            file.Close();
        }

        public void LoadAll()
        {
            foreach (var variable in Lines)
            {
                string operation;
                var result = _interpreter.InterpretLine(variable,FileName,out operation);

                if (result.Value && result.Key != string.Empty)
                {
                    Console.WriteLine(result.Key);
                }
            }

            foreach (var variable in Cache.Instance.Variables.ToList())
            {
                if (variable.Key.Item2 == FileName)
                {
                    Cache.Instance.Variables.Remove(variable.Key);
                }
            }
        }

        public void LoadFunctions()
        {
            //throw new NotImplementedException();
        }

        public void LoadReachableVars()
        {
            //throw new NotImplementedException();
        }
    }
}
