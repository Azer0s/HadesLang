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
using StringExtension;
using Variables;

namespace Interpreter
{
    public class ShellInterpreter
    {
        private static readonly Evaluator _evaluator = new Evaluator();
        public bool Clear { get; set; } = false;

        public string InterpretLine(string lineToInterprete)
        {
            if (Regex.IsMatch(lineToInterprete, Evaluator.VarPattern))
            {
                return _evaluator.CreateVariable(lineToInterprete);
            }

            if (lineToInterprete == "clear")
            {
                Clear = true;
                return string.Empty;
            }

            if (lineToInterprete.Contains("="))
            {
                return _evaluator.AssignValueToVariable(lineToInterprete);
            }

            //Method call
            if (lineToInterprete.Contains(":"))
            {
                var call = lineToInterprete.Split(new[] {':'},2);

                return _evaluator.EvaluateCall(call);
            }
            else
            {
                //Function call 
                try
                {
                    if (Regex.IsMatch(lineToInterprete, @"\[([^]]*)\]"))
                    {
                        return _evaluator.EvaluateBool(lineToInterprete).Result.ToString().ToLower();
                    }

                    if (lineToInterprete.ContainsFromList(Evaluator.OpperatorList))
                    {
                        lineToInterprete = _evaluator.ReplaceWithVars(lineToInterprete);
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
