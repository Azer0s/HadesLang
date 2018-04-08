using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text.RegularExpressions;
using Output;
using StringExtension;
using Variables;
using static System.String;
using Function = Variables.Function;

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
            Cache.Instance.LibraryLocation = File.Exists(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\deflib") ? File.ReadAllText(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\deflib") : Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!Directory.Exists(Cache.Instance.LibraryLocation))
            {
                Output.WriteLine("Default-library location does not exist! Aborting!");
                Output.ReadLine();
                Environment.Exit(-1);
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

        public string InterpretLine(string lineToInterprete, List<string> scopes, FileInterpreter file, bool writeSettings = false)
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
                        var tempLine = lineToInterprete.ToString();

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
            if (Cache.Instance.CallCache.ContainsKey(lineToInterprete))
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
                return VarDecleration(lineToInterprete, scopes, file);
            }

            //Array decleration
            if (RegexCollection.Store.CreateArray.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,ArrayDecleration);
                return ArrayDecleration(lineToInterprete, scopes, file);
            }

            //Array assignment
            if (RegexCollection.Store.ArrayAssignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,ArrayAssignment);
                return ArrayAssignment(lineToInterprete, scopes, file);
            }

            //Variable assignment
            if (RegexCollection.Store.Assignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,VarAssignment);
                return VarAssignment(lineToInterprete, scopes, file);
            }

            //Operator assignment
            if (RegexCollection.Store.OpAssignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,OperatorAssignment);
                return OperatorAssignment(lineToInterprete, scopes, file);
            }

            //Method calls
            if (RegexCollection.Store.MethodCall.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,MethodCalls);
                return MethodCalls(lineToInterprete, scopes,file);
            }

            //Var call assign
            if (RegexCollection.Store.VarCallAssign.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, VarCallAssign);
                return VarCallAssign(lineToInterprete, scopes, file);
            }

            //Var call
            if (RegexCollection.Store.VarCall.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,VarCall);
                return VarCall(lineToInterprete, scopes,file);
            }

            //Function
            if (RegexCollection.Store.Function.IsMatch(lineToInterprete) &&
                !lineToInterprete.Remainder(RegexCollection.Store.Outside)
                .ContainsFromList(Cache.Instance.CharList.Concat(Cache.Instance.Replacement.Keys)) &&
                (lineToInterprete.IsValidFunction() || lineToInterprete.NestedFunction(RegexCollection.Store.FunctionParam)))
            {
                //Exit
                if (RegexCollection.Store.Exit.IsMatch(lineToInterprete))
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
                            return file.CallFunction(lineToInterprete, this,scopes);
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
                    return CallCustomFunction(lineToInterprete,scopes,file);
                }

                //Out
                if (RegexCollection.Store.Output.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,CallOut);
                    return CallOut(lineToInterprete, scopes, file);
                }

                //Unload
                if (RegexCollection.Store.Unload.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,Unload);
                    return Unload(lineToInterprete, scopes,file);
                }

                //Raw
                if (RegexCollection.Store.Raw.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,CallRaw);
                    return CallRaw(lineToInterprete, scopes, file);
                }

                //GetFields
                if (RegexCollection.Store.Fields.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetFields);
                    return GetFields(lineToInterprete, scopes, file);
                }

                //Range
                if (RegexCollection.Store.Range.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetRange);
                    return GetRange(lineToInterprete, scopes, file);
                }

                #region Console-Specific

                //Input
                if (RegexCollection.Store.Input.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetInput);
                    return GetInput(lineToInterprete,scopes,file);
                }

                //Random number
                if (RegexCollection.Store.RandomNum.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetRandomNumber);
                    return GetRandomNumber(lineToInterprete,scopes,file);
                }

                //Type/dtype
                if (RegexCollection.Store.Type.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,GetType);
                    return GetType(lineToInterprete, scopes, file);
                }

                //Exists
                if (RegexCollection.Store.Exists.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete,Exists);
                    return Exists(lineToInterprete, scopes,file);
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
                return GetArrayValue(lineToInterprete, scopes, file);
            }

            //In/Decrease
            if (RegexCollection.Store.InDeCrease.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,InDecrease);
                return InDecrease(lineToInterprete, scopes, file);
            }

            //Return string
            if (RegexCollection.Store.IsPureWord.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,ReturnWordValue);
                return ReturnWordValue(lineToInterprete,scopes,file);
            }

            //Calculation & string concat
            if ((lineToInterprete.ContainsFromList(Cache.Instance.CharList) ||
                lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys)) &&
                (!RegexCollection.Store.IsWord.IsMatch(lineToInterprete) ||
                lineToInterprete.Remainder(RegexCollection.Store.IsWord)
                .ContainsFromList(Cache.Instance.CharList.Concat(Cache.Instance.Replacement.Keys.ToList())) &&
                !lineToInterprete.StartsWith("#")))
            {
                var result = Calculate(lineToInterprete, scopes, file);
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
                return Clear(lineToInterprete,scopes,file);
            }

            //Include library or file
            if (RegexCollection.Store.With.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete,IncludeLibOrFile);
                return IncludeLibOrFile(lineToInterprete, scopes,file);
            }

            //Bool type
            if (RegexCollection.Store.IsBit.IsMatch(lineToInterprete.ToLower()))
            {
                var fn = new Func<string, List<string>, IVariable, string>(((l, s, f) => l.ToLower()));
                Cache.Instance.CallCache.Add(lineToInterprete,fn);
                return fn.Invoke(lineToInterprete,scopes,file);
            }

            //Return var value
            if (RegexCollection.Store.Variable.IsMatch(lineToInterprete) || RegexCollection.Store.SingleName.IsMatch(lineToInterprete))
            {
                var result = GetVarValue(lineToInterprete, scopes,file);
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
                return ForceThrough(lineToInterprete, scopes, file);
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
            return Evaluator.CallCustomFunction(groups);
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

        private string GetRange(string lineToInterprete, List<string> scopes, IVariable file)
        {
            try
            {
                lineToInterprete = Evaluator.ReplaceWithVars(lineToInterprete, scopes, this, file as FileInterpreter);
            }
            catch (Exception e)
            {
                ExplicitOutput.WriteLine(e.Message);
                Error(e);
                return Empty;
            }

            var range = RegexCollection.Store.Range.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();
            var rangeArray = $"{{{Join(",", Enumerable.Range(int.Parse(range[1]), int.Parse(range[2])))}}}";

            Output.WriteLine(rangeArray);

            return rangeArray;
        }

        private string GetInput(string lineToInterprete, List<string> scopes, IVariable file)
        {
            return $"'{ExplicitOutput.ReadLine()}'";
        }

        private string GetRandomNumber(string lineToInterprete, List<string> scopes, IVariable file)
        {
            var result = new Random().Next(int.Parse(RegexCollection.Store.RandomNum.Match(lineToInterprete).Groups[1].Value))
                .ToString();
            Output.WriteLine(result);
            return result;
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
            IVariable variable = null;
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
            if (variable is Variable)
            {
                var o = variable as Variable;
                Output.WriteLine(o.Value);
                return o.Value;
            }
            if (variable is Variables.Array)
            {
                var o = variable as Variables.Array;
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

        #region Optimizer

        /// <summary>
        /// Takes a line, evaluates it and puts in into the call cache
        /// </summary>
        /// <param name="lineToInterprete">Line to optimize</param>
        public void OptimizeLine(string lineToInterprete)
        {
            if (IsNullOrEmpty(lineToInterprete) || lineToInterprete == "end")
            {
                return;
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
                return;
            }

            //Replacement for alias
            lineToInterprete = Evaluator.AliasManager.AliasReplace(lineToInterprete);

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
                        var tempLine = lineToInterprete.ToString();

                        foreach (Match variable in RegexCollection.Store.IsWord.Matches(tempLine))
                        {
                            var guid = Guid.NewGuid().ToString().ToLower();
                            tempLine = tempLine.Replace(variable.Value, guid);
                            stringDict.Add(guid, variable.Value);
                        }


                        var matches = tempLine.Split(new[] { "|>" }, StringSplitOptions.None).Select(a => a.Trim()).ToList();

                        if (matches[0].Closes('[', ']'))
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

            //Call cached call
            if (Cache.Instance.CallCache.ContainsKey(lineToInterprete))
            {
                return;
            }

            //Variable decleration
            if (RegexCollection.Store.CreateVariable.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, VarDecleration);
                return;
            }

            //Array decleration
            if (RegexCollection.Store.CreateArray.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, ArrayDecleration);
                return;
            }

            //Array assignment
            if (RegexCollection.Store.ArrayAssignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, ArrayAssignment);
                return;
            }

            //Variable assignment
            if (RegexCollection.Store.Assignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, VarAssignment);
                return;
            }

            //Operator assignment
            if (RegexCollection.Store.OpAssignment.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, OperatorAssignment);
                return;
            }

            //Method calls
            if (RegexCollection.Store.MethodCall.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, MethodCalls);
                return;
            }

            //Var call assign
            if (RegexCollection.Store.VarCallAssign.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, VarCallAssign);
                return;
            }

            //Var call
            if (RegexCollection.Store.VarCall.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, VarCall);
                return;
            }

            //Function
            if (RegexCollection.Store.Function.IsMatch(lineToInterprete) &&
                !lineToInterprete.Remainder(RegexCollection.Store.Outside)
                .ContainsFromList(Cache.Instance.CharList.Concat(Cache.Instance.Replacement.Keys)) &&
                (lineToInterprete.IsValidFunction() || lineToInterprete.NestedFunction(RegexCollection.Store.FunctionParam)))
            {

                var groups = RegexCollection.Store.Function.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

                //Custom functions
                if (Cache.Instance.Functions.Any(a => a.Name == groups[1].Value))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, CallCustomFunction);
                    return;
                }

                //Out
                if (RegexCollection.Store.Output.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, CallOut);
                    return;
                }

                //Unload
                if (RegexCollection.Store.Unload.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, Unload);
                    return;
                }

                //Raw
                if (RegexCollection.Store.Raw.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, CallRaw);
                    return;
                }

                //GetFields
                if (RegexCollection.Store.Fields.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, GetFields);
                    return;
                }

                //Range
                if (RegexCollection.Store.Range.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, GetRange);
                    return;
                }

                #region Console-Specific

                //Input
                if (RegexCollection.Store.Input.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, GetInput);
                    return;
                }

                //Random number
                if (RegexCollection.Store.RandomNum.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, GetRandomNumber);
                    return;
                }

                //Type/dtype
                if (RegexCollection.Store.Type.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, GetType);
                    return;
                }

                //Exists
                if (RegexCollection.Store.Exists.IsMatch(lineToInterprete))
                {
                    Cache.Instance.CallCache.Add(lineToInterprete, Exists);
                    return;
                }

                #endregion

                return;
            }

            //Return array value
            if (RegexCollection.Store.ArrayVariable.IsMatch($"${lineToInterprete.TrimStart('$')}") && !RegexCollection.Store.IsWord.IsMatch(lineToInterprete.Remainder(RegexCollection.Store.Variable)))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, GetArrayValue);
                return;
            }

            //In/Decrease
            if (RegexCollection.Store.InDeCrease.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, InDecrease);
                return;
            }

            //Return string
            if (RegexCollection.Store.IsPureWord.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, ReturnWordValue);
                return;
            }

            //Clear console
            if (lineToInterprete.ToLower().Replace(" ", "") == "clear")
            {
                Cache.Instance.CallCache.Add(lineToInterprete, Clear);
                return;
            }

            //Include library or file
            if (RegexCollection.Store.With.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, IncludeLibOrFile);
                return;
            }

            //Bool type
            if (RegexCollection.Store.IsBit.IsMatch(lineToInterprete.ToLower()))
            {
                var fn = new Func<string, List<string>, IVariable, string>(((l, s, f) => l.ToLower()));
                Cache.Instance.CallCache.Add(lineToInterprete, fn);
                return;
            }

            //Return var value
            if (RegexCollection.Store.Variable.IsMatch(lineToInterprete) || RegexCollection.Store.SingleName.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, GetVarValue);
                return;
            }

            //Force through
            if (RegexCollection.Store.ForceThrough.IsMatch(lineToInterprete))
            {
                Cache.Instance.CallCache.Add(lineToInterprete, ForceThrough);
                return;
            }
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