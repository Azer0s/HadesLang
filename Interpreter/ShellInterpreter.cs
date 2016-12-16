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
                return _evaluator.CreateVariable(lineToInterprete,"console");
            }

            if (lineToInterprete == "clear")
            {
                Clear = true;
                return string.Empty;
            }

            if (lineToInterprete.Contains("="))
            {
                return _evaluator.AssignValueToVariable(lineToInterprete,"console");
            }

            //Method call
            if (lineToInterprete.Contains(":") || lineToInterprete.Contains("->"))
            {
                string[] call;
                if (lineToInterprete.CheckOrder(":","->"))
                {
                    call = lineToInterprete.Split(new[] {':'}, 2);
                }
                else
                {
                    call = new string[2];
                    call[1] = lineToInterprete;
                }

                return _evaluator.EvaluateCall(call,"console").Key;
            }
            else
            {
                //Function call 
                try
                {
                    if (Regex.IsMatch(lineToInterprete, @"\[([^]]*)\]"))
                    {
                        return _evaluator.EvaluateBool(lineToInterprete,"console").Result.ToString().ToLower();
                    }

                    if (lineToInterprete.ContainsFromList(Evaluator.OperatorList))
                    {
                        lineToInterprete = _evaluator.ReplaceWithVars(lineToInterprete,"console");
                        return _evaluator.EvaluateCalculation(lineToInterprete);
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
