using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
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
        #region Variables

        public string CreateVariable(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file)
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
                return groups.Count == 5 ? AssignToVariable($"{groups[1]} = {groups[4]}", access, true, interpreter, file) : $"{groups[1]} is undefined";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string CreateArray(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.CreateArray.Match(lineToInterprete).Groups.OfType<Group>()
                .Where(a => !string.IsNullOrEmpty(a.Value)).Select(a => a.Value).ToList();

            var exist = Exists(groups[1], access);
            if (exist.Exists)
            {
                return exist.Message;
            }

            Cache.Instance.Variables.Add(new Meta { Name = groups[1], Owner = access },
                groups[3] == "*"
                    ? new Variables.Array
                    {
                        Access = TypeParser.ParseAccessType(groups[4]),
                        DataType = TypeParser.ParseDataType(groups[2]),
                        Values = new Dictionary<int, string>()
                    }
                    : new Variables.Array
                    {
                        Access = TypeParser.ParseAccessType(groups[4]),
                        DataType = TypeParser.ParseDataType(groups[2]),
                        Values = new Dictionary<int, string>(int.Parse(groups[3])),
                        Capacity = int.Parse(groups[3])
                    });

            try
            {
                return groups.Count == 6 ? AssignToArray($"{groups[1]} = {groups[5]}", access, interpreter, file) : $"{groups[1]} is undefined";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string AssignToArray(string s, string access, Interpreter interpreter, FileInterpreter file)
        {
            var output = interpreter.Output;
            interpreter.Output = new NoOutput();
            interpreter.MuteOut = true;
            var groups = RegexCollection.Store.Assignment.Match(s).Groups.OfType<Group>().Select(a => a.Value).ToList();
            var result = RegexCollection.Store.ArrayValues.IsMatch(groups[2])
                ? groups[2]
                : interpreter.InterpretLine(groups[2], access, file);
            if (!RegexCollection.Store.ArrayValues.IsMatch(result))
            {
                interpreter.Output = output;
                interpreter.MuteOut = false;
                throw new Exception("Invalid array format!");
            }

            var split = RegexCollection.Store.ArrayValues.Match(result).Groups[1].Value.StringSplit(',').ToList();

            for (var sp = 0; sp < split.Count; sp++)
            {
                split[sp] = interpreter.InterpretLine(split[sp].TrimStart(' ').TrimEnd(' '),access,file);
            }

            var success = false;
            if (Exists(groups[1], access).Exists)
            {
                var variable = GetVariable(groups[1], access);
                var datatypeFromVariable = variable.DataType;

                var index = 0;
                var list = split.Select(group =>
                {
                    var datatypeFromData = DataTypeFromData(group, false);
                    if (datatypeFromVariable == DataTypes.WORD)
                    {
                        index++;
                        return new KeyValuePair<int, string>(index - 1, $"'{group.TrimStart('\'').TrimEnd('\'')}'");
                    }
                    if (datatypeFromData == DataTypes.NUM && datatypeFromVariable == DataTypes.DEC)
                    {
                        index++;
                        return new KeyValuePair<int, string>(index - 1, $"{group.Replace(" ", "")}.0");
                    }
                    if (datatypeFromData == datatypeFromVariable)
                    {
                        index++;
                        return new KeyValuePair<int, string>(index - 1, group);
                    }

                    interpreter.Output = output;
                    interpreter.MuteOut = false;
                    throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                }).ToList();

                try
                {
                    success = SetArray(groups[1], access, list.ToDictionary(pair => pair.Key, pair => pair.Value.TrimStart(' ')));
                }
                catch (Exception e)
                {
                    interpreter.Output = output;
                    interpreter.MuteOut = false;
                    throw;
                }
            }

            interpreter.Output = output;
            interpreter.MuteOut = false;
            if (success)
            {
                return $"{groups[1]} is {{{Join(",",split)}}}";
            }
            throw new Exception($"{groups[1]} could not be set!");
        }

        public string AssignToArrayAtPos(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file)
        {
            var output = interpreter.Output;
            interpreter.Output = new NoOutput();
            interpreter.MuteOut = true;
            var groups = RegexCollection.Store.ArrayAssignment.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();

            var exists = Exists(groups[1], access);
            if (exists.Exists)
            {
                var variable = GetVariable(groups[1], access);
                var datatypeFromVariable = variable.DataType;
                groups[3] = interpreter.InterpretLine(groups[3], access, file);
                var datatypeFromData = DataTypeFromData(groups[3], false);
                int position;
                try
                {
                    position = int.Parse(interpreter.InterpretLine(groups[2], access, file));
                }
                catch (Exception e)
                {
                    interpreter.Output = output;
                    interpreter.MuteOut = false;

                    throw e;
                }

                if (datatypeFromVariable == DataTypes.WORD)
                {
                    try
                    {
                        SetArrayAtPos(groups[1], access, $"'{groups[3].TrimStart('\'').TrimEnd('\'')}'", position);
                    }
                    catch (Exception e)
                    {
                        interpreter.Output = output;
                        interpreter.MuteOut = false;

                        throw e;
                    }
                }
                else if (datatypeFromData == DataTypes.NUM && datatypeFromVariable == DataTypes.DEC)
                {
                    try
                    {
                        SetArrayAtPos(groups[1], access, $"{groups[3].Replace(" ", "")}.0", position);
                    }
                    catch (Exception e)
                    {
                        interpreter.Output = output;
                        interpreter.MuteOut = false;

                        throw e;
                    }
                }
                else if (datatypeFromData == datatypeFromVariable)
                {
                    try
                    {
                        SetArrayAtPos(groups[1], access, groups[3], position);
                    }
                    catch (Exception e)
                    {
                        interpreter.Output = output;
                        interpreter.MuteOut = false;

                        throw e;
                    }
                }
                else
                {
                    interpreter.Output = output;
                    interpreter.MuteOut = false;

                    throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                }

                interpreter.Output = output;
                interpreter.MuteOut = false;

                return $"{groups[1]}[{position}] is {groups[3]}";
            }

            interpreter.Output = output;
            interpreter.MuteOut = false;

            throw new Exception($"{exists.Message}");
        }

        private void SetArrayAtPos(string name, string access, string value, int position)
        {
            Variables.Array variable;
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL))
            {
                var reachableAllVar =
                    Cache.Instance.Variables.First(a => a.Key.Name == name &&
                                                        a.Value.Access == AccessTypes.REACHABLE_ALL);
                variable = Cache.Instance.Variables[reachableAllVar.Key] as Variables.Array;
            }
            else if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Key.Owner == access))
            {
                variable = Cache.Instance.Variables[new Meta { Name = name, Owner = access }] as Variables.Array;
            }
            else
            {
                throw new Exception($"Variable {name} does not exist or the access was denied!");

            }

            if (variable != null && position < variable.Capacity)
            {
                try
                {
                    variable.Values[position] = value;
                }
                catch (Exception)
                {
                    variable.Values.Add(position, value);
                }
            }
            else
            {
                throw new Exception($"Index {position} in array {name} is out of bounds!");
            }
        }

        private bool SetArray(string name, string access, Dictionary<int, string> values)
        {
            Variables.Array variable = null;
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL))
            {
                var reachableAllVar = Cache.Instance.Variables.First(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL);
                variable = Cache.Instance.Variables[reachableAllVar.Key] as Variables.Array;
            }
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Key.Owner == access))
            {
                variable = Cache.Instance.Variables[new Meta { Name = name, Owner = access }] as Variables.Array;
            }

            if (variable != null)
            {
                if (values.Count <= variable.Capacity)
                {
                    variable.Values = values;
                    return true;
                }
                throw new Exception($"Array is bigger than capacity!");
            }
            throw new Exception($"Variable {name} does not exist or the access was denied!");
        }

        public (bool Exists, string Message) Exists(string name, string access)
        {
            if (VariableIsReachableAll(name))
            {
                return (true, "Variable already defined as reachable_all!");
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

        public string ReplaceWithVars(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file)
        {
            var matches = RegexCollection.Store.Variable.Matches(lineToInterprete);

            var result = lineToInterprete;
            foreach (Match match in matches)
            {
                var variable = GetVariable(match.Groups[1].Value.TrimStart('$'), access);

                if (variable is Variable)
                {
                    result = result.Replace(match.Groups[1].Value, (variable as Variable).Value);
                }
                else if (variable is Variables.Array)
                {
                    if (RegexCollection.Store.ArrayVariable.IsMatch(lineToInterprete))
                    {
                        result = result.Replace(match.Value, GetArrayValue(match.Value, access, interpreter, file));
                    }
                    else
                    {
                        throw new Exception($"Variable {match.Value} is not an array!");
                    }
                }
                else
                {
                    throw new Exception($"Variable {match.Value} is an object!");
                }
            }
            return result;
        }

        public string GetArrayValue(string name, string access, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.ArrayVariable.Match(name).Groups;
            var variable = GetVariable(groups[1].Value.TrimStart('$'), access);

            if (variable is Variables.Array)
            {
                var output = interpreter.Output;
                var index = -1;
                try
                {
                    interpreter.Output = new NoOutput();
                    index = int.Parse(interpreter.InterpretLine(groups[2].Value, access, file));
                    var value = (variable as Variables.Array).Values[index];
                    interpreter.Output = output;

                    return value;
                }
                catch (Exception)
                {
                    interpreter.Output = output;
                    throw new Exception($"Index {index} in array {groups[1].Value} is out of bounds!");
                }
            }

            throw new Exception($"Could not get value of idex {groups[2]} in array {name}!");
        }

        public string InDeCrease(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file,string altAccess)
        {
            var groups = RegexCollection.Store.InDeCrease.Match(lineToInterprete).Groups.OfType<Group>().Select(a => a.Value).ToArray();
            lineToInterprete = $"{groups[1]} = ${groups[1]} {groups[2]} 1";
            return interpreter.InterpretLine(lineToInterprete, access, file,altAccess);
        }

        public IVariable GetVariable(string variable, string access)
        {
            if (VariableIsReachableAll(variable))
            {
                return Cache.Instance.Variables.First(a => a.Key.Name == variable).Value;
            }
            if (Exists(variable, access).Exists)
            {
                return Cache.Instance.Variables[new Meta { Name = variable, Owner = access }];
            }
            throw new Exception($"Variable {variable} does not exist or the access was denied!");
        }

        public string AssignToVariable(string lineToInterprete, string access, bool hardCompare, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.Assignment.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

            if (RegexCollection.Store.ArrayValues.IsMatch(groups[2].Value))
            {
                return AssignToArray(lineToInterprete, access, interpreter, file);
            }

            var output = interpreter.Output;
            interpreter.Output = new NoOutput();
            interpreter.MuteOut = true;
            var result = interpreter.InterpretLine(groups[2].Value, access, file);
            interpreter.MuteOut = false;
            interpreter.Output = output;

            var success = false;

            if (Exists(groups[1].Value, access).Exists)
            {
                var variable = GetVariable(groups[1].Value, access);

                if (variable is Variable)
                {
                    var datatypeFromVariable = variable.DataType;
                    var datatypeFromData = DataTypeFromData(result, hardCompare);

                    if (datatypeFromVariable == DataTypes.WORD)
                    {
                        result = $"'{result.TrimStart('\'').TrimEnd('\'')}'";
                        success = SetVariable(groups[1].Value, result, access);
                    }
                    else if (datatypeFromData == DataTypes.NUM && datatypeFromVariable == DataTypes.DEC)
                    {
                        result = $"{result.Replace(" ", "")}.0";
                        success = SetVariable(groups[1].Value, result, access);
                    }
                    else if (datatypeFromData == datatypeFromVariable)
                    {
                        success = SetVariable(groups[1].Value, result, access);
                    }
                    else
                    {
                        throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                    }
                }
                else if (variable is Variables.Array)
                {
                    return AssignToArray(lineToInterprete, access, interpreter, file);
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

        private bool SetVariable(string name, string value, string access)
        {
            Variable variable = null;
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL))
            {
                var reachableAllVar = Cache.Instance.Variables.First(a => a.Key.Name == name && a.Value.Access == AccessTypes.REACHABLE_ALL);
                variable = Cache.Instance.Variables[reachableAllVar.Key] as Variable;
            }
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Key.Owner == access))
            {
                variable = Cache.Instance.Variables[new Meta { Name = name, Owner = access }] as Variable;
            }

            if (variable != null)
            {
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

        public string LoadAs(string path, string varName, string access, Interpreter interpreter)
        {
            var fileInterpreter = new FileInterpreter(path);
            var result = Exists(varName, access);
            if (!result.Exists)
            {
                Cache.Instance.Variables.Add(new Meta{Name = varName,Owner = access}, fileInterpreter);
            }
            else
            {
                throw new Exception(result.Message);
            }

            return fileInterpreter.Execute(interpreter,path).Value;
        }

        public string LoadFile(string lineToInterprete,string access, Interpreter interpreter)
        {
            var groups = RegexCollection.Store.Load.Match(lineToInterprete).Groups.OfType<Group>().Select(a => a.Value).ToList();
            var output = interpreter.Output;
            interpreter.Output = new NoOutput();
            var path = !IsNullOrEmpty(groups[2]) ? interpreter.InterpretLine(groups[2],access,null).TrimStart('\'').TrimEnd('\'') : groups[1];
            interpreter.Output = output;

            if (!IsNullOrEmpty(groups[3]))
            {
                return LoadAs(path, groups[3], access, interpreter);
            }

            var result = new FileInterpreter(path).Execute(interpreter,path);

            if (Cache.Instance.EraseVars)
            {
                try
                {
                    Unload("all", path);
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return result.Value;
        }

        public string Unload(string variable, string access)
        {
            if (variable == "all")
            {
                foreach (var keyValuePair in Cache.Instance.Variables.Where(a => a.Key.Owner == access).Select(a => a).ToList())
                {
                    Unload(keyValuePair.Key.Name, access);
                }

                return $"Unloaded all variables from {access}!";
            }

            var obj = GetVariable(variable, access);
            if (obj is FileInterpreter && Cache.Instance.EraseVars)
            {
                Unload("all", (obj as FileInterpreter).FAccess);
            }

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

        public string GetObjectVar(string obj, string varname,string acccess, Interpreter interpreter)
        {
            if (!Exists(obj,acccess).Exists)
            {
                throw new Exception($"Object {obj} does not exist!");
            }

            var varObj = GetVariable(obj, acccess);

            if (varObj is FileInterpreter)
            {
                var fileVariable = GetVariable(varname, (varObj as FileInterpreter).FAccess);

                if (fileVariable.Access == AccessTypes.CLOSED)
                {
                    throw new Exception($"The access to variable {varname} was denied!");
                }

                if (fileVariable is Variable)
                {
                    return (fileVariable as Variable).Value;
                }

                var output = interpreter.Output;
                interpreter.Output = new NoOutput();
                var fileInterpreter = varObj as FileInterpreter;
                var result = interpreter.InterpretLine(varname, fileInterpreter.FAccess, fileInterpreter);
                interpreter.Output = output;
                return result;
            }
            throw new Exception($"Variable {obj} is not of type object!");
        }

        public string DumpVars(DataTypes dt)
        {
            var sb = new StringBuilder();

            foreach (var keyValuePair in dt != DataTypes.NONE ? Cache.Instance.Variables.Where(a => a.Value.DataType == dt).ToList() : Cache.Instance.Variables.ToList())
            {
                if (keyValuePair.Value is Variable)
                {
                    var kvpAsVar = (Variable)keyValuePair.Value;
                    sb.Append(IsNullOrEmpty(kvpAsVar.Value)
                        ? $"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=undefined\n"
                        : $"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}={kvpAsVar.Value}\n");
                }
                else if (keyValuePair.Value is Variables.Array)
                {
                    var kvpAsArray = (Variables.Array)keyValuePair.Value;
                    sb.Append(kvpAsArray.Values.Count == 0
                        ? $"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=undefined\n"
                        : $"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}={kvpAsArray.Values.Select(a => $"\n    [{a.Key}]:{a.Value}").Aggregate(Empty, (current, s) => current + s)}\n");
                }
                else if (keyValuePair.Value is Library)
                {
                    sb.Append($"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=Library\n");
                }
                else
                {
                    sb.Append($"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=Object\n");
                }
            }

            if (sb.Length >= 1)
            {
                sb.Length -= 1;
            }

            return sb.ToString();
        }

        #endregion

        #region Libraries

        public string IncludeLib(string lineToInterprete, string access)
        {
            var group = RegexCollection.Store.With.Match(lineToInterprete).Groups.OfType<Group>().ToList();

            var exist = Exists(group[2].Value, access);
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
                        Access = AccessTypes.REACHABLE_ALL,
                        DataType = DataTypes.NONE,
                        LibObject = Activator.CreateInstanceFrom(path,$"{group[1]}.Library")
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

        public (bool Success, string Result) ConcatString(string[] args)
        {
            var result = Empty;

            foreach (var s in args)
            {
                if (s != "+")
                {
                    if (s.EqualsFromList(Cache.Instance.CharList))
                    {
                        throw new Exception($"Invalid concat: {Join("",args)}!");
                    }
                    result += s.TrimStart('\'').TrimEnd('\'');
                }
            }

            return (true,$"'{result}'");
        }

        public (bool Success, string Result) EvaluateCalculation(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file)
        {
            if (lineToInterprete.Trim().StartsWith("'") && lineToInterprete.Trim().EndsWith("'"))
            {
                if (RegexCollection.Store.IsWord.Matches(lineToInterprete).Count == 1)
                {
                    return (false, Empty);
                }
            }

            try
            {
                lineToInterprete = ReplaceWithVars(lineToInterprete, access, interpreter, file);
            }
            catch (Exception e)
            {
                if (!lineToInterprete.Contains("->"))
                {
                    throw e;
                }
            }

            lineToInterprete = lineToInterprete.Replace("->", "~");
            lineToInterprete = Cache.Instance.CharList.Aggregate(lineToInterprete, (current, s) => current.Replace(s, $" {s} "));

            if (lineToInterprete.ContainsFromList(new List<string> { ":", "~" }))
            {
                var split = lineToInterprete.StringSplit(' ', new[] { '\'', '[', ']' ,'(',')'}).ToArray();

                lineToInterprete = Empty;

                foreach (var t in split)
                {
                    var repl = t.Replace("~", "->");
                    if (RegexCollection.Store.Function.IsMatch(repl) || RegexCollection.Store.MethodCall.IsMatch(repl) || RegexCollection.Store.VarCall.IsMatch(repl) || t.StartsWith("$"))
                    {
                        var output = interpreter.Output;
                        var eOutput = interpreter.ExplicitOutput;
                        interpreter.Output = new NoOutput();
                        interpreter.ExplicitOutput = new NoOutput();
                        lineToInterprete += interpreter.InterpretLine(repl, access, file);
                        interpreter.Output = output;
                        interpreter.ExplicitOutput = eOutput;
                    }
                    else
                    {
                        lineToInterprete += $" {t} ";
                    }
                }
            }

            var args = lineToInterprete.Trim().StringSplit(' ').ToArray();

            if (args.Any(a => a == "+"))
            {
                if (args.Any(a => RegexCollection.Store.IsWord.IsMatch(a)))
                {
                    return ConcatString(args);
                }
            }

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
                if (RegexCollection.Store.IsWord.IsMatch(args[i]))
                {
                    args[i] = args[i].GetIntValue();
                }
            }

            var expression = new StringBuilder();

            foreach (var s in args)
            {
                expression.Append(s + " ");
            }

            expression.Length -= 1;
            var expressionAsString = expression.ToString();

            if (Cache.Instance.CacheCalculation)
            {
                if (Cache.Instance.CachedCalculations.ContainsKey(expressionAsString))
                {
                    return (true, Cache.Instance.CachedCalculations[expressionAsString]);
                }
            }

            var result = Empty;
            var ex = new Expression(expressionAsString);

            var calc = ex.calculate();
            if (isBool)
            {
                try
                {
                    if ((int)calc == 1)
                    {
                        result = "true";
                    }
                    else if ((int)calc == 0)
                    {
                        result = "false";
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

            if (Cache.Instance.CacheCalculation)
            {
                Cache.Instance.CachedCalculations.Add(expressionAsString, result);
            }

            return (true, result);
        }

        #endregion

        #region System functions

        public void Exit(string lineToInterprete)
        {
            Environment.Exit(int.Parse(RegexCollection.Store.Exit.Match(lineToInterprete).Groups[1].Value));
        }

        public (string Value, string Message) Input(string lineToInterprete, string access, IScriptOutput output, Interpreter interpreter, FileInterpreter file)
        {
            var varname = RegexCollection.Store.Input.Match(lineToInterprete).Groups[1].Value;
            var input = output.ReadLine();
            try
            {
                return (input, AssignToVariable($"{varname} = '{input}'", access, false, interpreter, file));
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string EvaluateOut(string lineToInterprete, string access, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.Output.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

            var output = interpreter.Output;
            interpreter.Output = new NoOutput();

            string result;
            try
            {
                result = interpreter.InterpretLine(IsNullOrEmpty(groups[1].Value) ? groups[2].Value : groups[1].Value, access, file);
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

        public string CallMethod(string lineToInterprete, string access,Interpreter interpreter)
        {
            var groups = RegexCollection.Store.MethodCall.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();

            if (Exists(groups[1],access).Exists)
            {
                var variable = GetVariable(groups[1], access);

                if (variable is Library)
                {
                    var methodGroups = RegexCollection.Store.Function.Match(groups[2]).Groups.OfType<Group>().Select(a => a.Value).ToArray();

                    MethodInfo mi = (variable as Library).LibObject.Unwrap().GetType().GetMethod(methodGroups[1]);

                    var args = methodGroups[2].StringSplit(',', new[] {'\'', '[', ']', '(', ')','{','}'}).ToArray();

                    for (var i = 0; i < args.Length; i++)
                    {
                        args[i] = interpreter.InterpretLine(args[i], access, null);
                    }

                    return mi.Invoke((variable as Library).LibObject.Unwrap(), args).ToString();
                }

                if (variable is FileInterpreter)
                {
                    return (variable as FileInterpreter).CallFunction(groups[2], interpreter);
                }
                throw new Exception($"Variable {groups[1]} is not of type object!");
            }

            throw new Exception($"Object {groups[1]} does not exist!");
        }

        public class AliasManager
        {
            public static void Register(string alias)
            {
                if (RegexCollection.Store.Alias.IsMatch(alias))
                {
                    try
                    {
                        var groups = RegexCollection.Store.Alias.Match(alias).Groups.OfType<Group>()
                            .Select(a => a.Value).ToList();

                        if (Cache.Instance.Alias.ContainsKey(groups[2]))
                        {
                            return;
                        }
                        Cache.Instance.Alias.Add(groups[2], groups[1].TrimEnd(' '));
                    }
                    catch (Exception e)
                    {
                        throw e;
                    }

                    var aliasList = Cache.Instance.Alias.ToList();
                    aliasList.Sort((firstPair, nextPair) => nextPair.Value.Length.CompareTo(firstPair.Value.Length));
                    Cache.Instance.Alias = aliasList.ToDictionary(a => a.Key, a => a.Value);
                }
            }

            public static string AliasReplace(string input)
            {
                return Cache.Instance.Alias.Count > 0 ? Cache.Instance.Alias.Aggregate(input, (current, keyValuePair) => current.Replace(keyValuePair.Value, keyValuePair.Key)) : input;
            }
        }
    }
}