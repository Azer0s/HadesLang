using System.Collections.Generic;
using System.Linq;
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
        private readonly IScriptOutput _fileOutput;
        private readonly Evaluator _evaluator;


        public Interpreter(IScriptOutput output, IScriptOutput fileOutput)
        {
            Output = output;
            _fileOutput = fileOutput;
            _evaluator = new Evaluator(_fileOutput,this);
            Cache.Instance.Variables = new Dictionary<Meta, IVariable>();
            Cache.Instance.Functions = new List<Function>();
            Cache.Instance.LoadFiles = new List<string>();
        }

        public string InterpretLine(string lineToInterprete, string access)
        {
            //Variable decleration
            if (RegexCollection.Store.Variables.IsMatch(lineToInterprete))
            {
                Output.WriteLine(_evaluator.CreateVariable(lineToInterprete, access));
                return Empty;
            }

            //Clear console
            if (lineToInterprete.ToLower().Replace(" ","") == "clear")
            {
                Output.Clear();
                return Empty;
            }

            //Include library
            if (RegexCollection.Store.With.IsMatch(lineToInterprete))
            {
                var groups = RegexCollection.Store.With.Match(lineToInterprete).Groups.OfType<Group>().ToList();
                Output.WriteLine(_evaluator.IncludeLib(lineToInterprete,access));
                return Empty;
            }

            //Variable assignment
            if (RegexCollection.Store.Assignment.IsMatch(lineToInterprete))
            {
                Output.WriteLine(_evaluator.AssignToVariable(lineToInterprete,access));
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

                //TODO Method calls

                #region Console-Specific

                //Input
                if (RegexCollection.Store.Input.IsMatch(lineToInterprete))
                {
                    Output.WriteLine(_evaluator.Input(lineToInterprete,access,Output));
                    return Empty;
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