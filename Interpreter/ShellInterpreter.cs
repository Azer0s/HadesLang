using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCalc;
using Variables;

namespace Interpreter
{
    public class ShellInterpreter
    {
        private static readonly Evaluator _evaluator = new Evaluator();
        private static string _varPattern = @"as (num|dec|word)+ (reachable|reachable_all|closed)+";

        public string InterpretLine(string lineToInterprete)
        {
            if (Regex.IsMatch(lineToInterprete, _varPattern))
            {
                return _evaluator.EvaluateVar(lineToInterprete);
            }

            if (lineToInterprete.Contains("=")) { }

            //Method call
            if (lineToInterprete.Contains(":"))
            {
                var call = lineToInterprete.Split(':');

                if (call[0] == "out")
                {
                    if (Regex.IsMatch(call[1], @"\\""([^]]*)\\"""))
                    {
                        return call[1];
                    }
                    if (Cache.Instance.Variables.ContainsKey(call[1]))
                    {
                        return Cache.Instance.Variables[call[1]].Value;
                    }
                    else
                    {
                        return "Variable not defined";
                    }
                }

                if (call[0] == "exit")
                {
                    try
                    {
                        Environment.Exit(int.Parse(call[1]));
                    }
                    catch (Exception e)
                    {
                        return e.Message;
                    }
                }
            }
            else
            {
                //Function call 
                try
                {
                    if (lineToInterprete.Contains("[") && lineToInterprete.Contains("]"))
                    {
                        return _evaluator.EvaluateBool(lineToInterprete);
                    }

                    if (lineToInterprete.Contains("+") || lineToInterprete.Contains("-") || lineToInterprete.Contains("*") || lineToInterprete.Contains("/"))
                    {
                        var e = new Expression(lineToInterprete);
                        return e.Evaluate().ToString();
                    }
                }
                catch (Exception e)
                {
                    return e.Message;
                }                
            }
            return null;
        }
    }
}
