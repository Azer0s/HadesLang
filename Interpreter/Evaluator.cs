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
        public static List<string> OperatorList = new List<string> {"+", "-", "*", "/", "Sqrt", "Sin", "Cos", "Tan"};
        public static List<string> CompOperatorList = new List<string> {"is","or","and","not","smaller","bigger"};
        private bool ForceOut = false;

        public EvaluatedOperation EvaluateBool(string toEvaluate, string access)
        {
            toEvaluate = ReplaceWithVars(toEvaluate, access);
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

                if (reg.ContainsFromList(CompOperatorList))
                {
                    reg = reg.Replace("smallerIs", "<=");
                    reg = reg.Replace("biggerIs", ">=");
                    reg = reg.Replace("xor", "^");
                    reg = reg.Replace("is", "==");
                    reg = reg.Replace("or", "||");
                    reg = reg.Replace("and", "&&");
                    reg = reg.Replace("not", "!=");
                    reg = reg.Replace("smaller", "<");
                    reg = reg.Replace("bigger", ">");

                    var e = new Expression(reg).Evaluate().ToString().ToLower();

                    if (e.IsNum())
                    {
                        if (e == "1")
                        {
                            e = "true";
                        }
                        if (e == "0")
                        {
                            e = "false";
                        }
                    }

                    return new EvaluatedOperation(func,bool.Parse(e));
                }
            }
            return null;
        }

        public bool TryEvaluateBool(string toEvaluate, string access, out string result)
        {
            try
            {
                toEvaluate = ReplaceWithVars(toEvaluate, access);
                var reg = Regex.Match(toEvaluate.Replace(" ", ""), @"\[([^]]*)\]").Groups[1].Value;

                if (reg == "")
                {
                    reg = toEvaluate;
                }

                if (!string.IsNullOrWhiteSpace(reg))
                {
                    if (reg == "true" || reg == "false")
                    {
                        result = reg;
                        return true;
                    }

                    if (reg.ContainsFromList(CompOperatorList))
                    {
                        reg = reg.Replace("smallerIs", "<=");
                        reg = reg.Replace("biggerIs", ">=");
                        reg = reg.Replace("is", "==");
                        reg = reg.Replace("xor", "^");
                        reg = reg.Replace("or", "||");
                        reg = reg.Replace("and", "&&");
                        reg = reg.Replace("not", "!=");
                        reg = reg.Replace("smaller", "<");
                        reg = reg.Replace("bigger", ">");

                        var e = new Expression(reg).Evaluate().ToString().ToLower();

                        if (e.IsNum())
                        {
                            if (e == "1")
                            {
                                e = "true";
                            }
                            if (e == "0")
                            {
                                e = "false";
                            }
                        }

                        result = e;
                        return true;
                    }
                }
            }
            catch (Exception)
            {
            }
            result = null;
            return false;
        }

        public string CreateVariable(string toEvaluate,string access)
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
                        new Types(TypeParser.ParseAccessType(data[3]), TypeParser.ParseDataType(data[2]), "", access));
                    return AssignValueToVariable(data[0] + "=" + data[4],access);
                }
                else
                {
                    Cache.Instance.Variables.Add(data[0],
                        new Types(TypeParser.ParseAccessType(data[3]), TypeParser.ParseDataType(data[2]), "",access));
                    return $"{data[0]} is undefined";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        public string AssignValueToVariable(string toEvaluate,string access)
        {
            try
            {
                var data = toEvaluate.Split('=');
                var index = data[0].Replace(" ", "");
                var dt = GetVariable(index,access).Value.DataType;
                data[1] = ReplaceWithVars(data[1],access);
                var isOut = false;

                if (data[1].Contains(":"))
                {
                    var operation = data[1].Split(':');
                    operation[0] = operation[0].Replace(" ", "");
                    data[1] = "'" + EvaluateCall(operation,access) + "'";
                    isOut = true;
                }

                if (data[1].ContainsFromList(OperatorList))
                {
                    data[1] = EvaluateCalculation(data[1]);
                }

                if (Regex.IsMatch(data[1], @"\[([^]]*)\]"))
                {
                    data[1] = EvaluateBool(data[1],access).Result.ToString().ToLower();
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
                    SetVariable(index,data[1],access);
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

        public KeyValuePair<string,Types> GetVariable(string index,string access)
        {
            if (access == Cache.Instance.Variables[index].Owner)
            {
                return new KeyValuePair<string, Types>(index, Cache.Instance.Variables[index]);
            }
            else
            {
                throw new AccessDeniedException("You are note allowed to access this variable!");
            }
        }

        public void SetVariable(string variable, string value,string access)
        {
            if (Cache.Instance.Variables[variable].Owner == access)
            {
                Cache.Instance.Variables[variable].Value = value;
            }
            else
            {
                throw new AccessDeniedException("You are note allowed to access this variable!");
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

        public string EvaluateOut(string toEvaluate, bool ignoreQuote,string access)
        {
            if (ignoreQuote)
            {
                toEvaluate = $"'{toEvaluate}'";
            }
            try
            {
                if (Regex.IsMatch(toEvaluate, @"\[([^]]*)\]"))
                {
                    return EvaluateBool(toEvaluate,access).Result.ToString().ToLower();
                }
                if (toEvaluate.ContainsFromList(OperatorList))
                {
                    return EvaluateCalculation(toEvaluate);
                }
                if (Regex.IsMatch(toEvaluate, @"\'([^]]*)\'"))
                {
                    return Regex.Match(toEvaluate, @"\'([^]]*)\'").Groups[1].Value;
                }
                if (Cache.Instance.Variables.ContainsKey(toEvaluate))
                {
                    return GetVariable(toEvaluate,access).Value.Value;
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

        public KeyValuePair<string,bool> EvaluateCall(string[] toEvaluate,string access)
        {
            if (toEvaluate.Length == 1)
            {
                string tryResult;
                if (TryEvaluateBool(toEvaluate[0],access,out tryResult))
                {
                    return new KeyValuePair<string, bool>(tryResult, false);
                }
            }

            try
            {
                toEvaluate[1] = ReplaceWithVars(toEvaluate[1],access);
                bool isRec = false;

                if (Regex.IsMatch(toEvaluate[1], @"(\[)([^]]*)(\])"))
                {
                    int index = toEvaluate[1].IndexOf("[");
                    toEvaluate[1] = (index < 0)
                    ? toEvaluate[1]
                    : toEvaluate[1].Remove(index, "[".Length);
                    toEvaluate[1] = toEvaluate[1].Substring(0, toEvaluate[1].LastIndexOf("]"));

                    var result = EvaluateCall(toEvaluate[1].Split(new[] { ':' }, 2),access);
                    toEvaluate[1] = result.Key;
                    isRec = true;
                }

                if (toEvaluate[1].Contains("->"))
                {
                    var call = toEvaluate[1].Split(new[] { "->" }, 2, StringSplitOptions.None);

                    if (Cache.Instance.Variables.ContainsKey(call[0]))
                    {
                        if (GetVariable(call[0],access).Value.DataType == DataTypes.OBJECT)
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

                string callResult = null;
                switch (toEvaluate[0])
                {
                    case "out":
                        callResult = EvaluateOut(toEvaluate[1], isRec, access);
                        ForceOut = true;
                        goto returnResult;
                    case "load":
                        //TODO load file new FileInterpreter(toEvaluate[1]).FileName
                        return new KeyValuePair<string, bool>("",false);
                    case "type":
                        callResult = GetVarType(toEvaluate[1], access);
                        goto returnResult;
                    case "uload":
                        callResult = DeleteVar(toEvaluate[1], access);
                        goto returnResult;
                    case "dumpVars":
                        callResult = DumpAllVariables(toEvaluate[1]);
                        goto returnResult;
                    case "exists":
                        return new KeyValuePair<string, bool>(Cache.Instance.Variables.ContainsKey(toEvaluate[1]).ToString().ToLower(),false);
                    case "exit":
                        try
                        {
                            Environment.Exit(int.Parse(toEvaluate[1]));
                        }
                        catch (Exception e)
                        {
                            return new KeyValuePair<string, bool>(e.Message,true);
                        }
                        break;
                }

                returnResult:
                if (ForceOut)
                {
                    ForceOut = false;
                    return new KeyValuePair<string, bool>(callResult,true);
                }
                return new KeyValuePair<string, bool>(callResult,false);
            }
            catch (Exception e)
            {
                return new KeyValuePair<string, bool>(e.Message,true);
            }

            return new KeyValuePair<string, bool>();
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

        private string DeleteVar(string s,string access)
        {
            try
            {
                if (Cache.Instance.Variables.ContainsKey(s))
                {
                    RemoveVariable(s,access);
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

        private void RemoveVariable(string s, string access)
        {
            if (Cache.Instance.Variables[s].Owner == access)
            {
                Cache.Instance.Variables.Remove(s);
            }
            else
            {
                throw new AccessDeniedException("You are note allowed to access this variable!");
            }
        }

        public string ReplaceWithVars(string s,string access)
        {
            var reg = new Regex(@"\{([^\}]+)\}");
            var matches  = reg.Matches(s);

            foreach (var variable in matches)
            {
                var varName = variable.ToString().Replace("{", "").Replace("}", "");
                var data = GetVariable(varName,access).Value.Value;

                if (GetVariable(varName, access).Value.DataType == DataTypes.WORD)
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

        private string GetVarType(string s,string access)
        { 
            try
            {
                if (Cache.Instance.Variables.ContainsKey(s))
                {
                    return GetVariable(s, access).Value.DataType.ToString().ToLower();
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
