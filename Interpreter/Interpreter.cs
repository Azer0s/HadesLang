using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using StringExtension;

namespace Interpreter
{
    class Interpreter
    {
        public readonly Evaluator Evaluator = new Evaluator();
        public bool Clear { get; set; } = false;

        public KeyValuePair<string, bool> InterpretLine(string lineToInterprete, string access, out string operation)
        {
            operation = "";
            if (Regex.IsMatch(lineToInterprete, Evaluator.VarPattern))
            {
                var createResult = Evaluator.CreateVariable(lineToInterprete, access);

                if (Evaluator.ForceOut)
                {
                    Evaluator.ForceOut = false;
                    return new KeyValuePair<string, bool>(createResult, true);
                }
                return new KeyValuePair<string, bool>(createResult, false);
            }

            if (lineToInterprete == "clear")
            {
                Clear = true;
                return new KeyValuePair<string, bool>(string.Empty, false);
            }

            if (lineToInterprete.Contains("="))
            {
                return new KeyValuePair<string, bool>(Evaluator.AssignValueToVariable(lineToInterprete, access), false);
            }

            //Method call
            if (lineToInterprete.Contains(":") || lineToInterprete.Contains("->"))
            {
                string[] call;
                if (lineToInterprete.CheckOrder(":", "->"))
                {
                    call = lineToInterprete.Split(new[] { ':' }, 2);
                }
                else
                {
                    call = new string[2];
                    call[1] = lineToInterprete;
                }

                return Evaluator.EvaluateCall(call, access);
            }
            else
            {
                //Function call 
                try
                {
                    if (Regex.IsMatch(lineToInterprete, @"\[([^]]*)\]"))
                    {
                        var boolRes = Evaluator.EvaluateBool(lineToInterprete, access);
                        operation = boolRes.OperationType.ToString().ToLower();
                        return new KeyValuePair<string, bool>(boolRes.Result.ToString().ToLower(), false);
                    }

                    if (lineToInterprete.ContainsFromList(Evaluator.OperatorList))
                    {
                        return new KeyValuePair<string, bool>(Evaluator.EvaluateCalculation(lineToInterprete), false);
                    }
                }
                catch (Exception e)
                {
                    return new KeyValuePair<string, bool>(e.Message, true);
                }
            }
            return new KeyValuePair<string, bool>(null, false);
        }
    }
}
