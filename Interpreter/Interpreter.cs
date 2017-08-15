using System;
using System.Collections.Generic;
using System.Linq;
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
            Cache.Instance.Variables = new Dictionary<Meta, IVariable>();
            Cache.Instance.Functions = new List<Function>();
            Cache.Instance.LoadFiles = new List<string>();
        }

        public string InterpretLine(string lineToInterprete, string access)
        {
            //Variable decleration
            if (RegexCollection.Store.CreateVariable.IsMatch(lineToInterprete))
            {
                Output.WriteLine(_evaluator.CreateVariable(lineToInterprete, access,this));
                return Empty;
            }

            //Variable assignment
            if (RegexCollection.Store.Assignment.IsMatch(lineToInterprete))
            {
                Output.WriteLine(_evaluator.AssignToVariable(lineToInterprete,access,true,this));
                return Empty;
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

            //Method calls
            if (RegexCollection.Store.MethodCall.IsMatch(lineToInterprete))
            {
                var result = _evaluator.CallMethod(lineToInterprete, access);
                Output.WriteLine(result);
                return result;
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

            //Calculation
            if ((lineToInterprete.ContainsFromList(Cache.Instance.CharList) || lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys)) && !RegexCollection.Store.IsWord.IsMatch(lineToInterprete))
            {
                var calculationResult = _evaluator.EvaluateCalculation(lineToInterprete, access);
                Output.WriteLine(calculationResult.Result);

                if (calculationResult.Result != "NaN")
                {
                    return calculationResult.Result;
                }
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

            //Return var value
            if (RegexCollection.Store.Variable.IsMatch(lineToInterprete) || RegexCollection.Store.SingleName.IsMatch(lineToInterprete))
            {
                var variable = _evaluator.GetVariable(lineToInterprete.TrimStart('$'), access);
                if (!(variable is Object))
                {
                    var o = variable as Variable;
                    if (o != null) return o.Value;
                }
                else
                {
                    throw new Exception("Variable is an object!");
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