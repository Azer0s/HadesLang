using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Output;
using StringExtension;
using Variables;
using static System.String;

namespace Interpreter
{
    public class FileInterpreter : IVariable
    {
        public List<string> Lines;
        public List<Methods> Functions = new List<Methods>();
        public string FAccess;
        public FileInterpreter(string path)
        {
            try
            {
                Lines = File.ReadAllLines(path).ToList();
            }
            catch (Exception e)
            {
                throw new Exception($"File {path} could not be read!");
            }

            FAccess = path;

            for (var i = 0; i < Lines.Count; i++)
            {
                if (!IsNullOrEmpty(Lines[i]))
                {
                    Lines[i] = RegexCollection.Store.IgnoreTabsAndSpaces.Match(Lines[i]).Groups[1].Value;
                }
                else
                {
                    Lines.RemoveAt(i);
                    i--;
                }
            }

            LoadFunctions();
        }

        public string Execute(Interpreter interpreter, string access, int start = 0, int end = -1)
        {
            var output = interpreter.Output;
            interpreter.Output = new NoOutput();

            end = end != -1 ? end : Lines.Count;

            for (var i = start; i < end; i++)
            {
                //Function
                if (RegexCollection.Store.FunctionDecleration.IsMatch(Lines[i]))
                {
                    i = GetBlock("func", i, RegexCollection.Store.FunctionDecleration).end;
                    continue;
                }

                //Case
                if (RegexCollection.Store.Case.IsMatch(Lines[i]))
                {
                    var block = GetBlock("case", i, RegexCollection.Store.Case);
                    var groups = RegexCollection.Store.Case.Match(Lines[i]).Groups.OfType<Group>().Select(a => a.Value)
                        .ToArray();

                    if (bool.Parse(interpreter.InterpretLine(groups[1],access,this,FAccess)))
                    {
                        var result = Execute(interpreter,access: access, start: block.start + 1, end: block.end);

                        if (!IsNullOrEmpty(result))
                        {
                            interpreter.Output = output;
                            return result;
                        }
                    }

                    i = block.end;
                    continue;
                }

                //AsLongAs
                if (RegexCollection.Store.AsLongAs.IsMatch(Lines[i]))
                {
                    var block = GetBlock("asLongAs", i, RegexCollection.Store.AsLongAs);
                    var groups = RegexCollection.Store.AsLongAs.Match(Lines[i]).Groups.OfType<Group>().Select(a => a.Value)
                        .ToArray();

                    while (bool.Parse(interpreter.InterpretLine(groups[1], access,this,FAccess)))
                    {
                        var result = Execute(interpreter, start: block.start + 1, end: block.end,access:access);

                        if (!IsNullOrEmpty(result))
                        {
                            interpreter.Output = output;
                            return result;
                        }
                    }

                    i = block.end;
                    continue;
                }

                //TODO: Implement tasking

                //Put
                if (RegexCollection.Store.Put.IsMatch(Lines[i]))
                {
                    interpreter.Output = output;
                    return interpreter.InterpretLine(RegexCollection.Store.Put.Match(Lines[i]).Groups[1].Value,access,this,FAccess);
                }

                var interresult = interpreter.InterpretLine(Lines[i], access, this,FAccess);
                //Function call
                if (RegexCollection.Store.Function.IsMatch(Lines[i]) && Functions.Any(a => a.Name == RegexCollection.Store.Function.Match(Lines[i]).Groups[1].Value))
                {
                    if (!IsNullOrEmpty(interresult))
                    {
                        interpreter.Output = output;
                        return interresult;
                    }
                }
            }

            interpreter.Output = output;
            return Empty;
        }

        public string CallFunction(string function,Interpreter interpreter)
        {
            var groups = RegexCollection.Store.Function.Match(function).Groups.OfType<Group>().Select(a => a.Value)
                .ToList();
            if (Functions.Any(a => a.Name == groups[1]))
            {
                var func = Functions.First(a => a.Name == groups[1]);

                var expectedArgs = func.Parameters.ToList();
                var args = groups[2].StringSplit(',').ToList();

                if (expectedArgs.Count != args.Count)
                {
                    return $"Invalid function call: {function}!";
                }

                for (var i = 0; i < args.Count; i++)
                {
                    interpreter.Evaluator.CreateVariable($"{expectedArgs[i].Key} as {expectedArgs[i].Value.ToString().ToLower()} closed = {args[i]}",$"{FAccess}@{groups[1]}",interpreter,this,FAccess);
                }

                var result = Execute(interpreter, start: func.Postition.Item1+1, end: func.Postition.Item2,access: $"{FAccess}@{groups[1]}");

                interpreter.Evaluator.Unload("all", $"{FAccess}@{groups[1]}");

                return result;
            }
            throw new Exception($"Function {function} does not exist!");
        }

        private void LoadFunctions()
        {
            var index = 0;
            foreach (var line in Lines)
            {
                //Function
                if (RegexCollection.Store.FunctionDecleration.IsMatch(line))
                {
                    var block = GetBlock("func", index, RegexCollection.Store.FunctionDecleration);
                    var groups = RegexCollection.Store.FunctionDecleration.Match(line).Groups.OfType<Group>()
                        .Select(a => a.Value).ToArray();

                    var arguments = new Dictionary<string, DataTypes>();
                    foreach (var s in groups[2].StringSplit(',').ToList())
                    {
                        var arg = s.TrimStart(' ').TrimEnd(' ');
                        if (RegexCollection.Store.Argument.IsMatch(arg))
                        {
                            var argsGroups = RegexCollection.Store.Argument.Match(arg).Groups.OfType<Group>()
                                .Select(a => a.Value).ToArray();
                            arguments.Add(argsGroups[2],TypeParser.ParseDataType(arg.Split(' ')[0]));
                        }
                    }

                    Functions.Add(new Methods(groups[1],block.ToTuple(),arguments));
                }
                index++;
            }
        }

        private (int start, int end) GetBlock(string delimiter, int line,Regex toCheck)
        {
            var buffer = 0;
            for (var i = line+1; i < Lines.Count; i++)
            {
                if (toCheck.IsMatch(Lines[i]))
                {
                    buffer++;
                }

                if (string.Equals(Lines[i].ToLower(), $"end{delimiter.ToLower()}", StringComparison.CurrentCultureIgnoreCase))
                {
                    if (buffer != 0)
                    {
                        buffer--;
                    }
                    else
                    {
                        return (line, i);
                    }
                }
            }

            throw new Exception($"Couldnt get end of block: {Lines[line]}!");
        }
    }
}
