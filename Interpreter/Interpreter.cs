using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Hades.Output;
using Hades.StringExtension;
using Hades.Variables;
using static System.String;
using Function = Hades.Variables.Function;

namespace Hades.Interpreter
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
                        break;
                }
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

            //Dumpvars
            if (RegexCollection.Store.DumpVars.IsMatch(lineToInterprete))
            {
                var dataTypeAsString = RegexCollection.Store.DumpVars.Match(lineToInterprete).Groups[1].Value;
                Output.WriteLine(_evaluator.DumpVars(dataTypeAsString == "all" ? DataTypes.NONE : TypeParser.ParseDataType(dataTypeAsString)));
                return Empty;
            }


            //Calculation
            if (lineToInterprete.ContainsFromList(Cache.Instance.CharList) || lineToInterprete.ContainsFromList(Cache.Instance.Replacement.Keys))
            {
                var calculationResult = _evaluator.EvaluateCalculation(lineToInterprete, access);
                Output.WriteLine(calculationResult.Result);
                return calculationResult.Result;
            }

            //Method call
            //if (lineToInterprete.Contains(":") || lineToInterprete.Contains("->") || lineToInterprete.ContainsFromList(Cache.Instance.Functions.Select(a => a.Name).ToList()))
            //{
            //    string[] call;
            //    if (lineToInterprete.CheckOrder(":", "->"))
            //    {
            //        call = lineToInterprete.Split(new[] { ':' }, 2);
            //    }
            //    else
            //    {
            //        call = new string[2];
            //        call[1] = lineToInterprete;
            //    }

            //    return Evaluator.EvaluateCall(call, access);
            //}
            //else
            //{
            //    //Function call 
            //    try
            //    {
            //        if (Regex.IsMatch(lineToInterprete, @"\[([^]]*)\]"))
            //        {
            //            var boolRes = Evaluator.EvaluateBool(lineToInterprete, access);
            //            operation = boolRes.OperationType.ToString().ToLower();
            //            return new KeyValuePair<string, bool>(boolRes.Result.ToString().ToLower(), false);
            //        }

            //        if (lineToInterprete.ContainsFromList(Evaluator.OperatorList))
            //        {
            //            return new KeyValuePair<string, bool>(Evaluator.EvaluateCalculation(lineToInterprete), false);
            //        }
            //    }
            //    catch (Exception e)
            //    {
            //        return new KeyValuePair<string, bool>(e.Message, true);
            //    }
            //}

            return Empty;
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