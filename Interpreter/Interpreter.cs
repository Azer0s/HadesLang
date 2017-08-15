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
                Output.WriteLine(_evaluator.CreateArray(lineToInterprete,access,this));
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
                    Output.WriteLine(e.Message);
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
                    Output.WriteLine(e.Message);
                }
                return Empty;
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
                        Output.WriteLine(e.Message);
                        return Empty;
                    }
                    ExplicitOutput.WriteLine(result.TrimStart('\'').TrimEnd('\''));
                    return result;
                }

                //Unload
                if (RegexCollection.Store.Unload.IsMatch(lineToInterprete))
                {
                    Output.WriteLine(_evaluator.Unload(RegexCollection.Store.Unload.Match(lineToInterprete).Groups[1].Value, access));
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
                    return new Random().Next(int.Parse(RegexCollection.Store.RandomNum.Match(lineToInterprete).Groups[1].Value)).ToString();
                }

                //Type/dtype
                if (RegexCollection.Store.Type.IsMatch(lineToInterprete))
                {
                    string result;
                    if (lineToInterprete.StartsWith("d"))
                    {
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
            if ((lineToInterprete.ContainsFromList(Cache.Instance.CharList) || lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys)) && !RegexCollection.Store.IsWord.IsMatch(lineToInterprete))
            {
                var calculationResult = _evaluator.EvaluateCalculation(lineToInterprete.Replace("integ","int").Replace("%","#"), access, this);
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
                var value = _evaluator.GetArrayValue($"${lineToInterprete.TrimStart('$')}", access);
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
                    Output.WriteLine(e.Message);
                }
                if (variable != null && !(variable is Object))
                {
                    var o = variable as Variable;
                    if (o != null) return o.Value;
                }
                else
                {
                    Output.WriteLine("Variable is an object!");
                }
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