﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Debug;
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
        private readonly Dictionary<int, int> _lineMap;
        public string FAccess;
        public FileInterpreter(string path)
        {
            try
            {
                Lines = File.ReadAllLines(path).ToList();
                _lineMap = new Dictionary<int, int>(Lines.Count);
            }
            catch (Exception e)
            {
                throw new Exception($"File {path} could not be read!");
            }

            FAccess = path;
            var c = 1;
            for (var i = 0; i < Lines.Count; i++, c++)
            {
                if (!IsNullOrEmpty(Lines[i]))
                {
                    var ignored = RegexCollection.Store.IgnoreTabsAndSpaces.Match(Lines[i]).Groups[1].Value;
                    if (!ignored.StartsWith("//"))
                    {
                        Lines[i] = ignored;
                        _lineMap[i] = c;
                    }
                    else
                    {
                        Lines.RemoveAt(i);
                        i--;
                    }
                }
                else
                {
                    Lines.RemoveAt(i);
                    i--;
                }
            }

            LoadFunctions();
        }

        public (string Value,bool Return) Execute(Interpreter interpreter, string access, int start = 0, int end = -1)
        {
            var output = interpreter.Output;
            interpreter.Output = new NoOutput();

            end = end != -1 ? end : Lines.Count;

            for (var i = start; i < end; i++)
            {
                //Debug
                if (Cache.Instance.Debug)
                {
                    HadesDebugger.EventManager.InvokeOnInterrupted(new DebugInfo { File = FAccess, Line = _lineMap[i], VarDump = interpreter.Evaluator.DumpVars(DataTypes.NONE) });
                }

                //Break
                if (Lines[i] == "break")
                {
                    return (Empty, false);
                }

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

                        if (!IsNullOrEmpty(result.Value) && result.Return)
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

                        if (!IsNullOrEmpty(result.Value) && result.Return)
                        {
                            interpreter.Output = output;
                            return result;
                        }
                    }

                    i = block.end;
                    continue;
                }

                //IterateFor
                if (RegexCollection.Store.IterateFor.IsMatch(Lines[i]))
                {
                    var block = GetBlock("iterateFor", i, RegexCollection.Store.IterateFor);
                    var groups = RegexCollection.Store.IterateFor.Match(Lines[i]).Groups.OfType<Group>()
                        .Select(a => a.Value).ToList();
                    interpreter.Evaluator.CreateVariable($"{groups[2]} as {groups[1]} closed", access, interpreter, this);

                    var array = RegexCollection.Store.ArrayValues.IsMatch(groups[3]) ? groups[3] : interpreter.InterpretLine(groups[3], access, this);
                    array = array.TrimStart('{').TrimEnd('}');

                    foreach (var iterator in array.StringSplit(',').ToList())
                    {
                        interpreter.Evaluator.AssignToVariable($"{groups[2]} = {iterator}", access, false, interpreter,this);
                        var result = Execute(interpreter, start: block.start + 1, end: block.end, access: access);

                        if (!IsNullOrEmpty(result.Value) && result.Return)
                        {
                            interpreter.Output = output;
                            interpreter.Evaluator.Unload(groups[2], access);
                            return result;
                        }
                    }
                    interpreter.Evaluator.Unload(groups[2], access);
                    i = block.end;
                    continue;
                }
                //TODO: Implement tasking

                //Put
                if (RegexCollection.Store.Put.IsMatch(Lines[i]))
                {
                    var result = (interpreter.InterpretLine(RegexCollection.Store.Put.Match(Lines[i]).Groups[1].Value, access, this, FAccess), true);
                    interpreter.Output = output;
                    return result;
                }

                var interresult = interpreter.InterpretLine(Lines[i], access, this,FAccess);
                //Function call
                if (RegexCollection.Store.Function.IsMatch(Lines[i]) && Functions.Any(a => a.Name == RegexCollection.Store.Function.Match(Lines[i]).Groups[1].Value))
                {
                    var result = Execute(interpreter, access, i + 1);
                    if (!IsNullOrEmpty(result.Value) && result.Return)
                    {
                        return result;
                    }
                    if (!IsNullOrEmpty(interresult))
                    {
                        interpreter.Output = output;
                        return (interresult,false);
                    }
                }
            }

            interpreter.Output = output;
            return (Empty,false);
        }

        public string CallFunction(string function,Interpreter interpreter)
        {
            var groups = RegexCollection.Store.Function.Match(function).Groups.OfType<Group>().Select(a => a.Value)
                .ToList();
            var access = Guid.NewGuid().ToString().ToLower();
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
                    interpreter.Evaluator.CreateVariable($"{expectedArgs[i].Key} as {expectedArgs[i].Value.ToString().ToLower()} closed = {args[i]}",access,interpreter,this);
                }

                var result = Execute(interpreter, start: func.Postition.Item1+1, end: func.Postition.Item2,access:access);

                interpreter.Evaluator.Unload("all", access);

                return result.Value;
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
