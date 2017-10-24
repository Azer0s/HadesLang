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
    public class Interpreter
    {
        public IScriptOutput Output;
        public IScriptOutput ExplicitOutput;
        private IScriptOutput _backup;
        public readonly Evaluator Evaluator;
        public bool MuteOut = false;


        public Interpreter(IScriptOutput output, IScriptOutput explicitOutput)
        {
            Output = output;
            ExplicitOutput = explicitOutput;
            _backup = explicitOutput;
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

            if (file != null && !scopes.Contains(file.FAccess))
            {
                scopes.Add(file.FAccess);
            }

            //Variable decleration
            if (RegexCollection.Store.CreateVariable.IsMatch(lineToInterprete))
            {
                try
                {
                    Output.WriteLine(Evaluator.CreateVariable(lineToInterprete, scopes, this, file));
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                }
                return Empty;
            }

            //Array decleration
            if (RegexCollection.Store.CreateArray.IsMatch(lineToInterprete))
            {
                try
                {
                    Output.WriteLine(Evaluator.CreateArray(lineToInterprete, scopes, this, file));
                }
                catch (Exception e)
                {
                        ExplicitOutput.WriteLine(e.Message);
                }
                return Empty;
            }

            //Array assignment
            if (RegexCollection.Store.ArrayAssignment.IsMatch(lineToInterprete))
            {
                try
                {
                    Output.WriteLine(Evaluator.AssignToArrayAtPos(lineToInterprete, scopes, this, file));
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                }
                return Empty;
            }

            //Variable assignment
            if (RegexCollection.Store.Assignment.IsMatch(lineToInterprete))
            {
                try
                {
                    Output.WriteLine(Evaluator.AssignToVariable(lineToInterprete, scopes, true, this, file));
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                }
                return Empty;
            }

            //Operator assignment
            if (RegexCollection.Store.OpAssignment.IsMatch(lineToInterprete))
            {
                var groups = RegexCollection.Store.OpAssignment.Match(lineToInterprete).Groups.OfType<Group>()
                    .Select(a => a.Value).ToArray();

                var output = Output;
                var eOutput = ExplicitOutput;
                Output = new NoOutput();
                ExplicitOutput = new NoOutput();
                lineToInterprete = $"{groups[1]} = ${groups[1].TrimStart('$')} {groups[2]} {InterpretLine(groups[3], scopes, file)}";
                Output = output;
                ExplicitOutput = eOutput;

                return InterpretLine(lineToInterprete, scopes, file);
            }

            //Method calls
            if (RegexCollection.Store.MethodCall.IsMatch(lineToInterprete))
            {
                var result = Empty;
                try
                {
                    result = Evaluator.CallMethod(lineToInterprete, scopes, this);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                }

                if (result != Empty)
                {
                    Output.WriteLine(result);
                }
                return result;
            }

            //Var call
            if (RegexCollection.Store.VarCall.IsMatch(lineToInterprete))
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
                }

                if (result != Empty)
                {
                    Output.WriteLine(result);
                }
                return result;
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
                        catch (Exception)
                        {
                            // Ignored
                        }
                    }
                }

                //Custom functions
                if (Cache.Instance.Functions.Any(a => a.Name == groups[1].Value))
                {
                    return Evaluator.CallCustomFunction(groups); ;
                }

                //Out
                if (RegexCollection.Store.Output.IsMatch(lineToInterprete))
                {
                    string result;
                    try
                    {
                        result = Evaluator.EvaluateOut(lineToInterprete, scopes, this, file).TrimStart('\'').TrimEnd('\'');
                    }
                    catch (Exception e)
                    {
                        ExplicitOutput.WriteLine(e.Message);
                        return Empty;
                    }
                    if (!MuteOut)
                    {
                        ExplicitOutput.WriteLine(result);
                    }
                    return $"'{result}'";
                }

                //Unload
                if (RegexCollection.Store.Unload.IsMatch(lineToInterprete))
                {
                    try
                    {
                        Output.WriteLine(Evaluator.Unload(RegexCollection.Store.Unload.Match(lineToInterprete).Groups[1].Value, scopes));
                    }
                    catch (Exception e)
                    {
                        ExplicitOutput.WriteLine(e.Message);
                    }
                    return Empty;
                }

                //Raw
                if (RegexCollection.Store.Raw.IsMatch(lineToInterprete))
                {
                    var result = Evaluator.Raw(lineToInterprete, scopes, this, file);
                    Output.WriteLine(result);
                    return result;
                }

                //Range
                if (RegexCollection.Store.Range.IsMatch(lineToInterprete))
                {
                    try
                    {
                        lineToInterprete = Evaluator.ReplaceWithVars(lineToInterprete, scopes, this, file);
                    }
                    catch (Exception e)
                    {
                        ExplicitOutput.WriteLine(e.Message);
                        return Empty;
                    }

                    var range = RegexCollection.Store.Range.Match(lineToInterprete).Groups.OfType<Group>()
                        .Select(a => a.Value).ToList();
                    var rangeArray = $"{{{Join(",", Enumerable.Range(int.Parse(range[1]), int.Parse(range[2])))}}}";

                    Output.WriteLine(rangeArray);

                    return rangeArray;
                }

                #region Console-Specific

                //Input
                if (RegexCollection.Store.Input.IsMatch(lineToInterprete))
                {
                    return $"'{ExplicitOutput.ReadLine()}'";
                }

                //Random number
                if (RegexCollection.Store.RandomNum.IsMatch(lineToInterprete))
                {
                    var result = new Random().Next(int.Parse(RegexCollection.Store.RandomNum.Match(lineToInterprete).Groups[1].Value)).ToString();
                    Output.WriteLine(result);
                    return result;
                }

                //Type/dtype
                if (RegexCollection.Store.Type.IsMatch(lineToInterprete))
                {
                    string result;
                    var typeGroup = RegexCollection.Store.Type.Match(lineToInterprete).Groups.OfType<Group>()
                        .Select(a => a.Value).ToList();
                    var toCheck = IsNullOrEmpty(typeGroup[1]) ? typeGroup[2] : typeGroup[1]; 

                    if (lineToInterprete.StartsWith("d"))
                    {
                        var content = toCheck;
                        if (!RegexCollection.Store.IsPureWord.IsMatch(toCheck))
                        {
                            content = InterpretLine(toCheck, scopes, file);
                        }
                        result = Evaluator.DataTypeFromData(content, true).ToString();
                    }
                    else
                    {
                        result = Evaluator
                            .GetVariable(toCheck, scopes)
                            .DataType.ToString();
                    }

                    Output.WriteLine(result);
                    return result;
                }

                //Exists
                if (RegexCollection.Store.Exists.IsMatch(lineToInterprete))
                {
                    var result = Evaluator.Exists(RegexCollection.Store.Exists.Match(lineToInterprete).Groups[1].Value, scopes).Exists.ToString().ToLower();
                    Output.WriteLine(result);
                    return result;
                }

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
                var value = Empty;
                try
                {
                    value = Evaluator.GetArrayValue($"${lineToInterprete.TrimStart('$')}", scopes, this, file);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                    return value;
                }
                Output.WriteLine(value.TrimStart('\'').TrimEnd('\''));
                return value;
            }

            //In/Decrease
            if (RegexCollection.Store.InDeCrease.IsMatch(lineToInterprete))
            {
                try
                {
                    var eOutput = ExplicitOutput;
                    ExplicitOutput = new NoOutput();
                    Evaluator.InDeCrease(lineToInterprete, scopes, this, file);
                    ExplicitOutput = eOutput;
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                }
                return Empty;
            }

            //Return string
            if (RegexCollection.Store.IsPureWord.IsMatch(lineToInterprete))
            {
                return RegexCollection.Store.IsPureWord.Match(lineToInterprete).Groups[1].Value;
            }

            //Calculation & string concat
            if ((lineToInterprete.ContainsFromList(Cache.Instance.CharList) ||
                lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys)) &&
                (!RegexCollection.Store.IsWord.IsMatch(lineToInterprete) ||
                lineToInterprete.Remainder(RegexCollection.Store.IsWord)
                .ContainsFromList(Cache.Instance.CharList.Concat(Cache.Instance.Replacement.Keys.ToList())) &&
                !lineToInterprete.StartsWith("#")))
            {
                (bool Success, string Result) calculationResult;
                try
                {
                    calculationResult = Evaluator.EvaluateCalculation(lineToInterprete.Replace("integ(", "int(").Replace("%", "#"), scopes, this, file);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
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
            }

            //Clear console
            if (lineToInterprete.ToLower().Replace(" ", "") == "clear")
            {
                Output.Clear();
                return Empty;
            }

            //Include library or file
            if (RegexCollection.Store.With.IsMatch(lineToInterprete))
            {
                try
                {
                    return Evaluator.IncludeLib(lineToInterprete, scopes,this);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                    return Empty;
                }
            }

            //Bool type
            if (RegexCollection.Store.IsBit.IsMatch(lineToInterprete.ToLower()))
            {
                return lineToInterprete.ToLower();
            }

            //Constants
            switch (lineToInterprete)
            {
                case "e":
                    Output.WriteLine(Math.E.ToString(CultureInfo.InvariantCulture));
                    return Math.E.ToString(CultureInfo.InvariantCulture);
                case "pi":
                    Output.WriteLine(Math.PI.ToString(CultureInfo.InvariantCulture));
                    return Math.PI.ToString(CultureInfo.InvariantCulture);
            }

            //Return var value
            if (RegexCollection.Store.Variable.IsMatch(lineToInterprete) || RegexCollection.Store.SingleName.IsMatch(lineToInterprete))
            {
                IVariable variable = null;
                try
                {
                    variable = Evaluator.GetVariable(lineToInterprete.TrimStart('$'), scopes);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
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
            }

            //Force through
            if (RegexCollection.Store.ForceThrough.IsMatch(lineToInterprete))
            {
                var output = Output;
                var eOutput = ExplicitOutput;
                Output = new NoOutput();
                ExplicitOutput = new NoOutput();
                var valueToInterpret = InterpretLine(RegexCollection.Store.ForceThrough.Match(lineToInterprete).Groups[1].Value,scopes, file).TrimStart('\'').TrimEnd('\'');
                Output = output;
                ExplicitOutput = eOutput;

                return InterpretLine(valueToInterpret,scopes, file);
            }

            return lineToInterprete;
        }

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
    }
}