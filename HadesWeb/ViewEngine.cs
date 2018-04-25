using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Interpreter;

namespace HadesWeb
{
    public class ViewEngine
    {
        public byte[] RenderFile(string viewFile, Interpreter.Interpreter interpreter,FileInterpreter baseFile)
        {
            if (!File.Exists(viewFile))
            {
                return null;
            }

            var view = File.ReadAllLines(viewFile);
            
            //TODO: Add <hd> tags for script execution in hdhtml
            
            for (var i = 0; i < view.Length; i++)
            {
                if (RegexCollection.Store.ViewVariable.IsMatch(view[i]))
                {
                    view[i] = RegexCollection.Store.ViewVariable.Replace(view[i],
                        match => interpreter.InterpretLine(match.Groups[1].Value, new List<string> {baseFile.FAccess},
                            baseFile));
                }
            }            
            return Encoding.UTF8.GetBytes(string.Join("", view.ToArray()));
        }
    }
}