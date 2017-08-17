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
using Object = Variables.Object;

namespace Interpreter
{
    public class Interpreter
    {
        public IScriptOutput Output;
        public IScriptOutput ExplicitOutput;
        private readonly IScriptOutput _fileOutput;
        private readonly Evaluator _evaluator;


        public Interpreter(IScriptOutput output, IScriptOutput fileOutput,IScriptOutput explicitOutput)
        {
            Output = output;
            ExplicitOutput = explicitOutput;
            _fileOutput = fileOutput;
            _evaluator = new Evaluator(_fileOutput);

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

        public string InterpretLine(string lineToInterprete, string access)
        {
            if (IsNullOrEmpty(lineToInterprete))
            {
                return Empty;
            }

            //Variable decleration
            if (RegexCollection.Store.CreateVariable.IsMatch(lineToInterprete))
            {
                Output.WriteLine(_evaluator.CreateVariable(lineToInterprete, access,this));
                return Empty;
            }

            //Array decleration
            if (RegexCollection.Store.CreateArray.IsMatch(lineToInterprete))
            {
                try
                {
                    Output.WriteLine(_evaluator.CreateArray(lineToInterprete, access, this));
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
                    Output.WriteLine(_evaluator.AssignToArrayAtPos(lineToInterprete, access, this));
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
                    Output.WriteLine(_evaluator.AssignToVariable(lineToInterprete, access, true, this));
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
                lineToInterprete = $"{groups[1]} = ${groups[1].TrimStart('$')} {groups[2]} {InterpretLine(groups[3],access)}";
                Output = output;
                ExplicitOutput = eOutput;

                return InterpretLine(lineToInterprete, access);
            }

            //Method calls
            if (RegexCollection.Store.MethodCall.IsMatch(lineToInterprete))
            {
                var result = _evaluator.CallMethod(lineToInterprete, access);
                Output.WriteLine(result);
                return result;
            }    

            //Function
            if (RegexCollection.Store.Function.IsMatch(lineToInterprete))
            {
                //Exit
                if (RegexCollection.Store.Exit.IsMatch(lineToInterprete))
                {
                    _evaluator.Exit(lineToInterprete);
                }

                var groups = RegexCollection.Store.Function.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

                if (Cache.Instance.Functions.Any(a => a.Name == groups[1].Value))
                {
                    _evaluator.CallCustomFunction(groups);
                    return Empty;
                }

                //Out
                if (RegexCollection.Store.Output.IsMatch(lineToInterprete))
                {
                    string result;
                    try
                    {
                        result = _evaluator.EvaluateOut(lineToInterprete, access, this);
                    }
                    catch (Exception e)
                    {
                        ExplicitOutput.WriteLine(e.Message);
                        return Empty;
                    }
                    ExplicitOutput.WriteLine(result.TrimStart('\'').TrimEnd('\''));
                    return $"'{result.TrimStart('\'').TrimEnd('\'')}'";
                }

                //Unload
                if (RegexCollection.Store.Unload.IsMatch(lineToInterprete))
                {
                    try
                    {
                        Output.WriteLine(_evaluator.Unload(RegexCollection.Store.Unload.Match(lineToInterprete).Groups[1].Value, access));
                    }
                    catch (Exception e)
                    {
                        ExplicitOutput.WriteLine(e.Message);
                    }
                    return Empty;
                }

                #region Console-Specific

                //Input
                if (RegexCollection.Store.Input.IsMatch(lineToInterprete))
                {
                    var result = _evaluator.Input(lineToInterprete, access, Output,this);
                    Output.WriteLine(result.Message);
                    return result.Value;
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
                    if (lineToInterprete.StartsWith("d"))
                    {
                        if (_evaluator.DataTypeFromData(lineToInterprete,true) == DataTypes.WORD)
                        {
                            return DataTypes.WORD.ToString();
                        }
                        result = _evaluator.DataTypeFromData(InterpretLine(RegexCollection.Store.Type.Match(lineToInterprete).Groups[1].Value,access), true).ToString();
                    }
                    else
                    {
                        result = _evaluator
                            .GetVariable(RegexCollection.Store.Type.Match(lineToInterprete).Groups[1].Value, access)
                            .DataType.ToString();
                    }

                    Output.WriteLine(result);
                    return result;
                }

                //Exists
                if (RegexCollection.Store.Exists.IsMatch(lineToInterprete))
                {
                    var result = _evaluator.Exists(RegexCollection.Store.Exists.Match(lineToInterprete).Groups[1].Value,access).Exists.ToString().ToLower();
                    Output.WriteLine(result);
                    return result;
                }

                //ScriptOutput
                if (RegexCollection.Store.ScriptOutput.IsMatch(lineToInterprete))
                {
                    switch (RegexCollection.Store.ScriptOutput.Match(lineToInterprete).Groups[1].Value)
                    {
                        case "0":
                            _evaluator.ScriptOutput = new NoOutput();
                            Output.WriteLine("Script output disabled!");
                            return Empty;
                        case "1":
                            _evaluator.ScriptOutput = _fileOutput;
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
                    Output.WriteLine(_evaluator.DumpVars(dataTypeAsString == "all" ? DataTypes.NONE : TypeParser.ParseDataType(dataTypeAsString)));
                    return Empty;
                }

                #endregion

                return Empty;
            }

            //Calculation
            if ((lineToInterprete.ContainsFromList(Cache.Instance.CharList) || lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys)) && !RegexCollection.Store.IsWord.IsMatch(lineToInterprete) && !lineToInterprete.StartsWith("#"))
            {
                var calculationResult = _evaluator.EvaluateCalculation(lineToInterprete.Replace("integ(","int(").Replace("%","#"), access, this);
                Output.WriteLine(calculationResult.Result);

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

            //Include library
            if (RegexCollection.Store.With.IsMatch(lineToInterprete))
            {
                var groups = RegexCollection.Store.With.Match(lineToInterprete).Groups.OfType<Group>().ToList();
                Output.WriteLine(_evaluator.IncludeLib(lineToInterprete, access));
                return Empty;
            }

            //String concat
            if (lineToInterprete.Contains('+') && RegexCollection.Store.IsWord.IsMatch(lineToInterprete))
            {
                //TODO Make string concat, replace with vars
            }

            //Bool type
            if (RegexCollection.Store.IsBit.IsMatch(lineToInterprete.ToLower()))
            {
                return lineToInterprete.ToLower();
            }

            //Return string
            if (RegexCollection.Store.IsWord.IsMatch(lineToInterprete))
            {
                return RegexCollection.Store.IsWord.Match(lineToInterprete).Groups[1].Value;
            }

            //Return array value
            if (RegexCollection.Store.ArrayVariable.IsMatch($"${lineToInterprete.TrimStart('$')}"))
            {
                var value = Empty;
                try
                {
                    value = _evaluator.GetArrayValue($"${lineToInterprete.TrimStart('$')}", access,this);
                }
                catch (Exception e)
                {
                    ExplicitOutput.WriteLine(e.Message);
                    return value;
                }
                Output.WriteLine(value.TrimStart('\'').TrimEnd('\''));
                return value;
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
                    variable = _evaluator.GetVariable(lineToInterprete.TrimStart('$'), access);
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
                if(variable is Variables.Array)
                {
                    var o = variable as Variables.Array;
                    var result = o.Values.Aggregate("{", (current, keyValuePair) => current + $"{keyValuePair.Value},")
                                     .TrimEnd(',') + "}";
                    Output.WriteLine(result);
                    return result;
                }
                if (variable is Object)
                {
                    Output.WriteLine($"Variable {lineToInterprete} is an object!");
                    return Empty;
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
                var valueToInterpret = InterpretLine(RegexCollection.Store.ForceThrough.Match(lineToInterprete).Groups[1].Value,access).TrimStart('\'').TrimEnd('\'');
                Output = output;
                ExplicitOutput = eOutput;

                return InterpretLine(valueToInterpret, access);
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