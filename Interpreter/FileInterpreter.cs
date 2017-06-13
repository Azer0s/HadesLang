using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exceptions;
using StringExtension;
using Variables;

namespace Interpreter
{
    /// <summary>
    /// TODO: Documentation
    /// </summary>
    public class FileInterpreter
    {
        public string FileName { get; set; }
        public List<string> Lines = new List<string>();
        public List<Methods> Methods = new List<Methods>();
        public KeyValuePair<string, Types> Return;
        public IScriptOutput Output;
        public Dictionary<string, string> Parameters;
        private readonly Interpreter _interpreter;
        private bool _stop;
        private readonly List<int> _nextBreak = new List<int>();
        private bool _breakMode;

        public FileInterpreter(string fileName,IScriptOutput output)
        {
            fileName = fileName.Replace("'", "");
            FileName = fileName;
            Output = output;

            _interpreter = new Interpreter(output);

            int counter = 0;
            string line;

            var file = new System.IO.StreamReader(fileName);
            while ((line = file.ReadLine()) != null)
            {
                Lines.Add(StripComments(line).TrimStart().Replace("\t",""));
                counter++;
            }

            file.Close();
        }

        private static string StripComments(string code)
        {
            return Regex.Replace(code, @"(@(?:""[^""]*"")+|""(?:[^""\n\\]+|\\.)*""|'(?:[^'\n\\]+|\\.)*')|//.*|/\*(?s:.*?)\*/", "$1");
        }

        public FileInterpreter(IEnumerable<string> lines, List<Methods> methods, IScriptOutput output)
        {
            Lines = new List<string>(lines);
            Methods = methods;
            _interpreter = new Interpreter(output);
            Output = output;
            Parameters = new Dictionary<string, string>();
        }

        public void LoadAll()
        {
            LoadFunctions();
            ExecuteFromLineToLine(new Tuple<int, int>(-1, Lines.Count),true,out Return);
            Collect();      
        }

        public void Collect()
        {
            if (Cache.Instance.EraseVars)
            {
                foreach (var variable in Cache.Instance.Variables.ToList())
                {
                    if (variable.Key.Item2 == FileName || Cache.Instance.LoadFiles.Contains(variable.Key.Item2))
                    {
                        Cache.Instance.Variables.Remove(variable.Key);
                    }
                }
            }
        }

        public void ExecuteFromLineToLine(Tuple<int, int> fromTo,bool firstLevel,out KeyValuePair<string, Types> returnVal)
        {
            returnVal = new KeyValuePair<string, Types>();
            for (int i = fromTo.Item1 + 1; i <= fromTo.Item2; i++)
            {
                string operation;

                if (i > Lines.Count - 1)
                {
                   break; 
                }

                //Line contains function parameters
                if (Lines[i].Contains("param"))
                {
                    var param = Regex.Matches(Lines[i], "(param.)+[a-zA-Z 0-9]+");

                    foreach (Match o in param)
                    {
                        var paramName = o.Value.Replace("param.", "");
                        try
                        {
                            Lines[i] = Lines[i].Replace(o.Value, Parameters[paramName]);
                        }
                        catch (Exception)
                        {
                            throw new VariableNotDefinedException("Parameter was not found!");
                        }
                    }
                }

                //return
                if (Lines[i].StartsWith("put"))
                {
                    var returnVar = Lines[i].Split(' ');

                    if (returnVar[1].Contains(':') || returnVar[1].Contains("->",StringComparison.Ordinal))
                    {
                        var val = returnVar[1].Contains(':') ? _interpreter.Evaluator.EvaluateCall(returnVar[1].Split(':'), FileName).Key : _interpreter.Evaluator.EvaluateCall(returnVar[1].Split(new[] { "->" }, StringSplitOptions.None), FileName).Key;
                        returnVal = new KeyValuePair<string, Types>("",new Types(AccessTypes.REACHABLE, _interpreter.Evaluator.DataTypeFromData(val,false), val)); 
                    }
                    else
                    {
                        returnVal = _interpreter.Evaluator.GetVariable(returnVar[1], FileName);
                    }
                    _stop = true;
                }

                //stop loop or case
                if (Lines[i] == "break")
                {
                    if (!firstLevel)
                    {
                        _breakMode = true;
                        return;
                    }
                    return;
                }

                //skip function
                if (Lines[i].StartsWith("func"))
                {
                    var func = GetLineToLine(i, "func");
                    i = func.Item2;
                    continue;
                }

                //stop execution of script
                if (Lines[i] == "stopExec")
                {
                    _stop = true;
                    return;
                }  

                if (_stop)
                {
                    return;
                }

                var result = _interpreter.InterpretLine(Lines[i], FileName, out operation);

                if (_interpreter.Clear)
                {
                    _interpreter.Clear = false;
                    Output.Clear();
                }

                //case operation
                if (_interpreter.Evaluator.EvaluateOperation(operation) == OperationTypes.CASE)
                {
                    var LineToLine = GetLineToLine(i, "case");

                    if (bool.Parse(result.Key))
                    {
                        ExecuteFromLineToLine(LineToLine,false,out returnVal);
                        i = LineToLine.Item2;
                    }
                    else
                    {
                        i = LineToLine.Item2;
                    }
                }

                //Loop operation
                if (_interpreter.Evaluator.EvaluateOperation(operation) == OperationTypes.ASLONGAS)
                {
                    var lineToLine = GetLineToLine(i, "aslongas");
                    _nextBreak.Add(lineToLine.Item2);

                    while (bool.Parse(result.Key))
                    {
                        ExecuteFromLineToLine(lineToLine,false,out Return);
                        result = _interpreter.InterpretLine(Lines[i], FileName, out operation);

                        if (_stop)
                        {
                            return;
                        }

                        if (_breakMode && !firstLevel)
                        {
                            return;
                        }
                        if (_breakMode && firstLevel)
                        {
                            i = _nextBreak.Last();
                            _breakMode = false;
                            break;
                        }
                    }
                    i = lineToLine.Item2 + 1;
                }


                if (result.Value && result.Key != string.Empty)
                {
                    Output.WriteLine(result.Key);
                }
            }
        }

        private Tuple<int, int> GetLineToLine(int variable, string lookOut)
        {
            var currentLine = variable;
            var endCase = 0;
            var buffer = 0;

            for (var i = currentLine + 1; i < Lines.Count; i++)
            {
                if (Lines[i].ToLower().Replace(" ", "").Contains($"{lookOut}[") && Lines[i].ToLower().Contains("]") && Lines[i].ToLower() != $"end{lookOut}")
                {
                    buffer++;
                }
                if (Lines[i].ToLower().Replace(" ", "") == $"end{lookOut}")
                {
                    if (buffer != 0)
                    {
                        buffer--;
                    }
                    else
                    {
                        endCase = i;
                        goto ret;
                    }
                }
            }

            ret:
            return new Tuple<int, int>(currentLine, endCase);
        }

        public void LoadFunctions()
        {
            for (var i = 0; i < Lines.Count; i++)
            {
                if (!Lines[i].StartsWith("func")) continue;
                var func = GetLineToLine(i,"func");

                var parameters = new List<Tuple<string, DataTypes>>();
                Regex.Match(Lines[i], @"(\[)([^]]*)(\])").Value.TrimStart('[').TrimEnd(']').Split(',').ToList().ForEach(
                    a =>
                    {
                        try
                        {
                            var splitParam = a.TrimStart(' ').Split(' ');
                            var name = splitParam[1].Replace(" ", "");
                            var type = TypeParser.ParseDataType(splitParam[0].Replace(" ", ""));
                            parameters.Add(new Tuple<string, DataTypes>(name, type));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }  
                    });

                Methods.Add(new Methods(Lines[i].Split(' ')[1],func,parameters));
                i = func.Item2;
            }
        }

        public void LoadReachableVars()
        {
            //throw new NotImplementedException();
        }
    }
}
