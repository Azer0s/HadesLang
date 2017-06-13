﻿using System;
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
        /// <summary>
        /// Regex pattern for var decleration
        /// </summary>
        public static string VarPattern = @"as (num|dec|word|bit)+ (reachable|reachable_all|closed)+";

        /// <summary>
        /// Available operators
        /// </summary>
        public static List<string> OperatorList = new List<string>
        {
            "+",
            "-",
            "*",
            "/",
            "Sqrt",
            "Sin",
            "Cos",
            "Tan",
            "Pow",
            "[Pi]",
            "[e]"
        };

        /// <summary>
        /// Available comperators
        /// </summary>
        public static List<string> CompOperatorList = new List<string> {"is", "or", "and", "not", "smaller", "bigger"};

        /// <summary>
        /// Suppresses errors
        /// </summary>
        public bool ForceOut;

        /// <summary>
        /// IO system (Default: console), used for interpreter implementation
        /// </summary>
        public IScriptOutput Output;

        /// <summary>
        /// Vars for custom function call
        /// </summary>
        private List<string> _vars = new List<string>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="output">Set the default IO</param>
        public Evaluator(IScriptOutput output)
        {
            Output = output;
        }

        /// <summary>
        /// Evaluates a boolean expression (bit type in Hades)
        /// </summary>
        /// <param name="toEvaluate">The expression to be evaluated</param>
        /// <param name="access">Delimiter for variable ownership</param>
        /// <returns></returns>
        public EvaluatedOperation EvaluateBool(string toEvaluate, string access)
        {
            //Expression is replaced with var values
            toEvaluate = ReplaceWithVars(toEvaluate, access);
            var groups = Regex.Match(toEvaluate.Replace(" ", ""), @"\[([^]]*)\]").Groups;
            var reg = groups[1].Value;
            var func =
                EvaluateOperation(toEvaluate.Replace(" ", "")
                    .Replace(groups[1].Value, "")
                    .Replace("[", "")
                    .Replace("]", ""));

            //No expression existent
            if (!string.IsNullOrWhiteSpace(reg))
            {
                //Expression doesn´t need to be evaluated
                if (reg == "true" || reg == "false")
                {
                    return new EvaluatedOperation(func, bool.Parse(reg));
                }

                //Boolean comparison
                if (reg.ContainsFromList(CompOperatorList))
                {
                    reg = reg.ToLower();
                    reg = reg.Replace("smalleris", "<=");
                    reg = reg.Replace("biggeris", ">=");
                    reg = reg.Replace("xor", "^");
                    reg = reg.Replace("is", "==");
                    reg = reg.Replace("or", "||");
                    reg = reg.Replace("and", "&&");
                    reg = reg.Replace("not", "!=");
                    reg = reg.Replace("smaller", "<");
                    reg = reg.Replace("bigger", ">");

                    var e = new Expression(reg).Evaluate().ToString().ToLower();

                    //Library returns 1/0 in some cases
                    //TODO: Try to fix
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

                    return new EvaluatedOperation(func, bool.Parse(e));
                }
            }
            return null;
        }

        /// <summary>
        /// Tries to, safely, evaluate a bool expression
        /// </summary>
        /// <param name="toEvaluate">The expression to be evaluated</param>
        /// <param name="access">Delimiter for variable ownership</param>
        /// <param name="result">Output of method (is null if method was unsuccesful)</param>
        /// <returns></returns>
        public bool TryEvaluateBool(string toEvaluate, string access, out string result)
        {
            try
            {
                result = EvaluateBool(toEvaluate, access).Result.ToString().ToLower();
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }

        /// <summary>
        /// Creates a variable in the var store
        /// </summary>
        /// <param name="toEvaluate"></param>
        /// <param name="access">Delimiter for variable ownership</param>
        /// <returns></returns>
        public string CreateVariable(string toEvaluate, string access)
        {
            try
            {
                var data = toEvaluate.Split(' ').ToList();
                data.RemoveAll(s => s.Equals("") || s.Equals("="));
                var dt = TypeParser.ParseDataType(data[2]);
                var at = TypeParser.ParseAccessType(data[3]);

                if (Exists(new Tuple<string, string>(data[0], access)))
                {
                    ForceOut = true;
                    throw new DefinitionDeniedException(
                        "Variable has already been defined with acces type: reachable_all");
                }
                if (data.Count > 4)
                {
                    for (var i = 5; i < data.Count; i++)
                    {
                        data[4] += $" {data[i]}";
                    }

                    Cache.Instance.Variables.Add(new Tuple<string, string>(data[0], access), new Types(at, dt, ""));
                    return AssignValueToVariable(data[0] + "=" + data[4], access);
                }
                else
                {
                    Cache.Instance.Variables.Add(new Tuple<string, string>(data[0], access), new Types(at, dt, ""));
                    return $"{data[0]} is undefined";
                }
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Assigns a value to a variable
        /// </summary>
        /// <param name="toEvaluate">Variable assignment expression</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        public string AssignValueToVariable(string toEvaluate, string access)
        {
            try
            {
                var data = toEvaluate.Split('=');
                data[1] = data[1].TrimStart(' ').TrimEnd(' ');
                var index = data[0].Replace(" ", "");
                var dt = GetVariable(index, access).Value.DataType;
                data[1] = ReplaceWithVars(data[1], access);
                var isOut = false;

                if (data[1].Contains(":"))
                {
                    string[] operation;

                    string AssignWithMethod(string[] op,string Access,bool quot)
                    {
                        try
                        {
                            var evaluated = EvaluateCall(op, access).Key;
                            return AssignValueToVariable(quot ? $"{data[0]} = '{evaluated}'" : $"{data[0]} = {evaluated}", Access);
                        }
                        catch (Exception e)
                        {
                            throw e;
                        }
                    }

                    if (data[1].Contains("->"))
                    {
                        operation = new[] {null, data[1]};
                        return AssignWithMethod(operation,access,false);
                    }

                    operation = data[1].Split(new[] {':'}, 2);
                    operation[0] = operation[0].Replace(" ", "");

                    if (operation[0] == "out" && !operation[1].Contains("[") && !operation[1].Contains("]") &&
                        !operation[1].ContainsFromList(OperatorList))
                    {
                        return AssignWithMethod(operation,access,true);
                    }

                    data[1] = "'" + EvaluateCall(operation, access).Key + "'";
                    isOut = true;
                }

                if (data[1].ContainsFromList(OperatorList))
                {
                    data[1] = EvaluateCalculation(data[1]);
                }

                if (Regex.IsMatch(data[1], @"\[([^]]*)\]"))
                {
                    data[1] = EvaluateBool(data[1], access).Result.ToString().ToLower();
                }

                //Used in method calls and self calls - dynamic casting 
                try
                {
                    if (dt != DataTypes.WORD && DataTypeFromData(data[1].Replace("'", ""), false) != DataTypes.WORD)
                    {
                        data[1] = data[1].Replace("'", "");
                    }
                }
                catch (Exception e)
                {
                    throw new InvalidDataAssignException(
                        "The data type of the variable does not match the assignment type!");
                }

                if (dt == DataTypeFromData(data[1],false) || (dt == DataTypes.DEC && DataTypeFromData(data[1],false) == DataTypes.NUM))
                {
                    if (dt == DataTypes.WORD)
                    {
                        data[1] = Regex.Match(data[1], @"\'([^]]*)\'").Groups[1].Value.TrimStart('\n');
                    }

                    if (dt != DataTypes.WORD)
                    {
                        data[1] = data[1].Replace(" ", "").Replace("\n","").Replace("\t","").Replace("'","");
                    }

                    if (dt == DataTypes.DEC)
                    {
                        data[1] = data[1].Replace(",", ".");
                    }
                    SetVariable(index, data[1], access);
                }else if(isOut && dt == DataTypes.WORD)
                {
                    SetVariable(index, data[1], access);
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

        /// <summary>
        /// Gets a variable + value from the var store
        /// </summary>
        /// <param name="index">Variable name</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        public KeyValuePair<string, Types> GetVariable(string index, string access)
        {
            if (Exists(new Tuple<string, string>(index, access)))
            {
                foreach (var varN in Cache.Instance.Variables)
                {
                    if ((varN.Value.Access == AccessTypes.REACHABLE_ALL) && varN.Key.Item1 == index)
                    {
                        return new KeyValuePair<string, Types>(varN.Key.Item1, varN.Value);
                    }
                }

                return new KeyValuePair<string, Types>(index,
                    Cache.Instance.Variables[new Tuple<string, string>(index, access)]);
            }
            else
            {
                throw new AccessDeniedException("You are note allowed to access this variable or the variable doesn´t exist!");
            }
        }

        /// <summary>
        /// Sets the value of a variable
        /// </summary>
        /// <param name="variable">Name of the varible</param>
        /// <param name="value">Value the variable should get</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        public void SetVariable(string variable, string value, string access)
        {
            foreach (var varN in Cache.Instance.Variables)
            {
                if (varN.Value.Access == AccessTypes.REACHABLE_ALL && varN.Key.Item1 == variable)
                {
                    Cache.Instance.Variables[varN.Key].Value = value;
                }
            }

            if (Exists(new Tuple<string, string>(variable, access)))
            {
                Cache.Instance.Variables[new Tuple<string, string>(variable, access)].Value = value;
            }
            else
            {
                throw new AccessDeniedException("You are note allowed to access this variable!");
            }
        }


        /// <summary>
        /// Evaluates a mathematical expression
        /// </summary>
        /// <param name="toEvaluate">Expression to be evaluated</param>
        /// <returns></returns>
        public string EvaluateCalculation(string toEvaluate)
        {
            toEvaluate = ReplaceWithVars(toEvaluate, "console");
            try
            {
                var data = toEvaluate.Split('+');

                foreach (var variable in data)
                {
                    if (DataTypeFromData(variable,false) != DataTypes.WORD)
                    {
                        goto tryNumeric;
                    }
                }

                var resultString = string.Empty;
                foreach (var variable in data)
                {
                    resultString += Regex.Match(variable, @"\'([^]]*)\'").Groups[1].Value;
                }

                return resultString;
            }
            catch (Exception)
            {
                // ignored
            }

            tryNumeric:
            try
            {
                var e = new Expression(toEvaluate)
                {
                    Parameters =
                    {
                        ["Pi"] = Math.PI,
                        ["E"] = Math.E
                    }
                };
                return e.Evaluate().ToString().Replace(",", ".");
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        /// <summary>
        /// Evaluates possible outputs for the "out" command
        /// </summary>
        /// <param name="toEvaluate">Expression to be evaluated</param>
        /// <param name="ignoreQuote">Whether quotes should be ignored or not (true in console use)</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        public string EvaluateOut(string toEvaluate, bool ignoreQuote, string access)
        {
            if (ignoreQuote)
            {
                toEvaluate = $"'{toEvaluate}'";
            }
            try
            {
                if (Regex.IsMatch(toEvaluate, @"\[([^]]*)\]"))
                {
                    return EvaluateBool(toEvaluate, access).Result.ToString().ToLower();
                }
                if (toEvaluate.ContainsFromList(OperatorList))
                {
                    return EvaluateCalculation(toEvaluate);
                }
                if (Regex.IsMatch(toEvaluate, @"\'([^]]*)\'"))
                {
                    return Regex.Match(toEvaluate, @"\'([^]]*)\'").Groups[1].Value;
                }
                if (Cache.Instance.Variables.ContainsKey(new Tuple<string, string>(toEvaluate, access)))
                {
                    return GetVariable(toEvaluate, access).Value.Value;
                }
                if (VariableIsReachableAll(toEvaluate))
                {
                    return GetVariable(toEvaluate, access).Value.Value;
                }
                var stringWithVars = ReplaceWithVars(toEvaluate, access);
                if (stringWithVars != toEvaluate)
                {
                    return stringWithVars;
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

        /// <summary>
        /// Gets input from the user
        /// </summary>
        /// <param name="varname">Name of the variable the input should be inserted to</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        public void GetIn(string varname, string access)
        {
            var input = Output.ReadLine();
            var dt = DataTypeFromData(input,true);
            var varDt = GetVariable(varname, access).Value.DataType;

            if (varDt == dt || (varDt == DataTypes.DEC && dt == DataTypes.NUM))
            {
                if (varDt == DataTypes.DEC)
                {
                    input = input.Replace(",", ".");
                }
                SetVariable(varname, input, access);
            }
            else
            {
                throw new InvalidDataAssignException(
                    "The data type of the variable does not match the assignment type!");
            }
        }

        /// <summary>
        /// Checks if a variable is global
        /// </summary>
        /// <param name="toEvaluate">Variable name</param>
        /// <returns></returns>
        private bool VariableIsReachableAll(string toEvaluate)
        {
            return Cache.Instance.Variables.Any(variable => variable.Value.Access == AccessTypes.REACHABLE_ALL && variable.Key.Item1 == toEvaluate);
        }

        /// <summary>
        /// Evaluates a generic call/command
        /// </summary>
        /// <param name="toEvaluate">The expression to be evaluated</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        public KeyValuePair<string, bool> EvaluateCall(string[] toEvaluate, string access)
        {
            var ignoreCase = false;
            string callResult = null;

            //only one information given
            if (toEvaluate.Length == 1)
            {
                if (TryEvaluateBool(toEvaluate[0], access, out string tryResult))
                {
                    return new KeyValuePair<string, bool>(tryResult, false);
                }
            }

            //nested commands, eg. out:[type:a]
            if (toEvaluate[0] != null && Regex.IsMatch(toEvaluate[1], @"\[([^]]*)\]") && (toEvaluate[1].Contains(":") || toEvaluate[1].Contains(":")))
            {
                //TODO: Support objects and method calls
                try
                {
                    var nested = toEvaluate[1].TrimStart('[').TrimEnd(']');

                    var parameters = nested.Contains(":") ? nested.Split(new[] { ':' }, 2) : nested.SplitToTwo("->", StringSplitOptions.None);

                    toEvaluate[1] = EvaluateCall(parameters, access).Key;
                    ignoreCase = true;
                }
                catch (Exception)
                {
                    // ignored
                }
            }

            //Checks where and if there is a method call
            var contains = false;
            var callIndex = 0;
            toEvaluate.ToList().ForEach(a =>
            {
                if (!string.IsNullOrEmpty(a) && a.Contains("->"))
                {
                    contains = true;
                    callIndex = toEvaluate.ToList().IndexOf(a);
                }
            });

            try
            {
                //Executes method call if there is one
                if (contains)
                {
                    var call = toEvaluate[callIndex].Split(new[] { "->" }, 2, StringSplitOptions.None);
                    call[0] = call[0].Replace(" ", "");

                    if (Cache.Instance.Variables.ContainsKey(new Tuple<string, string>(call[0], access)))
                    {
                        var variable = GetVariable(call[0], access);
                        if (variable.Value.DataType == DataTypes.OBJECT)
                        {
                            var methodData = call[1].Split(new[]{':'},2);
                            var method = variable.Value.Methods.Where(a => a.Name == methodData[0]).ToList();

                            var parameters = new Dictionary<string, string>();
                            try
                            {
                                var data = methodData[1].TrimStart('[').TrimEnd(']').CsvSplitter().ToArray();

                                if (data.Length != method.First().Parameters.Count)
                                {
                                    throw new InvalidOperationException("Method is being called incorrectly!");
                                }
                                for (var i = 0; i < data.Length; i++)
                                {
                                    parameters.Add(method.First().Parameters[i].Item1,data[i]);
                                }
                            }
                            catch (Exception)
                            {
                                // ignored
                            }

                            if (!Regex.IsMatch(methodData[1], @"(\[)([^]]*)(\])") || method.Count == 0)
                            {
                                throw new InvalidOperationException("Method does not exist or is being called incorrectly!");
                            }

                            var fi = new FileInterpreter(variable.Value.Lines, variable.Value.Methods, Output)
                            {
                                Parameters = parameters,
                                FileName = call[0] + "->" + methodData[0]
                            };
                            fi.ExecuteFromLineToLine(new Tuple<int, int>(method.First().Postition.Item1, method.First().Postition.Item2), true, out _);
                            callResult = fi.Return.Value.Value;
                            fi.Collect();
                            goto returnResult;
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

            //built in methods
                switch (toEvaluate[0])
                {
                    case "out":
                        callResult = EvaluateOut(toEvaluate[1], ignoreCase, access);
                        ForceOut = true;
                        goto returnResult;
                    case "in":
                        GetIn(toEvaluate[1], access);
                        goto returnResult;
                    case "load":
                        callResult = LoadFile(toEvaluate[1],access);
                        goto returnResult;
                    case "type":
                        callResult = GetVarType(toEvaluate[1], access);
                        goto returnResult;
                    case "dtype":
                        toEvaluate[1] = ReplaceWithVars(toEvaluate[1], access);
                        callResult = DataTypeFromData(toEvaluate[1],false).ToString().ToLower();
                        goto returnResult;
                    case "uload":
                        callResult = DeleteVar(toEvaluate[1], access);
                        goto returnResult;
                    case "dumpVars":
                        callResult = DumpAllVariables(toEvaluate[1]);
                        goto returnResult;
                    case "rand":
                        callResult = GetRandom(toEvaluate[1]);
                        goto returnResult;
                    case "eraseVars":
                        if (toEvaluate[1] == "1")
                        {
                            Cache.Instance.EraseVars = true;
                            callResult = "EraseVars set to true";
                            goto returnResult;
                        }
                        if (toEvaluate[1] == "0")
                        {
                            Cache.Instance.EraseVars = false;
                            callResult = "EraseVars set to false";
                            goto returnResult;
                        }
                        callResult = "Invalid input!";
                        goto returnResult;
                    case "exists":
                        return
                            new KeyValuePair<string, bool>(Exists(new Tuple<string, string>(toEvaluate[1], access)).ToString().ToLower(), false);
                    case "exit":
                        try
                        {
                            Environment.Exit(int.Parse(toEvaluate[1]));
                        }
                        catch (Exception e)
                        {
                            return new KeyValuePair<string, bool>(e.Message, true);
                        }
                        break;
                    default:
                        //Check if Cache contains previously loaded file
                        if (Cache.Instance.Variables.ContainsKey(new Tuple<string, string>($"'{access}'", $"'{access}'")))
                        {
                            //Get method and object
                            var obj = Cache.Instance.Variables[new Tuple<string, string>($"'{access}'", $"'{access}'")];
                            var method = obj.Methods.First(a => a.Name == toEvaluate[0]);
                            if (method != null)
                            {
                                //TODO: Add data to call toEvaluate[1]
                                var fi = new FileInterpreter(obj.Lines.GetRange(method.Postition.Item1, method.Postition.Item2 - 1), obj.Methods, Output);
                                fi.ExecuteFromLineToLine(new Tuple<int, int>(0, fi.Lines.Count), false, out _);
                                callResult = fi.Return.Value.Value;
                                fi.Collect();
                            }
                        }
                        else if (Cache.Instance.Functions.Count(a => a.Name == toEvaluate[0]) != 0)
                        {
                            if (Regex.IsMatch(toEvaluate[1], @"\[([^]]*)\]"))
                            {
                                toEvaluate[1] = toEvaluate[1].TrimStart('[').TrimEnd(']');
                                if (!toEvaluate[1].Contains(','))
                                {
                                    _vars.Add(toEvaluate[1]);
                                }
                                else
                                {
                                    _vars.AddRange(toEvaluate[1].CsvSplitter());
                                }
                                Cache.Instance.Functions.First(a => a.Name == toEvaluate[0]).Execute();
                                _vars.Clear();
                                return new KeyValuePair<string, bool>(string.Empty, false);
                            }
                            else
                            {
                                throw new InvalidOperationException("Method call was invalid!");
                            }
                        }
                        break;
                }

                returnResult:
                if (ForceOut)
                {
                    ForceOut = false;
                    return new KeyValuePair<string, bool>(callResult, true);
                }
                return new KeyValuePair<string, bool>(callResult, false);
            }
            catch (Exception e)
            {
                return new KeyValuePair<string, bool>(e.Message, true);
            }
        }

        public void SaveObject(string varname, string access, FileInterpreter fi)
        {
            var kwp = new KeyValuePair<Tuple<string, string>, Types>(new Tuple<string, string>(varname, access), new Types(AccessTypes.CLOSED, DataTypes.OBJECT, ""));
            kwp.Value.Lines = fi.Lines;
            kwp.Value.Methods = fi.Methods;

            Cache.Instance.Variables.Add(kwp.Key, kwp.Value);
        }

        private string LoadFile(string s,string access)
        {
            try
            {
                if (Regex.IsMatch(s, @"'([^]]*)' (as)+"))
                {
                    var fn = Regex.Match(s, @"'([^]]*)'").Value;
                    var varName = s.Replace(fn + " as ", "");

                    if (!Regex.IsMatch(s, @"'([^]]*)'"))
                    {
                        throw new InvalidFileNameException("Filename is invalid!");
                    }
                    var fiW = new FileInterpreter(fn,Output);
                    fiW.LoadFunctions();
                    fiW.LoadReachableVars();

                    SaveObject(varName, access, fiW);
                }
                else
                {
                    var fiL = new FileInterpreter(s,Output);
                    Cache.Instance.LoadFiles.Add(s);
                    SaveObject(s, s, fiL);
                    fiL.LoadFunctions();
                    fiL.LoadAll();
                    var returnVal = fiL.Return.Value.Value;

                    try
                    {
                        if (!string.IsNullOrEmpty(returnVal))
                        {
                            return returnVal;
                        }
                    }
                    catch (Exception)
                    {
                        // ignored
                    }                    
                }
                if (!Regex.IsMatch(s, @"'([^]]*)'")) throw new InvalidFileNameException("Filename is invalid!"); 
                
                return "Loaded succesfuly!";
            }
            catch (Exception e)
            {
                try
                {
                    RemoveVariable(s, s);
                }
                catch (Exception exception)
                {
                    // ignored
                }
                return e.Message;
            }
        }

        /// <summary>
        /// Gets a random number
        /// </summary>
        /// <param name="s">Maximum</param>
        /// <returns></returns>
        private string GetRandom(string s)
        {
            if (s == "void")
            {
                return new Random().Next().ToString();
            }
            else
            {
                try
                {
                    return new Random().Next(int.Parse(s)).ToString();
                }
                catch (Exception e)
                {
                    return e.Message;
                }
            }
        }

        /// <summary>
        /// Outputs all variables to the screen
        /// </summary>
        /// <param name="s">Filtertype</param>
        /// <returns></returns>
        private string DumpAllVariables(string s)
        {
            var sb = new StringBuilder();
            var dt = DataTypes.NONE;

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

            if (Cache.Instance.Variables.Count == 0)
            {
                return string.Empty;
            }

            foreach (var variable in Cache.Instance.Variables)
            {
                if (variable.Value.DataType == dt || dt == DataTypes.NONE)
                {
                    sb.Append(variable.Value.DataType == DataTypes.OBJECT
                        ? $"{variable.Key.Item1}@{variable.Key.Item2} = OBJECT\n"
                        : $"{variable.Key.Item1}@{variable.Key.Item2} = {variable.Value.Value}\n");
                }
            }

            sb.Length = sb.Length - 1;

            return sb.ToString();
        }

        /// <summary>
        /// Same as RemoveVariable - Generates output
        /// </summary>
        /// <param name="s">The name of the variable</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        private string DeleteVar(string s,string access)
        {
            try
            {
                if (Cache.Instance.Variables.ContainsKey(new Tuple<string, string>(s,access)))
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

        /// <summary>
        /// Removes a variable from the var cache
        /// </summary>
        /// <param name="s">The name of the variable</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        private void RemoveVariable(string s, string access)
        {
            if (Exists(new Tuple<string, string>(s,access)))
            {
                Cache.Instance.Variables.Remove(new Tuple<string, string>(s,access));
            }
            else
            {
                throw new AccessDeniedException("You are note allowed to access this variable!");
            }
        }

        /// <summary>
        /// Checks if a variable exists in the var store
        /// </summary>
        /// <param name="instanceVariable">Tuple of variable + access</param>
        /// <returns></returns>
        private bool Exists(Tuple<string, string> instanceVariable)
        {
            if (Cache.Instance.Variables.Any(variable => variable.Value.Access == AccessTypes.REACHABLE_ALL && variable.Key.Item1 == instanceVariable.Item1))
            {
                return true;
            }

            try
            {
                var test = Cache.Instance.Variables[instanceVariable].Value;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Replaces the placeholder in a string with the data from a variable in the var store
        /// </summary>
        /// <param name="s">Expression with placeholders</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        public string ReplaceWithVars(string s,string access)
        {
            var reg = new Regex(@"\{([^\}]+)\}");
            var matches  = reg.Matches(s);

            foreach (var variable in matches)
            {
                var varName = variable.ToString().Replace("{", "").Replace("}", "");
                var data = GetVariable(varName, access).Value.Value;

                s = s.Replace(variable.ToString(), GetVariable(varName, access).Value.DataType == DataTypes.WORD ? $"'{data}'" : data);
            }

            return s;
        }

        /// <summary>
        /// Returns the data type from the data of a variable in the var store
        /// </summary>
        /// <param name="s">Variable</param>
        /// <param name="access">Delimiter for variabel ownership</param>
        /// <returns></returns>
        private string GetVarType(string s,string access)
        { 
            try
            {
                if (Cache.Instance.Variables.ContainsKey(new Tuple<string, string>(s,access)))
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

        /// <summary>
        /// Returns the data type of the data within the input string
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ignoreQuotes"></param>
        /// <returns></returns>
        public DataTypes DataTypeFromData(string data, bool ignoreQuotes)
        {
            if (data.IsNum())
            {
                return DataTypes.NUM;
            }

            if (data.IsBit())
            {
                return DataTypes.BIT;
            }

            if (data.IsDec())
            {
                return DataTypes.DEC;
            }

            if (Regex.IsMatch(data, @"\'([^]]*)\'") || ignoreQuotes)
            {
                return DataTypes.WORD;
            }

            throw new InvalidDataTypeException("Invalid data type!");
        }

        /// <summary>
        /// Returns an enum for the data type according to the input string
        /// </summary>
        /// <param name="typeName"></param>
        /// <returns></returns>
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
                case "bit":
                    return DataTypes.BIT;
                case "object":
                    return DataTypes.OBJECT;
                default:
                    throw new InvalidDataTypeException("Given data type was invalid!");
            }
        }

        /// <summary>
        /// Returns an enum for the operation type according to the input string
        /// </summary>
        /// <param name="operation"></param>
        /// <returns></returns>
        public OperationTypes EvaluateOperation(string operation)
        {
            if (operation == "")
            {
                return OperationTypes.NONE;
            }

            switch (operation.ToLower())
            {
                case "case":
                    return OperationTypes.CASE;
                case "aslongas":
                    return OperationTypes.ASLONGAS;
                default:
                    throw new InvalidOperationException("Invalid operation!");
            }
        }

        public List<string> GetFunctionValues()
        {
            return _vars;
        }
    }
}
