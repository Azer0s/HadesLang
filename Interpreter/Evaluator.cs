using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Exceptions;
using NCalc;
using StringExtension;
using Variables;
using InvalidOperationException = Exceptions.InvalidOperationException;

namespace Interpreter
{
    public class Evaluator
    {
        public static string VarPattern = @"as (num|dec|word|binary)+ (reachable|reachable_all|closed)+";
        public static List<string> OpperatorList = new List<string> {"+", "-", "*", "/", "Sqrt", "Sin", "Cos", "Tan"};

        public EvaluatedOperation EvaluateBool(string toEvaluate)
        {
            toEvaluate = ReplaceWithVars(toEvaluate);
            var reg = Regex.Match(toEvaluate.Replace(" ", ""), @"\[([^]]*)\]").Groups[1].Value;
            var func =
                EvaluateOperation(
                    toEvaluate.Replace(Regex.Match(toEvaluate.Replace(" ", ""), @"\[([^]]*)\]").Groups[0].Value, ""),
                    toEvaluate);

            if (!string.IsNullOrWhiteSpace(reg))
            {
                if (reg == "true" || reg == "false")
                {
                    return new EvaluatedOperation(func, bool.Parse(reg));
                }

                if (reg.Contains("is") || reg.Contains("or") || reg.Contains("and") || reg.Contains("not") ||
                    reg.Contains("smaller") || reg.Contains("bigger"))
                {
                    reg = reg.Replace("smallerIs", "<=");
                    reg = reg.Replace("biggerIs", ">=");
                    reg = reg.Replace("is", "==");
                    reg = reg.Replace("or", "||");
                    reg = reg.Replace("and", "&&");
                    reg = reg.Replace("not", "!=");
                    reg = reg.Replace("smaller", "<");
                    reg = reg.Replace("bigger", ">");

                    var e = new Expression(reg);
                    return new EvaluatedOperation(func, bool.Parse(e.Evaluate().ToString().ToLower()));
                }
            }
            return null;
        }

        public string CreateVariable(string toEvaluate)
        {
            try
            {
                var data = toEvaluate.Split(' ').ToList();
                data.RemoveAll(s => s.Equals("") || s.Equals("="));

                if (data.Count > 4)
                {
                    for (int i = 5; i < data.Count; i++)
                    {
                        data[4] += $" {data[i]}";
                    }

                    Cache.Instance.Variables.Add(data[0],
                        new Types(TypeParser.ParseAccessType(data[3]), TypeParser.ParseDataType(data[2]), ""));
                    return AssignValueToVariable(data[0] + "=" + data[4]);
                }
                else
                {
                    Cache.Instance.Variables.Add(data[0],
                        new Types(TypeParser.ParseAccessType(data[3]), TypeParser.ParseDataType(data[2]), ""));
                    return $"{data[0]} is undefined";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string AssignValueToVariable(string toEvaluate)
        {
            try
            {
                var data = toEvaluate.Split('=');
                var index = data[0].Replace(" ", "");
                var dt = Cache.Instance.Variables[index].DataType;
                data[1] = ReplaceWithVars(data[1]);
                var isOut = false;

                if (data[1].Contains(":"))
                {
                    var operation = data[1].Split(':');
                    operation[0] = operation[0].Replace(" ", "");
                    data[1] = "'" + EvaluateCall(operation) + "'";
                    isOut = true;
                }

                if (data[1].ContainsFromList(OpperatorList))
                {
                    data[1] = EvaluateCalculation(data[1]);
                }

                if (Regex.IsMatch(data[1], @"\[([^]]*)\]"))
                {
                    data[1] = EvaluateBool(data[1]).Result.ToString().ToLower();
                }

                if (dt == DataTypeFromData(data[1]) || isOut)
                {
                    if (dt == DataTypes.WORD)
                    {
                        data[1] = Regex.Match(data[1], @"\'([^]]*)\'").Groups[1].Value;
                    }

                    if (dt == DataTypes.DEC)
                    {
                        data[1] = data[1].Replace(",", ".");
                    }
                    Cache.Instance.Variables[index].Value = data[1];
                }
                else
                {
                    throw new InvalidDataAssignException(
                        "The data type of the variable does not match the assignment type!");
                }

                return $"{index} is {data[1]}";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string EvaluateCalculation(string toEvaluate)
        {
            try
            {
                var e = new Expression(toEvaluate);
                return e.Evaluate().ToString().Replace(",",".");
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string EvaluateOut(string toEvaluate, bool ignoreQuote)
        {
            if (ignoreQuote)
            {
                toEvaluate = $"'{toEvaluate}'";
            }
            try
            {
                if (Regex.IsMatch(toEvaluate, @"\[([^]]*)\]"))
                {
                    return EvaluateBool(toEvaluate).Result.ToString().ToLower();
                }
                if (toEvaluate.ContainsFromList(OpperatorList))
                {
                    return EvaluateCalculation(toEvaluate);
                }
                if (Regex.IsMatch(toEvaluate, @"\'([^]]*)\'"))
                {
                    return Regex.Match(toEvaluate, @"\'([^]]*)\'").Groups[1].Value;
                }
                if (Cache.Instance.Variables.ContainsKey(toEvaluate))
                {
                    return Cache.Instance.Variables[toEvaluate].Value;
                }
                else
                {
                    throw new VariableNotDefinedException("Variable not defined!");
                }
            }
            catch (VariableNotDefinedException ve)
            {
                return ve.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string EvaluateCall(string[] toEvaluate)
        {
            try
            {
                toEvaluate[1] = ReplaceWithVars(toEvaluate[1]);
                bool isRec = false;

                if (Regex.IsMatch(toEvaluate[1], @"(\[)([^]]*)(\])"))
                {
                    int index = toEvaluate[1].IndexOf("[");
                    toEvaluate[1] = (index < 0)
                    ? toEvaluate[1]
                    : toEvaluate[1].Remove(index, "[".Length);
                    toEvaluate[1] = toEvaluate[1].Substring(0, toEvaluate[1].LastIndexOf("]"));

                    var result = EvaluateCall(toEvaluate[1].Split(new[] { ':' }, 2));
                    toEvaluate[1] = result;
                    isRec = true;
                }

                if (toEvaluate[1].Contains("->"))
                {
                    var call = toEvaluate[1].Split(new[] { "->" }, 2, StringSplitOptions.None);

                    if (Cache.Instance.Variables.ContainsKey(call[0]))
                    {
                        if (Cache.Instance.Variables[call[0]].DataType == DataTypes.OBJECT)
                        {
                            //Todo call method
                        }
                        else
                        {
                           throw new InvalidDataTypeException("Variable is not an object!"); 
                        }
                    }
                    else
                    {
                        throw new VariableNotDefinedException("Variable not defined!");
                    }
                }

                switch (toEvaluate[0])
                {
                    case "out":
                        return EvaluateOut(toEvaluate[1], isRec);
                    case "load":
                        //TODO load file
                        return /*new FileInterpreter(toEvaluate[1]).FileName*/ "";
                    case "type":
                        return GetVarType(toEvaluate[1]);
                    case "uload":
                        return DeleteVar(toEvaluate[1]);
                    case "dumpVars":
                        return DumpAllVariables(toEvaluate[1]);
                    case "exists":
                        return Cache.Instance.Variables.ContainsKey(toEvaluate[1]).ToString().ToLower();
                    case "exit":
                        try
                        {
                            Environment.Exit(int.Parse(toEvaluate[1]));
                        }
                        catch (Exception e)
                        {
                            return e.Message;
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }

            return null;
        }

        private string DumpAllVariables(string s)
        {
            var sb = new StringBuilder();
            DataTypes dt = DataTypes.NONE;

            if (s != "all")
            {
                try
                {
                    dt = DataTypeFromString(s.ToUpper());
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }

            foreach (var variable in Cache.Instance.Variables)
            {
                if (variable.Value.DataType == dt || dt == DataTypes.NONE)
                {
                    sb.Append($"{variable.Key} = {variable.Value.Value}\n");
                }
            }

            sb.Length = sb.Length - 2;

            return sb.ToString();
        }

        private string DeleteVar(string s)
        {
            try
            {
                if (Cache.Instance.Variables.ContainsKey(s))
                {
                    Cache.Instance.Variables.Remove(s);
                    return "Variable unloaded!";
                }
                else
                {
                    throw new VariableNotDefinedException("Variable not defined!");
                }
            }
            catch (VariableNotDefinedException ve)
            {
                return ve.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string ReplaceWithVars(string s)
        {
            var reg = new Regex(@"\{([^\}]+)\}");
            var matches  = reg.Matches(s);

            foreach (var variable in matches)
            {
                var varName = variable.ToString().Replace("{", "").Replace("}", "");
                var data = Cache.Instance.Variables[varName].Value;

                if (Cache.Instance.Variables[varName].DataType == DataTypes.WORD)
                {
                    s = s.Replace(variable.ToString(), $"\"{data}\"");
                }
                else
                {
                    s = s.Replace(variable.ToString(), data);
                }

            }

            return s;
        }

        private string GetVarType(string s)
        { 
            try
            {
                if (Cache.Instance.Variables.ContainsKey(s))
                {
                    return Cache.Instance.Variables[s].DataType.ToString().ToLower();
                }
                else
                {
                    throw new VariableNotDefinedException("Variable not defined!");
                }
            }
            catch (VariableNotDefinedException ve)
            {
                return ve.Message;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public DataTypes DataTypeFromData(string data)
        {
            if (data.IsNum())
            {
                return DataTypes.NUM;
            }

            if (data.IsBinary())
            {
                return DataTypes.BINARY;
            }

            if (data.IsDec())
            {
                return DataTypes.DEC;
            }

            if (Regex.IsMatch(data, @"\'([^]]*)\'"))
            {
                return DataTypes.WORD;
            }

            throw new InvalidDataTypeException("Invalid data type!");
        }

        public DataTypes DataTypeFromString(string typeName)
        {
            switch (typeName.ToLower())
            {
                case "word":
                    return DataTypes.WORD;
                case "num":
                    return DataTypes.NUM;
                case "dec":
                    return DataTypes.DEC;
                case "binary":
                    return DataTypes.BINARY;
                default:
                    throw new InvalidDataTypeException("Given data type was invalid!");
            }
        }

        public OperationTypes EvaluateOperation(string operation, string toEvaluate)
        {
            if (operation == toEvaluate || operation == "")
            {
                return OperationTypes.NONE;
            }

            switch (operation)
            {
                case "case":
                    return OperationTypes.CASE;
                case "runala":
                    return OperationTypes.RUNALA;
                default:
                    throw new InvalidOperationException("Invalid operation!");
            }
        }
    }
}
