using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NCalc;
using Variables;

namespace Interpreter
{
    public class Evaluator
    {
        public string EvaluateBool(string toEvaluate)
        {
            var reg = Regex.Match(toEvaluate.Replace(" ", ""), @"\[([^]]*)\]").Groups[1].Value;
            var func =
                toEvaluate.Replace(
                    Regex.Match(toEvaluate.Replace(" ", ""), @"\[([^]]*)\]").Groups[0].Value, "");

            if (!string.IsNullOrWhiteSpace(reg))
            {
                if (reg == "true" || reg == "false")
                {
                    return reg;
                }

                if (reg.Contains("is") || reg.Contains("or") || reg.Contains("and") || reg.Contains("not") || reg.Contains("smaller") || reg.Contains("bigger"))
                {
                    reg = reg.Replace("is", "==");
                    reg = reg.Replace("or", "||");
                    reg = reg.Replace("and", "&&");
                    reg = reg.Replace("not", "!=");
                    reg = reg.Replace("smaller", "<");
                    reg = reg.Replace("bigger", ">");
                    reg = reg.Replace("smallerIs", "<=");
                    reg = reg.Replace("biggerIs", ">=");

                    var e = new Expression(reg);
                    return e.Evaluate().ToString().ToLower();
                }
            }
            return null;
        }

        public string EvaluateVar(string toEvaluate)
        {
            var data = toEvaluate.Split(new char[] { ' ', '=' }).ToList();
            data.RemoveAll(s => s.Equals(""));

            try
            {
                if (data.Count > 4)
                {
                    if (data[4].Contains("+") || data[4].Contains("-") || data[4].Contains("*") ||
                        data[4].Contains("/"))
                    {
                        data[4] = new Expression(data[4]).Evaluate().ToString();
                    }
                    if (data[4].Contains("[") && data[4].Contains("]"))
                    {
                        data[4] = EvaluateBool(data[4]);
                    }
                    Cache.Instance.Variables.Add(data[0], new Types(AccessTypeParse.ParseTypes(data[3]), data[4]));
                    return $"{data[0]} is {data[4]}";
                }
                else
                {
                    Cache.Instance.Variables.Add(data[0], new Types(AccessTypeParse.ParseTypes(data[3]), ""));
                    return $"{data[0]} is undefined";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
    }
}
