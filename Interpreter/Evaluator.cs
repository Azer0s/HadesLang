using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using org.mariuszgromada.math.mxparser;
using Output;
using StringExtension;
using Variables;
using static System.String;

namespace Interpreter
{
    public class Evaluator
    {
        public IScriptOutput ScriptOutput;
        public Evaluator(IScriptOutput scriptOutput)
        {
            ScriptOutput = scriptOutput;
        }

        #region Variables

        public string CreateVariable(string lineToInterprete, string access,Interpreter interpreter)
        {
            var groups = RegexCollection.Store.CreateVariable.Match(lineToInterprete).Groups.OfType<Group>()
                .Where(a => !string.IsNullOrEmpty(a.Value)).Select(a => a.Value).ToList();

            var exist = Exists(groups[1], access);
            if (exist.Exists)
            {
                return exist.Message;
            }

            Cache.Instance.Variables.Add(new Meta { Name = groups[1], Owner = access }, new Variable { Access = TypeParser.ParseAccessType(groups[3]), DataType = TypeParser.ParseDataType(groups[2]) });

            try
            {
                return groups.Count == 5 ? AssignToVariable($"{groups[1]} = {groups[4]}", access, true, interpreter) : $"{groups[1]} is undefined";
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        private (bool Exists,string Message) Exists(string name, string access)
        {
            if (VariableIsReachableAll(name))
            {
                return (true,"Variable already defined as reachable_all!");
            }

            if (Cache.Instance.Variables.ContainsKey(new Meta { Name = name, Owner = access }))
            {
                return (true, "Variable already exists!");
            }

            return (false, $"Variable {name} does not exist or the access was denied!");
        }

        private bool VariableIsReachableAll(string varname)
        {
            return Cache.Instance.Variables.Any(a => a.Key.Name == varname && a.Value.Access == AccessTypes.REACHABLE_ALL);
        }

        private string ReplaceWithVars(string lineToInterprete, string access)
        {
            var matches = RegexCollection.Store.Variable.Matches(lineToInterprete);

            var result = lineToInterprete;
            foreach (Match match in matches)
            {
                var variable = GetVariable(match.Value.TrimStart('$'), access);

                if (variable is Variable)
                {
                    result = result.Replace(match.Value,(variable as Variable).Value);
                }
                else
                { 
                    throw new Exception($"Variable {match.Value} is an object!");
                }
            }
            return result;
        }

        public IVariable GetVariable(string variable, string access)
        {
            if (VariableIsReachableAll(variable))
            {
                return Cache.Instance.Variables.First(a => a.Key.Name == variable).Value;
            }
            if (Exists(variable,access).Exists)
            {
                return Cache.Instance.Variables[new Meta {Name = variable, Owner = access}];
            }
            throw new Exception($"Variable {variable} does not exist or the access was denied!");
        }

        public string AssignToVariable(string lineToInterprete, string access, bool hardCompare,Interpreter interpreter)
        {
            var groups = RegexCollection.Store.Assignment.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

            var output = interpreter.Output;
            var eOutput = interpreter.ExplicitOutput;
            interpreter.Output = new NoOutput();
            interpreter.ExplicitOutput = new NoOutput();

            var result =interpreter.InterpretLine(groups[2].Value, access);
            interpreter.Output = output;
            interpreter.ExplicitOutput = eOutput;

            var success = false;

            if (Exists(groups[1].Value,access).Exists)
            {
                var variable = GetVariable(groups[1].Value, access);

                if (variable is Variable)
                {
                    var datatypeFromVariable = variable.DataType;
                    var datatypeFromData = DataTypeFromData(result,hardCompare);

                    if (datatypeFromVariable == DataTypes.WORD)
                    {
                        result = $"'{result.TrimStart('\'').TrimEnd('\'')}'";
                        success = SetVariable(groups[1].Value,result, access);
                    }
                    else if (datatypeFromData == DataTypes.NUM && datatypeFromVariable == DataTypes.DEC)
                    {
                        result = $"{result.Replace(" ", "")}.0";
                        success = SetVariable(groups[1].Value,result, access);
                    }
                    else if (datatypeFromData == datatypeFromVariable)
                    {
                        success = SetVariable(groups[1].Value,result, access);
                    }
                    else
                    {
                        throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                    }
                }
                else
                {
                    throw new Exception("Cant assign to variable of type object!");
                }
            }

            if (success)
            {
                return $"{groups[1]} is {result}";
            }
            throw new Exception($"{groups[1]} could not be set!");
        }

        private bool SetVariable(string name,string value, string access)
        {
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL))
            {
                var reachableAllVar = Cache.Instance.Variables.First(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL);
                var variable = Cache.Instance.Variables[reachableAllVar.Key] as Variable;
                if (variable != null)
                    variable.Value = value;

                return true;
            }
            if (Cache.Instance.Variables.Any(a => a.Key.Name== name && a.Key.Owner == access))
            {
                var variable = Cache.Instance.Variables[new Meta {Name = name, Owner = access}] as Variable;
                if (variable != null)
                    variable.Value = value;

                return true;
            }

            throw new Exception($"Variable {name} does not exist or the access was denied!");
        }

        public DataTypes DataTypeFromData(string result, bool hardCompare)
        {
            if (!hardCompare)
            {
                result = result.TrimStart('\'').TrimEnd('\'');
                if (RegexCollection.Store.IsNum.IsMatch(result))
                {
                    return DataTypes.NUM;
                }
                if (RegexCollection.Store.IsDec.IsMatch(result))
                {
                    return DataTypes.DEC;
                }
                if (RegexCollection.Store.IsBit.IsMatch(result.ToLower()))
                {
                    return DataTypes.BIT;
                }
                return DataTypes.WORD;
            }
            if (RegexCollection.Store.IsNum.IsMatch(result))
            {
                return DataTypes.NUM;
            }
            if (RegexCollection.Store.IsDec.IsMatch(result))
            {
                return DataTypes.DEC;
            }
            if (RegexCollection.Store.IsBit.IsMatch(result.ToLower()))
            {
                return DataTypes.BIT;
            }
            if (RegexCollection.Store.IsWord.IsMatch(result))
            {
                return DataTypes.WORD;
            }
            return DataTypes.NONE;
        }

        //TODO: load

        public string Unload(string variable, string access)
        {
            if (VariableIsReachableAll(variable))
            {
                Cache.Instance.Variables.Remove(Cache.Instance.Variables.First(a => a.Key.Name == variable).Key);
                return $"Unloaded variable {variable}!";
            }
            if (Exists(variable, access).Exists)
            {
                Cache.Instance.Variables.Remove(new Meta { Name = variable, Owner = access });
                return $"Unloaded variable {variable}!";
            }
            throw new Exception($"Variable {variable} does not exist or the access was denied!");
        }

        public string DumpVars(DataTypes dt)
        {
            var sb = new StringBuilder();

            foreach (var keyValuePair in dt != DataTypes.NONE ? Cache.Instance.Variables.Where(a => a.Value.DataType == dt).ToList() : Cache.Instance.Variables.ToList())
            {
                if (keyValuePair.Value is Variable)
                {
                    var kvpAsVar = (Variable) keyValuePair.Value;
                    sb.Append(string.IsNullOrEmpty(kvpAsVar.Value)
                        ? $"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=undefined\n"
                        : $"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}={kvpAsVar.Value}\n");
                }
                else
                {
                    sb.Append($"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=Object\n");
                }
            }

            sb.Length -= 1;

            return sb.ToString();
        }

        #endregion

        #region Libraries

        public string IncludeLib(string lineToInterprete, string access)
        {
            var group = RegexCollection.Store.With.Match(lineToInterprete).Groups.OfType<Group>().ToList();

            var exist = Exists(group[2].Value,access);
            if (exist.Exists)
            {
                return exist.Message;
            }
            var path = $"{Cache.Instance.LibraryLocation}\\{group[1]}.dll";

            if (!File.Exists(path))
            {
                return $"Library {group[1]} does not exist!";
            }

            try
            {
                Cache.Instance.Variables.Add(new Meta
                    {
                        Name = group[2].Value,
                        Owner = access
                    },
                    new Library
                    {
                        Access = AccessTypes.CLOSED,
                        DataType = DataTypes.NONE,
                        LibObject = Activator.CreateInstance(Assembly.LoadFile(path).GetType("Library.Library"))
                    });
            }
            catch (Exception)
            {
                return $"Error while loading library {group[1]}!";
            }
            
            return $"Succesfully loaded library {group[1]}!";
        }

        #endregion

        #region Calculations

        public (bool Success, string Result) EvaluateCalculation(string lineToInterprete, string access, Interpreter interpreter)
        {
            //TODO: Replace vars
            try
            {
                lineToInterprete = ReplaceWithVars(lineToInterprete, access);
            }
            catch (Exception e)
            {
                return (false, e.Message);
            }

            lineToInterprete = Cache.Instance.CharList.Aggregate(lineToInterprete, (current, s) => current.Replace(s, $" {s} "));

            if (lineToInterprete.ContainsFromList(new List<string> { ":", "->" }))
            {
                var split = lineToInterprete.StringSplit(' ', new[] { '\'', '[', ']' }).ToArray();

                lineToInterprete = Empty;

                foreach (var t in split)
                {
                    if (!(RegexCollection.Store.IsNum.IsMatch(t) || RegexCollection.Store.IsDec.IsMatch(t) || t.EqualsFromList(Cache.Instance.CharList)))
                    {
                        lineToInterprete += interpreter.InterpretLine(t, access);
                    }
                    else
                    {
                        if (t.EqualsFromList(Cache.Instance.CharList))
                        {
                            lineToInterprete += $" {t} ";
                        }
                        else
                        {
                            lineToInterprete += t;
                        }
                    }
                }
            }

            var args = lineToInterprete.StringSplit(' ').ToArray();

            var isBool = false;

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i].ToUpper().EqualsFromList(Cache.Instance.Replacement.Keys))
                {
                    args[i] = Cache.Instance.Replacement[args[i].ToUpper()];
                    isBool = true;
                }
                if (args[i].ToLower() == "true")
                {
                    args[i] = "1.0";
                    isBool = true;
                }
                if (args[i].ToLower() == "false")
                {
                    args[i] = "0.0";
                    isBool = true;
                }
            }

            var expression = new StringBuilder();

            foreach (var s in args)
            {
                expression.Append(s + " ");
            }

            expression.Length -= 1;

            var result = string.Empty;
            var ex = new Expression(expression.ToString());

            var calc = ex.calculate();
            if (isBool)
            {
                try
                {
                    if ((int)calc == 1)
                    {
                        result = bool.TrueString;
                    }
                    else if ((int)calc == 0)
                    {
                        result = bool.FalseString;
                    }
                    else
                    {
                        return (false, "Mixed types in boolean comparison are invalid!");
                    }
                }
                catch (Exception e)
                {
                    return (false, "Mixed types in boolean comparison are invalid!");
                }
            }
            else
            {
                result = calc.ToString(CultureInfo.InvariantCulture);
            }

            return (true, result);
        }

        #endregion

        #region System functions

        public void Exit(string lineToInterprete)
        {
            Environment.Exit(int.Parse(RegexCollection.Store.Exit.Match(lineToInterprete).Groups[1].Value));
        }

        public (string Value,string Message) Input(string lineToInterprete,string access, IScriptOutput output,Interpreter interpreter)
        {
            var varname = RegexCollection.Store.Input.Match(lineToInterprete).Groups[1].Value;
            var input = output.ReadLine();
            try
            {
                return (input, AssignToVariable($"{varname} = '{input}'", access, false, interpreter));
            }
            catch (Exception e)
            {
                return (null,e.Message);
            }
        }

        public string EvaluateOut(string lineToInterprete, string access, Interpreter interpreter)
        {
            var groups = RegexCollection.Store.Output.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

            var output = interpreter.Output;
            interpreter.Output = new NoOutput();

            string result;
            try
            {
                result = interpreter.InterpretLine(IsNullOrEmpty(groups[1].Value) ? groups[2].Value : groups[1].Value, access);
            }
            catch (Exception e)
            {
                throw e;
            }
            interpreter.Output = output;

            return $"'{result}'";
        }

        #endregion

        #region Custom functions

        public void CallCustomFunction(Group[] groups)
        {
            var firstOrDefault = Cache.Instance.Functions.FirstOrDefault(a => a.Name == groups[1].Value);
            firstOrDefault?.Execute(groups[2].Value.StringSplit(',').Select(a => a.Replace("'", "")));
        }

        #endregion

        public string CallMethod(string lineToInterprete, string access)
        {
            throw new NotImplementedException();
        }
    }
}