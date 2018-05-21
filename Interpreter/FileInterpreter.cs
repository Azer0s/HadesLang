using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        public string FAccess;
        private Dictionary<int, int> _lineMap;
        private readonly Dictionary<int, int> _blockCache;
        private readonly Dictionary<string, string> _requirements;

        public FileInterpreter(int order) : base(order)
        {
            //Default constructor
            _blockCache = new Dictionary<int, int>();
            _requirements = new Dictionary<string, string>();
        }

        public void Construct(Interpreter interpreter, Dictionary<string, string> construct)
        {
            if (construct.Count != _requirements.Count)
            {
                throw new Exception("Ctor requirements not satisfied!");
            }

            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);

            try
            {
                foreach (var val in _requirements)
                {
                    var assign = IsNullOrEmpty(construct[val.Key]) ? Empty : $" = {construct[val.Key]}";
                    interpreter.InterpretLine($"{val.Key} as {val.Value}{assign}", new List<string>(),this);
                }
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                interpreter.SetOutput(output.output, output.eOutput);
            }
        }

        public FileInterpreter(string path, int order, Interpreter interpreter, List<string> customLines = null) : base(order)
        {
            _blockCache = new Dictionary<int, int>();
            _requirements = new Dictionary<string, string>();
            var optimize = false;
            if (IsNullOrEmpty(path) && customLines != null)
            {
                Lines = customLines;
                _lineMap = new Dictionary<int, int>(Lines.Count);
            }
            else
            {
                try
                {
                    Lines = File.ReadAllLines(path).ToList();
                    _lineMap = new Dictionary<int, int>(Lines.Count);
                }
                catch (Exception)
                {
                    throw new Exception($"File {path} could not be read!");
                }
            }

            //Requires
            if (Lines.Any(a => RegexCollection.Store.Requires.IsMatch(a)))
            {
                var req = RegexCollection.Store.CtorParams.Matches(RegexCollection.Store.Requires
                    .Match(Lines.FirstOrDefault(a => RegexCollection.Store.Requires.IsMatch(a)) ?? throw new Exception("Invalid requires command!")).Groups[1].Value);

                foreach (Match o in req)
                {
                    _requirements.Add(o.Groups[3].Value,o.Groups[1].Value + (IsNullOrEmpty(o.Groups[2].Value) ? "" : $"[{o.Groups[2].Value}]"));
                }

                Lines.RemoveAll(a => RegexCollection.Store.Requires.IsMatch(a));
            }

            //Directives
            while (Lines.Any(a => a.StartsWith("%")))
            {
                //Imports
                while (Lines.Any(a => RegexCollection.Store.Import.IsMatch(a)))
                {
                    var directives = Lines.Where(a => RegexCollection.Store.Import.IsMatch(a)).Select(a => a).ToList();

                    foreach (var directive in directives)
                    {
                        if (RegexCollection.Store.Import.IsMatch(directive))
                        {
                            var importPath = RegexCollection.Store.Import.Match(directive).Groups[1].Value;
                            List<string> importedLines;
                            try
                            {
                                importedLines = File.ReadLines(importPath).ToList();
                            }
                            catch (Exception)
                            {
                                throw new Exception($"File {importPath} could not be read!");
                            }

                            if (importedLines.Count > 0)
                            {
                                Lines.InsertRange(Lines.IndexOf(directive), importedLines);
                            }
                            Lines.Remove(directive);
                        }
                        else
                        {
                            throw new Exception($"Invalid import directive {directive}");
                        }
                    }
                }

                //Import load
                while (Lines.Any(a => RegexCollection.Store.ImportLoad.IsMatch(a)))
                {
                    var directives = Lines.Where(a => RegexCollection.Store.ImportLoad.IsMatch(a)).Select(a => a)
                        .ToList();

                    foreach (var directive in directives)
                    {
                        if (RegexCollection.Store.ImportLoad.IsMatch(directive))
                        {
                            var importPath = RegexCollection.Store.ImportLoad.Match(directive).Groups[1].Value;
                            List<string> importedLines;
                            try
                            {
                                importedLines = new HttpClient()
                                    .GetAsync(importPath).Result
                                    .Content.ReadAsStringAsync()
                                    .Result.Split('\n').ToList();
                            }
                            catch (Exception)
                            {
                                throw new Exception($"URL {importPath} could not be opened!");
                            }

                            if (importedLines.Count > 0)
                            {
                                Lines.InsertRange(Lines.IndexOf(directive), importedLines);
                            }

                            Lines.Remove(directive);
                        }
                        else
                        {
                            throw new Exception($"Invalid import-load directive {directive}");
                        }
                    }
                }

                //Alias
                while (Lines.Any(a => RegexCollection.Store.Alias.IsMatch(a)))
                {
                    var directives = Lines.Where(a => RegexCollection.Store.Alias.IsMatch(a)).Select(a => a).ToList();

                    foreach (var directive in directives)
                    {
                        Lines.Remove(directive);
                        Evaluator.AliasManager.Register(directive);
                    }
                }

                if (Lines.Any(a => RegexCollection.Store.Optimize.IsMatch(a)))
                {
                    Lines.Remove("%optimize%");
                    optimize = true;
                }
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
                        ignored = Evaluator.AliasManager.AliasReplace(ignored);
                        //TODO: Escape for strings
                        if (ignored.Contains("//"))
                        {
                            ignored = RegexCollection.Store.SingleLineComment.Replace(ignored, "");
                        }

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

            if (optimize)
            {
                Optimize(interpreter);
            }

            LoadFunctions();
        }

        public void Optimize(Interpreter interpreter)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);

            foreach (var line in Lines)
            {
                if (RegexCollection.Store.IfOrUnless.IsMatch(line) 
                    || RegexCollection.Store.While.IsMatch(line)
                    || RegexCollection.Store.FunctionDecleration.IsMatch(line) 
                    || RegexCollection.Store.For.IsMatch(line)
                    || RegexCollection.Store.Put.IsMatch(line))
                {
                    continue;
                }

                try
                {
                    interpreter.OptimizeLine(line);
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            interpreter.SetOutput(output.output, output.eOutput);
        }

        public (string Value, bool Return) Execute(Interpreter interpreter, List<string> scopes, int start = 0, int end = -1)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);

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
                    i = GetBlock(i).end;
                    continue;
                }

                //If or unless
                if (RegexCollection.Store.IfOrUnless.IsMatch(Lines[i]))
                {
                    var block = GetBlock(i);
                    (int start, int end) elseBlock = (0, 0);
                    var elseLoc = 0;

                    try
                    {
                        //Get next else
                        for (var j = block.end + 1; j < Lines.Count; j++)
                        {
                            if (RegexCollection.Store.IfOrUnless.IsMatch(Lines[j]))
                            {
                                break;
                            }
                            if (Lines[j] == "else")
                            {
                                elseLoc = j;
                                break;
                            }
                        }

                        //If next else exists, get else block
                        if (elseLoc != 0)
                        {
                            elseBlock = GetBlock(elseLoc);
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }

                    var groups = RegexCollection.Store.IfOrUnless.Match(Lines[i]).Groups.OfType<Group>().Select(a => a.Value)
                        .ToArray();

                    (string Value, bool Return) ExecuteBetween()
                    {
                        //Execute lines between end and else
                        if (elseLoc > block.end)
                        {
                            var guid = Guid.NewGuid().ToString();
                            scopes.Insert(0,guid);
                            var result = Execute(interpreter, scopes, block.end, elseLoc);
                            interpreter.Evaluator.Unload("all", scopes);
                            scopes.RemoveAt(0);
                            return result;
                        }
                        return ("", false);
                    }

                    bool eval;
                    try
                    {
                        eval = Lines[i].StartsWith("if")
                            ? bool.Parse(interpreter.InterpretLine(groups[1], scopes, this))
                            : !bool.Parse(interpreter.InterpretLine(groups[1], scopes, this));
                    }
                    catch (Exception)
                    {
                        interpreter.ExplicitOutput.WriteLine("Value was not recognized as a valid bit!");
                        return (Empty, true);
                    }

                    if (eval)
                    {
                        var guid = Guid.NewGuid().ToString();
                        scopes.Insert(0, guid);
                        var result = Execute(interpreter, scopes, block.start+1, block.end);
                        interpreter.Evaluator.Unload("all", scopes);
                        scopes.RemoveAt(0);

                        //Return if put was called
                        if (!IsNullOrEmpty(result.Value) && result.Return)
                        {
                            interpreter.SetOutput(output.output, output.eOutput);
                            return result;
                        }

                        result = ExecuteBetween();
                        //Return if put was called
                        if (!IsNullOrEmpty(result.Value) && result.Return)
                        {
                            interpreter.SetOutput(output.output, output.eOutput);
                            return result;
                        }
                    }
                    else
                    {
                        var betweenResult = ExecuteBetween();
                        //Return if put was called
                        if (!IsNullOrEmpty(betweenResult.Value) && betweenResult.Return)
                        {
                            interpreter.SetOutput(output.output, output.eOutput);
                            return betweenResult;
                        }

                        //If else block exists
                        if (elseBlock.end != 0)
                        {
                            try
                            {
                                var guid = Guid.NewGuid().ToString();
                                scopes.Insert(0, guid);
                                var result = Execute(interpreter, scopes, elseBlock.start + 1, elseBlock.end);
                                interpreter.Evaluator.Unload("all", scopes);
                                scopes.RemoveAt(0);

                                if (!IsNullOrEmpty(result.Value) && result.Return)
                                {
                                    interpreter.SetOutput(output.output, output.eOutput);
                                    return result;
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }
                        }
                    }

                    if (elseBlock.end != 0)
                    {
                        block = elseBlock;
                    }

                    i = block.end;
                    continue;
                }

                //While
                if (RegexCollection.Store.While.IsMatch(Lines[i]))
                {
                    var block = GetBlock(i);
                    var groups = RegexCollection.Store.While.Match(Lines[i]).Groups.OfType<Group>().Select(a => a.Value)
                        .ToArray();
                    var guid = Guid.NewGuid().ToString();
                    scopes.Insert(0, guid);

                    while (bool.Parse(interpreter.InterpretLine(groups[1], scopes, this)))
                    {
                        var result = Execute(interpreter, start: block.start + 1, end: block.end, scopes:scopes);

                        if (!IsNullOrEmpty(result.Value) && result.Return)
                        {
                            interpreter.SetOutput(output.output, output.eOutput);
                            interpreter.Evaluator.Unload("all", scopes);
                            scopes.RemoveAt(0);
                            return result;
                        }
                        interpreter.Evaluator.Unload("all", scopes);
                    }
                    scopes.RemoveAt(0);

                    i = block.end;
                    continue;
                }

                //For
                if (RegexCollection.Store.For.IsMatch(Lines[i]))
                {
                    var block = GetBlock(i);
                    var groups = RegexCollection.Store.For.Match(Lines[i]).Groups.OfType<Group>()
                        .Select(a => a.Value).ToList();
                    var guid = Guid.NewGuid().ToString();
                    scopes.Insert(0,guid);
                    interpreter.Evaluator.CreateVariable($"{groups[2]} as {groups[1]} closed", scopes, interpreter, this);

                    var array = RegexCollection.Store.ArrayValues.IsMatch(groups[3]) ? groups[3] : interpreter.InterpretLine(groups[3], scopes, this);
                    array = array.TrimStart('{').TrimEnd('}');

                    void forUnload()
                    {
                        scopes.RemoveAt(0);
                        interpreter.Evaluator.Unload(groups[2], scopes);
                        scopes.RemoveAt(0);
                    }

                    foreach (var iterator in array.StringSplit(',').ToList())
                    {
                        var tempId = Guid.NewGuid().ToString();
                        scopes.Insert(0,tempId);

                        interpreter.Evaluator.AssignToVariable($"{groups[2]} = {iterator}", scopes, false, interpreter, this);
                        var result = Execute(interpreter, start: block.start + 1, end: block.end, scopes:scopes);

                        if (!IsNullOrEmpty(result.Value) && result.Return)
                        {
                            interpreter.SetOutput(output.output, output.eOutput);
                            forUnload();
                            return result;
                        }

                        interpreter.Evaluator.Unload("all", scopes);
                    }
                    forUnload();
                    i = block.end;
                    continue;
                }

                //Put
                if (RegexCollection.Store.Put.IsMatch(Lines[i]))
                {
                    var result = (interpreter.InterpretLine(RegexCollection.Store.Put.Match(Lines[i]).Groups[1].Value, scopes, this), true);
                    interpreter.SetOutput(output.output, output.eOutput);
                    return result;
                }

                interpreter.InterpretLine(Lines[i], scopes, this);
            }

            interpreter.SetOutput(output.output, output.eOutput);
            return (Empty, false);
        }

        private Methods GetMethodForGuard(Interpreter interpreter,List<Methods> methods, List<string> scopes, List<string> args)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), new NoOutput());
            foreach (var method in methods)
            {
                var guid = Guid.NewGuid().ToString();
                try
                {
                    SetArgVariables(interpreter, scopes, args, method.Parameters.ToList(), guid);

                    var tempScopes = scopes.ToList();
                    tempScopes.Clear();
                    tempScopes.Insert(0, FAccess);
                    tempScopes.Insert(0, guid);

                    if (bool.Parse(interpreter.InterpretLine(method.Guard, tempScopes, this)))
                    {
                        interpreter.SetOutput(output.output, output.eOutput);
                        return method;
                    }
                }
                catch (Exception)
                {
                    //ignored
                }
                finally
                {
                    interpreter.Evaluator.Unload("all", new List<string> { guid });
                }
            }
            interpreter.SetOutput(output.output, output.eOutput);
            throw new Exception("No function with valid guard found!");
        }

        private void SetArgVariables(Interpreter interpreter,List<string> scopes,List<string> args, List<KeyValuePair<string, DataTypes>> expectedArgs, string id)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), new NoOutput());
            for (var i = 0; i < args.Count; i++)
            {
                try
                {
                    args[i] = interpreter.InterpretLine(args[i], scopes, this);

                    interpreter.Evaluator.CreateVariable($"{expectedArgs[i].Key} as {expectedArgs[i].Value.ToString().ToLower()} closed = {args[i]}", new List<string> { id }, interpreter, this);
                }
                catch (Exception)
                {
                    interpreter.SetOutput(output.output, output.eOutput);
                    throw;
                }
            }
            interpreter.SetOutput(output.output, output.eOutput);
        }

        public string CallFunction(string function, Interpreter interpreter, List<string> scopes)
        {
            var groups = RegexCollection.Store.Function.Match(function).Groups.OfType<Group>().Select(a => a.Value)
                .ToList();
            var guid = Guid.NewGuid().ToString().ToLower();
            var args = groups[2].StringSplit(',').ToList();
            if (Functions.Any(a => a.Name == groups[1] && a.Parameters.Count == args.Count))
            {
                var funcs = Functions.Where(a => a.Name == groups[1] && a.Parameters.Count == args.Count).ToList();
                Methods func = null;

                if (funcs.Count > 1)
                {
                    if (funcs.Any(method => IsNullOrEmpty(method.Guard)))
                    {
                        throw new Exception($"Undefined function guard for {function}");
                    }

                    func = GetMethodForGuard(interpreter,funcs,scopes,args);
                }
                else
                {
                    func = funcs.First();
                }

                SetArgVariables(interpreter,scopes,args, func.Parameters.ToList(),guid);
                
                var tempScopes = scopes.ToList();
                tempScopes.Clear();
                tempScopes.Insert(0,FAccess);
                tempScopes.Insert(0,guid);
                var result = Execute(interpreter, start: func.Postition.Item1 + 1, end: func.Postition.Item2, scopes:tempScopes);

                interpreter.Evaluator.Unload("all", new List<string> { guid });

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
                    var block = GetBlock(index);
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
                            arguments.Add(argsGroups[2], TypeParser.ParseDataType(arg.Split(' ')[0]));
                        }
                    }

                    Functions.Add(new Methods(groups[1], block.ToTuple(), arguments, groups[3]));
                }
                index++;
            }
        }

        private (int start, int end) GetBlock(int line)
        {
            //Blockcache
            if (_blockCache.ContainsKey(line))
            {
                return (line, _blockCache[line]);
            }

            var buffer = 0;

            for (var i = line + 1; i < Lines.Count; i++)
            {
                if (RegexCollection.Store.For.IsMatch(Lines[i]) ||
                    RegexCollection.Store.While.IsMatch(Lines[i]) ||
                    RegexCollection.Store.FunctionDecleration.IsMatch(Lines[i]) ||
                    RegexCollection.Store.IfOrUnless.IsMatch(Lines[i]) ||
                    RegexCollection.Store.Else.IsMatch(Lines[i]))
                {
                    buffer++;
                }

                if (Lines[i].TrimEnd(' ', '\t') == "end")
                {
                    if (buffer != 0)
                    {
                        buffer--;
                    }
                    else
                    {
                        //Add value to blockcache
                        _blockCache.Add(line, i);
                        return (line, i);
                    }
                }
            }

            throw new Exception($"Couldnt get end of block: {Lines[line]}!");
        }

        public void Set(FileInterpreter obj)
        {
            FAccess = obj.FAccess;
            Functions = obj.Functions;
            Lines = obj.Lines;
            _lineMap = obj._lineMap;
        }
    }
}
