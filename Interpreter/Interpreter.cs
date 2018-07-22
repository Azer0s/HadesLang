using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Output;
using StringExtension;
using Variables;
using static System.String;
using Array = Variables.Array;
using Function = Variables.Function;
// ReSharper disable RedundantEnumerableCastCall

namespace Interpreter
{
    public class Interpreter : IDisposable
    {
        public IScriptOutput Output;
        public IScriptOutput ExplicitOutput;
        public bool MuteOut = false;
        public Evaluator Evaluator;
        private bool _error;
        private IScriptOutput _backup;

        public void Dispose()
        {
            Output = null;
            ExplicitOutput = null;
            Evaluator = null;
            _backup = null;
        }

        ~Interpreter()
        {
            Dispose();
        }

        public Interpreter(IScriptOutput output, IScriptOutput explicitOutput, bool error = false)
        {
            Output = output;
            ExplicitOutput = explicitOutput;
            _backup = explicitOutput;
            _error = error;
            Evaluator = new Evaluator();

            //Custom default library location
            Cache.Instance.LibraryLocation =
                File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Format("{0}deflib",Path.DirectorySeparatorChar))
                    ? File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Format("{0}deflib",Path.DirectorySeparatorChar))
                    : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!Directory.Exists(Cache.Instance.LibraryLocation))
            {
                if (!Directory.Exists(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + Cache.Instance.LibraryLocation))
                {
                    Cache.Instance.LibraryLocation = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar +
                                                     Cache.Instance.LibraryLocation;
                }
                else
                {
                    Output.WriteLine("Default-library location does not exist! Aborting!");
                    Output.ReadLine();
                    Environment.Exit(-1);
                }
            }

            Cache.Instance.Variables = new Dictionary<Meta, IVariable>();
            Cache.Instance.Functions = new List<Function>();
            Cache.Instance.LoadFiles = new List<string>();
            Cache.Instance.ReplacementWithoutBinary = Cache.Instance.Replacement
                .Where(a => !a.Key.StartsWith("@") && a.Key != "LSHIFT" && a.Key != "RSHIFT")
                .ToDictionary(a => a.Key, a => a.Value);

            try
            {
                //Add constants to CallCache
                Cache.Instance.CallCache.Add("e", (s, l, f) =>
                {
                    Output.WriteLine(Math.E.ToString(CultureInfo.InvariantCulture));
                    return Math.E.ToString(CultureInfo.InvariantCulture);
                });
                Cache.Instance.CallCache.Add("pi", (s, l, f) =>
                {
                    Output.WriteLine(Math.PI.ToString(CultureInfo.InvariantCulture));
                    return Math.PI.ToString(CultureInfo.InvariantCulture);
                });

                // Length
                RegisterFunction(new Function("length", objects =>
                {
                    var parameters = objects.Select(a => a.ToString()).ToList();
                    if (parameters.Count != 1 || IsNullOrEmpty(parameters[0]))
                    {
                        throw new Exception("Invalid invocation of method length!");
                    }

                    if (RegexCollection.Store.ObjCode.IsMatch(parameters[0]))
                    {
                        throw new Exception("Can't get length of an object!");
                    }

                    var length = RegexCollection.Store.ArrayValues.IsMatch(parameters[0])
                        ? parameters[0].TrimStart('{').TrimEnd('}').StringSplit(',').ToList().Count.ToString()
                        : parameters[0].Length.ToString();
                    Output.WriteLine(length);
                    return length;
                }));

                //Position in
                RegisterFunction(new Function("positionIn", objects =>
                {
                    var parameters = objects.Select(a => a.ToString()).ToList();

                    if (parameters.Count != 2 
                        || !RegexCollection.Store.ArrayValues.IsMatch(parameters[0]) || IsNullOrEmpty(parameters[0]) 
                        || RegexCollection.Store.ArrayValues.IsMatch(parameters[1]) || IsNullOrEmpty(parameters[1]))
                    {
                        throw new Exception("Invalid invocation of method positionIn!");
                    }

                    var returndat = parameters[0].TrimStart('{').TrimEnd('}').StringSplit(',').ToList().IndexOf(parameters[1])
                        .ToString();
                    Output.WriteLine(returndat);
                    return returndat;
                }));

                //Range
                RegisterFunction(new Function("range", objects =>
                {
                    var parameters = objects.Select(a => a.ToString()).ToList();

                    if (parameters.Count != 2 || !int.TryParse(parameters[0], out _) ||
                        !int.TryParse(parameters[1], out _))
                    {
                        throw new Exception("Invalid invocation of method range!");
                    }

                    var start = int.Parse(parameters[0]);
                    var end = int.Parse(parameters[1]);

                    var rangeArray = start < end ? $"{{{Join(",", Enumerable.Range(start,end+1))}}}" : $"{{{Join(",", Enumerable.Range(end, (start - end) + 1).Reverse())}}}";
                    Output.WriteLine(rangeArray);
                    return rangeArray;
                }));

                //Random number
                RegisterFunction(new Function("rand", objects =>
                {
                    var parameters = objects.Select(a => a.ToString()).ToList();

                    if (parameters.Count != 1 || !int.TryParse(parameters[0], out _))
                    {
                        throw new Exception("Invalid invocation of method rand!");
                    }

                    var random = new Random().Next(int.Parse(parameters[0])).ToString();
                    Output.WriteLine(random);
                    return random;
                }));
            }
            catch (Exception)
            {
                // ignored
            }   
        }

        public void SetOutput(IScriptOutput output, IScriptOutput explicitOutput)
        {
            Output = output;
            ExplicitOutput = explicitOutput;
        }

        public (IScriptOutput output, IScriptOutput eOutput) GetOutput()
        {
            return (Output, ExplicitOutput);
        }

        public string InterpretLine(string lineToInterprete, List<string> scopes, FileInterpreter file, bool writeSettings = false, bool optimize = false)
        {
            if (IsNullOrEmpty(lineToInterprete) || lineToInterprete == "end")
            {
                return Empty;
            }

            #region Alias

            //Register aliases
            if (lineToInterprete.StartsWith("%") && lineToInterprete.EndsWith("%"))
            {
                try
                {
                    Evaluator.AliasManager.Register(lineToInterprete);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                    Error(e);
                }
                return Empty;
            }

            //Replacement for alias
            lineToInterprete = Evaluator.AliasManager.AliasReplace(lineToInterprete);

            //Alias info
            if (lineToInterprete == "aliasinfo")
            {
                if (writeSettings)
                {
                    foreach (var keyValuePair in Cache.Instance.Alias)
                    {
                        ExplicitOutput.WriteLine($"{keyValuePair.Key}<=>{keyValuePair.Value}");
                    }
                    return Empty;
                }
            }

            #endregion

            #region Pipeline

            if (lineToInterprete.Contains("|>"))
            {
                if (Cache.Instance.Pipelined.ContainsKey(lineToInterprete))
                {
                    lineToInterprete = Cache.Instance.Pipelined[lineToInterprete];
                }
                else
                {
                    if (RegexCollection.Store.Pipeline.IsMatch(lineToInterprete))
                    {
                        var stringDict = new Dictionary<string, string>();
                        var tempLine = lineToInterprete;

                        foreach (Match variable in RegexCollection.Store.IsWord.Matches(tempLine))
                        {
                            var guid = Guid.NewGuid().ToString().ToLower();
                            tempLine = tempLine.Replace(variable.Value, guid);
                            stringDict.Add(guid, variable.Value);
                        }


                        var matches = tempLine.Split(new[] {"|>"}, StringSplitOptions.None).Select(a => a.Trim()).ToList();

                        if (matches[0].Closes('[',']'))
                        {
                            if (matches[0].Contains("="))
                            {
                                Cache.Instance.Pipelined.Add(lineToInterprete, lineToInterprete);
                            }
                            else
                            {
                                for (var i = 0; i < matches.Count; i++)
                                {
                                    if (i + 1 == matches.Count)
                                    {
                                        tempLine = matches.Last();
                                    }
                                    else
                                    {
                                        matches[i + 1] = matches[i + 1].Replace("??", matches[i].Trim());
                                    }
                                }

                                tempLine = stringDict.Aggregate(tempLine, (current, variable) => current.Replace(variable.Key, variable.Value));
                                Cache.Instance.Pipelined.Add(lineToInterprete, tempLine);
                                lineToInterprete = tempLine;
                            }
                        }  
                    }
                }
            }

            #endregion

            //FAccess addition
            if (file != null && !scopes.Contains(file.FAccess))
            {
                scopes.Insert(0,file.FAccess);
            }

            //Call cached call
            if (Cache.Instance.CallCache.ContainsKey(lineToInterprete) && !optimize)
            {
                var result = Cache.Instance.CallCache[lineToInterprete].Invoke(lineToInterprete, scopes, file);
                if (result != null)
                {
                    return result;
                }
            }

            //Variable decleration
            if (RegexCollection.Store.CreateVariable.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,VarDecleration);
                return !optimize ? VarDecleration(lineToInterprete, scopes, file) : null;
            }

            //Array decleration
            if (RegexCollection.Store.CreateArray.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,ArrayDecleration);
                return !optimize ? ArrayDecleration(lineToInterprete, scopes, file) : null;
            }

            //Array assignment
            if (RegexCollection.Store.ArrayAssignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,ArrayAssignment);
                return !optimize ? ArrayAssignment(lineToInterprete, scopes, file) : null;
            }

            //Variable assignment
            if (RegexCollection.Store.Assignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,VarAssignment);
                return !optimize ? VarAssignment(lineToInterprete, scopes, file) : null;
            }

            //Operator assignment
            if (RegexCollection.Store.OpAssignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,OperatorAssignment);
                return !optimize ? OperatorAssignment(lineToInterprete, scopes, file) : null;
            }

            //Method calls
            if (RegexCollection.Store.MethodCall.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,MethodCalls);
                return !optimize ? MethodCalls(lineToInterprete, scopes,file) : null;
            }

            //Var call assign
            if (RegexCollection.Store.VarCallAssign.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, VarCallAssign);
                return !optimize ? VarCallAssign(lineToInterprete, scopes, file) : null;
            }

            //Var call
            if (RegexCollection.Store.VarCall.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,VarCall);
                return !optimize ? VarCall(lineToInterprete, scopes,file) : null;
            }

            //Function
            if (RegexCollection.Store.Function.IsMatch(lineToInterprete) &&
                !lineToInterprete.Remainder(RegexCollection.Store.Outside)
                .ContainsFromList(Cache.Instance.CharList.Concat(Cache.Instance.Replacement.Keys)) &&
                (lineToInterprete.IsValidFunction() || lineToInterprete.NestedFunction(RegexCollection.Store.FunctionParam)))
            {
                //Exit
                if (RegexCollection.Store.Exit.IsMatch(lineToInterprete) && !optimize)
                {
                    Evaluator.Exit(lineToInterprete);
                }

                var groups = RegexCollection.Store.Function.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

                //Method call from file
                if (file != null)
                {
                    if (file.Functions.Any(a => a.Name == groups[1].Value))
                    {
                        try
                        {
                            return !optimize ? file.CallFunction(lineToInterprete, this,scopes) : null;
                        }
                        catch (Exception e)
                        {
                            Error(e);
                        }
                    }
                }

                //Custom functions
                if (Cache.Instance.Functions.Any(a => a.Name == groups[1].Value))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,CallCustomFunction);
                    return !optimize ? CallCustomFunction(lineToInterprete,scopes,file) : null;
                }

                //Out
                if (RegexCollection.Store.Output.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,CallOut);
                    return !optimize ? CallOut(lineToInterprete, scopes, file) : null;
                }

                //Unload
                if (RegexCollection.Store.Unload.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,Unload);
                    return !optimize ? Unload(lineToInterprete, scopes,file) : null;
                }

                //Raw
                if (RegexCollection.Store.Raw.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,CallRaw);
                    return !optimize ? CallRaw(lineToInterprete, scopes, file) : null;
                }

                //GetFields
                if (RegexCollection.Store.Fields.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetFields);
                    return !optimize ? GetFields(lineToInterprete, scopes, file) : null;
                }

                #region Console-Specific

                //Input
                if (RegexCollection.Store.Input.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetInput);
                    return !optimize ? GetInput(lineToInterprete,scopes,file) : null;
                }

                //Type/dtype
                if (RegexCollection.Store.Type.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetType);
                    return !optimize ? GetType(lineToInterprete, scopes, file) : null;
                }

                //Exists
                if (RegexCollection.Store.Exists.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,Exists);
                    return !optimize ? Exists(lineToInterprete, scopes,file) : null;
                }

                //Not in callcache
                #region Settings

                if (writeSettings)
                {
                    //ScriptOutput
                    if (RegexCollection.Store.ScriptOutput.IsMatch(lineToInterprete))
                    {
                        switch (RegexCollection.Store.ScriptOutput.Match(lineToInterprete).Groups[1].Value)
                        {
                            case "0":
                                _backup = ExplicitOutput;
                                ExplicitOutput = new NoOutput();
                                Output.WriteLine("Script output disabled!");
                                return Empty;
                            case "1":
                                ExplicitOutput = _backup;
                                Output.WriteLine("Script output enabled!");
                                return Empty;
                            default:
                                Output.WriteLine("Invalid setting!");
                                return Empty;
                        }
                    }

                    //EraseVars
                    if (RegexCollection.Store.EraseVars.IsMatch(lineToInterprete))
                    {
                        switch (RegexCollection.Store.EraseVars.Match(lineToInterprete).Groups[1].Value)
                        {
                            case "0":
                                Cache.Instance.EraseVars = false;
                                Output.WriteLine("Garbage collection disabled!");
                                return Empty;
                            case "1":
                                Cache.Instance.EraseVars = true;
                                Output.WriteLine("Garbage collection enabled!");
                                return Empty;
                            default:
                                Output.WriteLine("Invalid setting!");
                                return Empty;
                        }
                    }

                    //Debug
                    if (RegexCollection.Store.Debug.IsMatch(lineToInterprete))
                    {
                        switch (RegexCollection.Store.Debug.Match(lineToInterprete).Groups[1].Value)
                        {
                            case "0":
                                Cache.Instance.Debug = false;
                                Output.WriteLine("Debug disabled!");
                                return Empty;
                            case "1":
                                Cache.Instance.Debug = true;
                                Output.WriteLine("Debug enabled!");
                                return Empty;
                            default:
                                Output.WriteLine("Invalid setting!");
                                return Empty;
                        }
                    }

                    //Cache calculations
                    if (RegexCollection.Store.CacheCalculations.IsMatch(lineToInterprete))
                    {
                        switch (RegexCollection.Store.CacheCalculations.Match(lineToInterprete).Groups[1].Value)
                        {
                            case "0":
                                Cache.Instance.CacheCalculation = false;
                                Output.WriteLine("Caching disabled!");
                                return Empty;
                            case "1":
                                Cache.Instance.CacheCalculation = true;
                                Output.WriteLine("Caching enabled!");
                                return Empty;
                            default:
                                Output.WriteLine("Invalid setting!");
                                return Empty;
                        }
                    }

                    //Dumpvars
                    if (RegexCollection.Store.DumpVars.IsMatch(lineToInterprete))
                    {
                        var dataTypeAsString = RegexCollection.Store.DumpVars.Match(lineToInterprete).Groups[1].Value;
                        var result = Evaluator.DumpVars(dataTypeAsString == "all"
                            ? DataTypes.NONE
                            : TypeParser.ParseDataType(dataTypeAsString));

                        if (!IsNullOrEmpty(result))
                        {
                            Output.WriteLine(result);
                        }
                        return Empty;
                    }
                }

                #endregion

                #endregion

                return Empty;
            }

            //Return array value
            if (RegexCollection.Store.ArrayVariable.IsMatch($"${lineToInterprete.TrimStart('$')}") && !RegexCollection.Store.IsWord.IsMatch(lineToInterprete.Remainder(RegexCollection.Store.Variable)))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,GetArrayValue);
                return !optimize ? GetArrayValue(lineToInterprete, scopes, file) : null;
            }

            //In/Decrease
            if (RegexCollection.Store.InDeCrease.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,InDecrease);
                return !optimize ? InDecrease(lineToInterprete, scopes, file) : null;
            }

            //Return string
            if (RegexCollection.Store.IsPureWord.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,ReturnWordValue);
                return !optimize ? ReturnWordValue(lineToInterprete,scopes,file) : null;
            }

            //Include library or file
            if (RegexCollection.Store.With.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,IncludeLibOrFile);
                return !optimize ? IncludeLibOrFile(lineToInterprete, scopes,file) : null;
            }
            
            //Calculation & string concat
            if ((lineToInterprete.Replace("->","").ContainsFromList(Cache.Instance.CharList) ||
                lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys)) &&
                (!RegexCollection.Store.IsWord.IsMatch(lineToInterprete) ||
                lineToInterprete.Remainder(RegexCollection.Store.IsWord)
                .ContainsFromList(Cache.Instance.CharList.Concat(Cache.Instance.Replacement.Keys.ToList())) &&
                !lineToInterprete.StartsWith("#")))
            {
                var result = !optimize ? Calculate(lineToInterprete, scopes, file) : null;
                if (result != null)
                {
                    try
                    {
                        Cache.Instance.CallCache.Add(lineToInterprete, Calculate);
                    }
                    catch (Exception e)
                    {
                        Error(e);
                    }
                    return result;
                }
            }

            //Clear console
            if (lineToInterprete.ToLower().Replace(" ", "") == "clear")
            {
                Cache.Instance.CallCache.Add(lineToInterprete,Clear);
                return !optimize ? Clear(lineToInterprete,scopes,file) : null;
            }

            //Bool type
            if (RegexCollection.Store.IsBit.IsMatch(lineToInterprete.ToLower()))
            {
                var fn = new Func<string, List<string>, IVariable, string>(((l, s, f) => l.ToLower()));
                Cache.Instance.CallCache.Add(lineToInterprete,fn);
                return !optimize ? fn.Invoke(lineToInterprete,scopes,file) : null;
            }

            //Return var value
            if (RegexCollection.Store.Variable.IsMatch(lineToInterprete) || RegexCollection.Store.SingleName.IsMatch(lineToInterprete))
            {
                var result = !optimize ? GetVarValue(lineToInterprete, scopes,file) : null;
                if (result != null)
                {
                    try
                    {
                        Cache.Instance.CallCache.Add(lineToInterprete, GetVarValue);
                    }
                    catch (Exception e)
                    {
                        Error(e);
                    }
                    return result;
                }
            }

            //Force through
            if (RegexCollection.Store.ForceThrough.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, ForceThrough);
                return !optimize ? ForceThrough(lineToInterprete, scopes, file) : null;
            }

            return lineToInterprete;
        }

        #region Implementation

        private string ArrayDecleration(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                Output.WriteLine(Evaluator.CreateArray(lineToInterprete, scopes, this, file as FileInterpreter));
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }
            return Empty;
        }

        private string VarDecleration(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                Output.WriteLine(Evaluator.CreateVariable(lineToInterprete, scopes, this, file as FileInterpreter));
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }
            return Empty;
        }

        private string VarAssignment(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                Output.WriteLine(Evaluator.AssignToVariable(lineToInterprete, scopes, true, this, file as FileInterpreter));
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }
            return Empty;
        }

        private string ArrayAssignment(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                Output.WriteLine(Evaluator.AssignToArrayAtPos(lineToInterprete, scopes, this, file as FileInterpreter));
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }
            return Empty;
        }

        private string OperatorAssignment(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var groups = RegexCollection.Store.OpAssignment.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToArray();

            var output = Output;
            var eOutput = ExplicitOutput;
            Output = new NoOutput();
            ExplicitOutput = new NoOutput();
            lineToInterprete =
                $"{groups[1]} = ${groups[1].TrimStart('$')} {groups[2]} {InterpretLine(groups[3], scopes, file as FileInterpreter)}";
            Output = output;
            ExplicitOutput = eOutput;

            return InterpretLine(lineToInterprete, scopes, file as FileInterpreter);
        }

        private string MethodCalls(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var result = Empty;
            try
            {
                result = Evaluator.CallMethod(lineToInterprete, scopes, this);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }

            if (result != Empty)
            {
                Output.WriteLine(result);
            }
            return result;
        }

        private string VarCall(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var groups = RegexCollection.Store.VarCall.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();
            var result = Empty;

            try
            {
                result = Evaluator.GetObjectVar(groups[1], groups[2], scopes, this);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }

            if (result != Empty)
            {
                Output.WriteLine(result);
            }
            return result;
        }

        private string CallOut(string lineToInterprete, List<string> scopes, IVariable file)
        {
            string result;
            try
            {
                result = Evaluator.EvaluateOut(lineToInterprete, scopes, this, file as FileInterpreter).TrimStart('\'').TrimEnd('\'');
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
                return Empty;
            }
            if (!MuteOut)
            {
                ExplicitOutput.WriteLine(result);
            }
            return $"'{result}'";
        }

        private string CallCustomFunction(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var groups = RegexCollection.Store.Function.Match(lineToInterprete).Groups.OfType<Group>().ToArray();
            return Evaluator.CallCustomFunction(groups,this,scopes,file as FileInterpreter);
        }

        private string Unload(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                Output.WriteLine(Evaluator.Unload(RegexCollection.Store.Unload.Match(lineToInterprete).Groups[1].Value,
                    scopes));
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }
            return Empty;
        }

        private string CallRaw(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var result = Evaluator.Raw(lineToInterprete, scopes, this, file as FileInterpreter);
            Output.WriteLine(result);
            return result;
        }

        private string GetInput(string lineToInterprete, List<string> scopes, IVariable file)
        {
            return $"'{ExplicitOutput.ReadLine()}'";
        }

        private string GetType(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var result = Empty;
            var typeGroup = RegexCollection.Store.Type.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();
            var toCheck = IsNullOrEmpty(typeGroup[1]) ? typeGroup[2] : typeGroup[1];

            if (lineToInterprete.StartsWith("d"))
            {
                var content = toCheck;
                if (!RegexCollection.Store.IsPureWord.IsMatch(toCheck))
                {
                    content = InterpretLine(toCheck, scopes, file as FileInterpreter);
                }
                result = Evaluator.DataTypeFromData(content, true).ToString();
            }
            else
            {
                try
                {
                    result = Evaluator
                        .GetVariable(toCheck, scopes)
                        .DataType.ToString();
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                    Error(e);
                }
            }

            Output.WriteLine(result);
            return result;
        }

        private string Exists(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var result = Evaluator.Exists(RegexCollection.Store.Exists.Match(lineToInterprete).Groups[1].Value, scopes).Exists
                .ToString().ToLower();
            Output.WriteLine(result);
            return result;
        }

        private string GetArrayValue(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var value = Empty;
            try
            {
                value = Evaluator.GetArrayValue($"${lineToInterprete.TrimStart('$')}", scopes, this, file as FileInterpreter);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
                return value;
            }
            Output.WriteLine(value.TrimStart('\'').TrimEnd('\''));
            return value;
        }

        private string InDecrease(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                var eOutput = ExplicitOutput;
                ExplicitOutput = new NoOutput();
                Evaluator.InDeCrease(lineToInterprete, scopes, this, file as FileInterpreter);
                ExplicitOutput = eOutput;
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }
            return Empty;
        }

        private static string ReturnWordValue(string lineToInterprete, List<string> scopes, IVariable file)
        {
            return RegexCollection.Store.IsPureWord.Match(lineToInterprete).Groups[1].Value;
        }

        private string Calculate(string lineToInterprete, List<string> scopes, IVariable file)
        {
            (bool Success, string Result) calculationResult;
            try
            {
                calculationResult = Evaluator.EvaluateCalculation(lineToInterprete.Replace("integ(", "int(").Replace("%", "#"),
                    scopes, this, file as FileInterpreter);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
                return Empty;
            }

            if (calculationResult.Result != Empty)
            {
                Output.WriteLine(calculationResult.Result);
            }

            if (calculationResult.Result != "NaN")
            {
                return calculationResult.Result;
            }
            return null;
        }

        private string Clear(string lineToInterprete, List<string> scopes, IVariable file)
        {
            ExplicitOutput.Clear();
            return Empty;
        }

        private string IncludeLibOrFile(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                return Evaluator.IncludeLib(lineToInterprete, scopes, this);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
                return Empty;
            }
        }

        private string GetVarValue(string lineToInterprete, List<string> scopes, IVariable file)
        {
            IVariable variable;
            try
            {
                variable = Evaluator.GetVariable(lineToInterprete.TrimStart('$'), scopes);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
                return Empty;
            }
            if (variable is Variable o1)
            {
                Output.WriteLine(o1.Value);
                return o1.Value;
            }
            if (variable is Array o)
            {
                var result = o.Values.Aggregate("{", (current, keyValuePair) => current + $"{keyValuePair.Value},")
                                 .TrimEnd(',') + "}";
                Output.WriteLine(result);
                return result;
            }
            if (variable is FileInterpreter)
            {
                string guid;

                //Put into cache
                if (Cache.Instance.FileCache.ContainsValue(variable))
                {
                    guid = Cache.Instance.FileCache.First(a => a.Value == variable).Key;
                }
                else
                {
                    guid = Guid.NewGuid().ToString();
                    Cache.Instance.FileCache.Add(guid, variable);
                }
                var reference = $"obj{guid}";
                Output.WriteLine(reference);
                return reference;
            }
            Output.WriteLine($"Invalid operation {lineToInterprete}");
            return null;
        }

        private string ForceThrough(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var output = Output;
            var eOutput = ExplicitOutput;
            Output = new NoOutput();
            ExplicitOutput = new NoOutput();
            var valueToInterpret =
                InterpretLine(RegexCollection.Store.ForceThrough.Match(lineToInterprete).Groups[1].Value, scopes, file as FileInterpreter)
                    .TrimStart('\'').TrimEnd('\'');
            Output = output;
            ExplicitOutput = eOutput;

            return InterpretLine(valueToInterpret, scopes, file as FileInterpreter);
        }

        private string VarCallAssign(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var groups = RegexCollection.Store.VarCallAssign.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToArray();

            try
            {
                Output.WriteLine(Evaluator.VarCallAssign(groups[1], groups[2], groups[3], scopes, this,file as FileInterpreter));
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
            }

            return Empty;
        }

        private string GetFields(string linetointerpret, List<string> scopes, IVariable file)
        {
            var result = Evaluator.GetFields(scopes);
            Output.WriteLine(result);
            return result;
        }

        #endregion

        /// <summary>
        /// Registers a custom function
        /// </summary>
        /// <param name="f">Custom function</param>
        /// <returns></returns>
        public bool RegisterFunction(Function f)
        {
            if (Cache.Instance.Functions.Contains(f)) return false;
            Cache.Instance.Functions.Add(f);
            return true;
        }

        private void Error(Exception exception)
        {
            if (_error)
            {
                throw exception;
            }
        }

        public void ShouldError(bool error)
        {
            _error = error;
        }
    }
}