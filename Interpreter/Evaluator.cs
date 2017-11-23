using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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

        public string CreateVariable(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.CreateVariable.Match(lineToInterprete).Groups.OfType<Group>().Select(a => a.Value).ToList();

            var exist = Exists(groups[1], scopes);
            if (exist.Exists)
            {
                return exist.Message;
            }

            var dt = TypeParser.ParseDataType(groups[2]);
            var variable = dt == DataTypes.OBJECT
                ? (IVariable) new FileInterpreter(Cache.Instance.GetOrder())
                {
                    Access = TypeParser.ParseAccessType(groups[3]),
                    DataType = TypeParser.ParseDataType(groups[2])
                }
                : new Variable(Cache.Instance.GetOrder())
                {
                    Access = TypeParser.ParseAccessType(groups[3]),
                    DataType = TypeParser.ParseDataType(groups[2])
                };

            Cache.Instance.Variables.Add(new Meta { Name = groups[1], Owner = scopes[0] }, variable);

            try
            {
                return !IsNullOrEmpty(groups[4]) ? AssignToVariable($"{groups[1]} = {groups[4]}", scopes, true, interpreter, file) : $"{groups[1]} is undefined";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        public string CreateArray(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.CreateArray.Match(lineToInterprete).Groups.OfType<Group>().Select(a => a.Value).ToList();

            var exist = Exists(groups[1], scopes);
            if (exist.Exists)
            {
                return exist.Message;
            }

            Cache.Instance.Variables.Add(new Meta { Name = groups[1], Owner = scopes[0] },
                groups[3] == "*"
                    ? new Variables.Array(Cache.Instance.GetOrder())
                    {
                        Access = TypeParser.ParseAccessType(groups[4]),
                        DataType = TypeParser.ParseDataType(groups[2]),
                        Values = new Dictionary<int, string>()
                    }
                    : new Variables.Array(Cache.Instance.GetOrder())
                    {
                        Access = TypeParser.ParseAccessType(groups[4]),
                        DataType = TypeParser.ParseDataType(groups[2]),
                        Values = new Dictionary<int, string>(int.Parse(groups[3])),
                        Capacity = int.Parse(groups[3])
                    });

            try
            {
                return !IsNullOrEmpty(groups[5]) ? AssignToArray($"{groups[1]} = {groups[5]}", scopes, interpreter, file) : $"{groups[1]} is undefined";
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private string AssignToArray(string s, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);
            interpreter.MuteOut = true;
            var groups = RegexCollection.Store.Assignment.Match(s).Groups.OfType<Group>().Select(a => a.Value).ToList();
            var result = RegexCollection.Store.ArrayValues.IsMatch(groups[2])
                ? groups[2]
                : interpreter.InterpretLine(groups[2], scopes, file);
            if (!RegexCollection.Store.ArrayValues.IsMatch(result))
            {
                interpreter.SetOutput(output.output, output.eOutput);
                interpreter.MuteOut = false;
                throw new Exception("Invalid array format!");
            }

            var split = RegexCollection.Store.ArrayValues.Match(result).Groups[1].Value.StringSplit(',').ToList();

            for (var sp = 0; sp < split.Count; sp++)
            {
                split[sp] = interpreter.InterpretLine(split[sp].TrimStart(' ').TrimEnd(' '), scopes, file);
            }

            var success = false;
            if (Exists(groups[1], scopes).Exists)
            {
                var variable = GetVariable(groups[1], scopes);
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

                    interpreter.SetOutput(output.output, output.eOutput);
                    interpreter.MuteOut = false;
                    throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                }).ToList();

                try
                {
                    success = SetArray(groups[1],scopes, list.ToDictionary(pair => pair.Key, pair => pair.Value.TrimStart(' ')));
                }
                catch (Exception)
                {
                    interpreter.SetOutput(output.output, output.eOutput);
                    interpreter.MuteOut = false;
                    throw;
                }
            }

            interpreter.SetOutput(output.output, output.eOutput);
            interpreter.MuteOut = false;
            if (success)
            {
                return $"{groups[1]} is {{{Join(",", split)}}}";
            }
            throw new Exception($"{groups[1]} could not be set!");
        }

        public string AssignToArrayAtPos(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);
            interpreter.MuteOut = true;
            var groups = RegexCollection.Store.ArrayAssignment.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();

            var exists = Exists(groups[1], scopes);
            if (exists.Exists)
            {
                var variable = GetVariable(groups[1], scopes);
                var datatypeFromVariable = variable.DataType;
                groups[3] = interpreter.InterpretLine(groups[3], scopes, file);
                var datatypeFromData = DataTypeFromData(groups[3], false);
                int position;
                try
                {
                    position = int.Parse(interpreter.InterpretLine(groups[2], scopes, file));
                }
                catch (Exception e)
                {
                    interpreter.SetOutput(output.output, output.eOutput);
                    interpreter.MuteOut = false;

                    throw e;
                }

                if (datatypeFromVariable == DataTypes.WORD)
                {
                    try
                    {
                        SetArrayAtPos(groups[1], scopes, $"'{groups[3].TrimStart('\'').TrimEnd('\'')}'", position);
                    }
                    catch (Exception e)
                    {
                        interpreter.SetOutput(output.output, output.eOutput);
                        interpreter.MuteOut = false;

                        throw e;
                    }
                }
                else if (datatypeFromData == DataTypes.NUM && datatypeFromVariable == DataTypes.DEC)
                {
                    try
                    {
                        SetArrayAtPos(groups[1], scopes, $"{groups[3].Replace(" ", "")}.0", position);
                    }
                    catch (Exception e)
                    {
                        interpreter.SetOutput(output.output, output.eOutput);
                        interpreter.MuteOut = false;

                        throw e;
                    }
                }
                else if (datatypeFromData == datatypeFromVariable)
                {
                    try
                    {
                        SetArrayAtPos(groups[1], scopes, groups[3], position);
                    }
                    catch (Exception e)
                    {
                        interpreter.SetOutput(output.output, output.eOutput);
                        interpreter.MuteOut = false;

                        throw e;
                    }
                }
                else
                {
                    interpreter.SetOutput(output.output, output.eOutput);
                    interpreter.MuteOut = false;

                    throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                }

                interpreter.SetOutput(output.output, output.eOutput);
                interpreter.MuteOut = false;

                return $"{groups[1]}[{position}] is {groups[3]}";
            }

            interpreter.SetOutput(output.output, output.eOutput);
            interpreter.MuteOut = false;

            throw new Exception($"{exists.Message}");
        }

        private void SetArrayAtPos(string name, List<string> scopes, string value, int position)
        {
            Variables.Array variable;
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.GLOBAL))
            {
                var reachableAllVar =
                    Cache.Instance.Variables.First(a => a.Key.Name == name &&
                                                        a.Value.Access == AccessTypes.GLOBAL);
                variable = Cache.Instance.Variables[reachableAllVar.Key] as Variables.Array;
            }
            else
            {
                var owner = Empty;

                foreach (var scope in scopes)
                {
                    owner = Cache.Instance.Variables.FirstOrDefault(a =>
                        a.Key.Name == name && a.Key.Owner == scope).Key.Owner;

                    if (!IsNullOrEmpty(owner))
                    {
                        break;
                    }
                }
                
                if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Key.Owner == owner))
                {
                    variable = Cache.Instance.Variables[new Meta { Name = name, Owner = owner }] as Variables.Array;
                }
                else
                {
                    throw new Exception($"Variable {name} does not exist or the access was denied!");

                }
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

        private bool SetArray(string name, List<string> scopes, Dictionary<int, string> values)
        {
            Variables.Array variable = null;
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.GLOBAL))
            {
                var reachableAllVar = Cache.Instance.Variables.First(a => a.Key.Name == name && a.Value.Access == AccessTypes.GLOBAL);
                variable = Cache.Instance.Variables[reachableAllVar.Key] as Variables.Array;
            }
            var owner = Empty;

            foreach (var scope in scopes)
            {
                owner = Cache.Instance.Variables.FirstOrDefault(a =>
                    a.Key.Name == name && a.Key.Owner == scope).Key.Owner;

                if (!IsNullOrEmpty(owner))
                {
                    break;
                }
            }
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Key.Owner == owner))
            {
                variable = Cache.Instance.Variables[new Meta { Name = name, Owner = owner }] as Variables.Array;
            }

            if (variable != null)
            {
                if (values.Count <= variable.Capacity)
                {
                    variable.Values = values;
                    return true;
                }
                throw new Exception("Array is bigger than capacity!");
            }
            throw new Exception($"Variable {name} does not exist or the access was denied!");
        }

        public (bool Exists, string Message) Exists(string name, List<string> scopes)
        {
            if (VariableIsReachableAll(name))
            {
                return (true, "Variable already defined as global!");
            }

            var owner = Empty;

            foreach (var scope in scopes)
            {
                owner = Cache.Instance.Variables.FirstOrDefault(a =>
                    a.Key.Name == name && a.Key.Owner == scope).Key.Owner;

                if (!IsNullOrEmpty(owner))
                {
                    break;
                }
            }

            if (Cache.Instance.Variables.ContainsKey(new Meta { Name = name, Owner = owner}))
            {
                return (true, "Variable already exists!");
            }

            return (false, $"Variable {name} does not exist or the access was denied!");
        }

        private bool VariableIsReachableAll(string varname)
        {
            return Cache.Instance.Variables.Any(a => a.Key.Name == varname && a.Value.Access == AccessTypes.GLOBAL);
        }

        public string ReplaceWithVars(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var matches = RegexCollection.Store.Variable.Matches(lineToInterprete);

            var stringDict = new Dictionary<string, string>();

            foreach (Match variable in RegexCollection.Store.IsWord.Matches(lineToInterprete))
            {
                var guid = Guid.NewGuid().ToString().ToLower();
                lineToInterprete = lineToInterprete.Replace(variable.Value, guid);
                stringDict.Add(guid, variable.Value);
            }

            foreach (Match match in matches)
            {
                var variable = GetVariable(match.Groups[1].Value.TrimStart('$'), scopes);

                if (variable is Variable)
                {
                    lineToInterprete = lineToInterprete.Replace(match.Groups[1].Value, (variable as Variable).Value);
                }
                else if (variable is Variables.Array)
                {
                    if (RegexCollection.Store.ArrayVariable.IsMatch(match.Value))
                    {
                        lineToInterprete = lineToInterprete.Replace(match.Value, GetArrayValue(match.Value, scopes, interpreter, file));
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
            return stringDict.Aggregate(lineToInterprete, (current, variable) => current.Replace(variable.Key, variable.Value)); ;
        }

        public string GetArrayValue(string name, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.ArrayVariable.Match(name).Groups;
            var variable = GetVariable(groups[1].Value.TrimStart('$'), scopes);

            if (variable is Variables.Array)
            {
                var output = interpreter.GetOutput();
                var index = -1;
                try
                {
                    interpreter.SetOutput(new NoOutput(), output.output);
                    index = int.Parse(interpreter.InterpretLine(groups[2].Value, scopes, file));
                    var value = (variable as Variables.Array).Values[index];
                    interpreter.SetOutput(output.output, output.eOutput);

                    return value;
                }
                catch (Exception)
                {
                    interpreter.SetOutput(output.output, output.eOutput);
                    throw new Exception($"Index {index} in array {groups[1].Value} is out of bounds or does not exist!");
                }
            }

            throw new Exception($"Could not get value of idex {groups[2]} in array {name}!");
        }

        public string InDeCrease(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.InDeCrease.Match(lineToInterprete).Groups.OfType<Group>().Select(a => a.Value).ToArray();
            lineToInterprete = $"{groups[1]} = ${groups[1]} {groups[2]} 1";
            return interpreter.InterpretLine(lineToInterprete, scopes, file);
        }

        public IVariable GetVariable(string variable, List<string> scopes)
        {
            if (VariableIsReachableAll(variable))
            {
                return Cache.Instance.Variables.First(a => a.Key.Name == variable).Value;
            }
            if (Exists(variable, scopes).Exists)
            {
                foreach (var scope in scopes)
                {
                    try
                    {
                        if (Cache.Instance.Variables.Any(a => a.Key.Owner == scope && a.Key.Name == variable))
                        {
                            return Cache.Instance.Variables[new Meta { Name = variable, Owner = scope }];
                        }
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }
            }
            throw new Exception($"Variable {variable} does not exist or the access was denied!");
        }

        public string AssignToVariable(string lineToInterprete, List<string> scopes, bool hardCompare, Interpreter interpreter, FileInterpreter file)
        {
            var groups = RegexCollection.Store.Assignment.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

            if (RegexCollection.Store.ArrayValues.IsMatch(groups[2].Value))
            {
                return AssignToArray(lineToInterprete, scopes, interpreter, file);
            }

            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);
            interpreter.MuteOut = true;
            var result = interpreter.InterpretLine(groups[2].Value, scopes, file);
            interpreter.MuteOut = false;
            interpreter.SetOutput(output.output, output.eOutput);

            var success = false;

            if (Exists(groups[1].Value, scopes).Exists)
            {
                var variable = GetVariable(groups[1].Value, scopes);

                if (variable is Variable)
                {
                    var datatypeFromVariable = variable.DataType;
                    var datatypeFromData = DataTypeFromData(result, hardCompare);

                    if (datatypeFromVariable == DataTypes.ANY)
                    {
                        if (RegexCollection.Store.IsPureWord.IsMatch(groups[2].Value))
                        {
                            datatypeFromData = DataTypes.WORD;
                            result = $"'{result.TrimStart('\'').TrimEnd('\'')}'";
                        }

                        if (datatypeFromData != DataTypes.NONE)
                        {
                            variable.DataType = datatypeFromData;
                            datatypeFromData = datatypeFromVariable;
                        }
                    }

                    if (datatypeFromVariable == DataTypes.WORD)
                    {
                        result = $"'{result.TrimStart('\'').TrimEnd('\'')}'";
                        success = SetVariable(groups[1].Value, result, scopes);
                    }
                    else if (datatypeFromData == DataTypes.NUM && datatypeFromVariable == DataTypes.DEC)
                    {
                        result = $"{result.Replace(" ", "")}.0";
                        success = SetVariable(groups[1].Value, result, scopes);
                    }
                    else if (datatypeFromData == datatypeFromVariable)
                    {
                        success = SetVariable(groups[1].Value, result, scopes);
                    }
                    else
                    {
                        throw new Exception($"Can't assign value of type {datatypeFromData} to variable of type {datatypeFromVariable}!");
                    }
                }
                else if (variable is FileInterpreter)
                {
                    success = SetVariable(groups[1].Value, result, scopes);
                    result = "object";
                }
                else if (variable is Variables.Array)
                {
                    return AssignToArray(lineToInterprete,scopes, interpreter, file);
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

        private bool SetVariable(string name, string value, List<string> scopes)
        {
            IVariable variable = null;
            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Value.Access == AccessTypes.GLOBAL))
            {
                var reachableAllVar = Cache.Instance.Variables.First(a => a.Key.Name == name && a.Value.Access == AccessTypes.GLOBAL);

                if (reachableAllVar.Value is FileInterpreter interpreter)
                {
                    variable = interpreter;
                }
                else
                {
                    variable = Cache.Instance.Variables[reachableAllVar.Key] as Variable;
                }
            }

            var owner = Empty;

            foreach (var scope in scopes)
            {
                owner = Cache.Instance.Variables.FirstOrDefault(a =>
                    a.Key.Name == name && a.Key.Owner == scope).Key.Owner;

                if (!IsNullOrEmpty(owner))
                {
                    break;
                }
            }

            if (Cache.Instance.Variables.Any(a => a.Key.Name == name && a.Key.Owner == owner))
            {
                var varFromCache = Cache.Instance.Variables[new Meta {Name = name, Owner = owner}];

                if (varFromCache is Variable)
                {
                    variable = varFromCache as Variable;
                }
                else if(varFromCache is FileInterpreter)
                {
                    variable = varFromCache as FileInterpreter;
                }
            }

            if (variable != null)
            {
                // ReSharper disable once MergeCastWithTypeCheck
                if (variable is Variable)
                {
                    ((Variable)variable).Value = value;
                    // ReSharper disable once UseNullPropagation
                }else if (variable is FileInterpreter)
                {
                    var key = value.Replace("obj", "");
                    ((FileInterpreter) variable).Set(Cache.Instance.FileCache[key] as FileInterpreter);
                    Cache.Instance.FileCache.Remove(key);
                }
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
            if (result.StartsWith("obj"))
            {
                return DataTypes.OBJECT;
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

        public string LoadAs(string path, string varName, List<string> scopes, Interpreter interpreter, Dictionary<string, string> construct)
        {
            var fileInterpreter = new FileInterpreter(path,Cache.Instance.GetOrder());
            fileInterpreter.Construct(interpreter,construct);
            var result = Exists(varName, scopes);
            if (!result.Exists)
            {
                Cache.Instance.Variables.Add(new Meta { Name = varName, Owner = scopes[0] }, fileInterpreter);
            }
            else if (Cache.Instance.Variables.Any(a => a.Value is FileInterpreter && a.Key.Name == varName))
            {
                var guid = Guid.NewGuid().ToString();
                Cache.Instance.FileCache.Add(guid,fileInterpreter);
                AssignToVariable($"{varName} = obj{guid}", scopes, true, interpreter, null);
            }
            else
            {
                throw new Exception(result.Message);
            }

            return fileInterpreter.Execute(interpreter, new List<string>{path}).Value;
        }

        public string LoadFile(string path, string varname, List<string> scopes, Interpreter interpreter, string constructorinfo)
        {
            var construct = new Dictionary<string,string>();
            if (constructorinfo != Empty)
            {
                var stringDict = new Dictionary<string, string>();

                foreach (Match variable in RegexCollection.Store.IsWord.Matches(constructorinfo))
                {
                    var guid = Guid.NewGuid().ToString().ToLower();
                    constructorinfo = constructorinfo.Replace(variable.Value, guid);
                    stringDict.Add(guid, variable.Value);
                }

                var assignments = constructorinfo.Split(';').Select(a => a.Trim());

                foreach (var assignment in assignments)
                {
                    if (RegexCollection.Store.Assignment.IsMatch(assignment))
                    {
                        var groups = RegexCollection.Store.Assignment.Match(assignment).Groups.OfType<Group>().Skip(1)
                            .Select(a => a.Value).ToArray();

                        var output = interpreter.GetOutput();
                        interpreter.SetOutput(new NoOutput(), output.eOutput);

                        construct.Add(groups[0],
                            RegexCollection.Store.GUID.IsMatch(groups[1])
                                ? stringDict.Aggregate(groups[1],
                                    (current, variable) => current.Replace(variable.Key, variable.Value))
                                : interpreter.InterpretLine(
                                    stringDict.Aggregate(groups[1],
                                        (current, variable) => current.Replace(variable.Key, variable.Value)), scopes,
                                    null));

                        interpreter.SetOutput(output.output, output.eOutput);
                    }
                    else
                    {
                        throw new Exception($"Invalid assignment in construction: {assignment}!");
                    }
                }
            }

            if (!IsNullOrEmpty(varname))
            {
                return LoadAs(path, varname, scopes, interpreter,construct);
            }

            var file = new FileInterpreter(path,Cache.Instance.GetOrder());
            file.Construct(interpreter,construct);
            var result = file.Execute(interpreter, new List<string> { path });

            if (Cache.Instance.EraseVars)
            {
                try
                {
                    Unload("all", new List<string>{path});
                }
                catch (Exception e)
                {
                    throw e;
                }
            }

            return result.Value;
        }

        public string Unload(string variable, List<string> scopes)
        {
            if (variable == "all")
            {
                foreach (var keyValuePair in Cache.Instance.Variables.Where(a => a.Key.Owner == scopes[0]).Select(a => a).ToList())
                {
                    try
                    {
                        Unload(keyValuePair.Key.Name, scopes);
                    }
                    catch (Exception)
                    {
                        //ignored
                    }
                }

                return $"Unloaded all variables from {scopes[0]}!";
            }

            var obj = GetVariable(variable, scopes);

            var owner = Empty;

            foreach (var scope in scopes)
            {
                owner = Cache.Instance.Variables.FirstOrDefault(a =>
                    a.Key.Name == variable && a.Key.Owner == scope).Key.Owner;

                if (!IsNullOrEmpty(owner))
                {
                    break;
                }
            }

            if (obj is FileInterpreter && Cache.Instance.EraseVars)
            {
                if (!RegexCollection.Store.GUID.IsMatch(owner))
                {
                    Unload("all", new List<string>{(obj as FileInterpreter).FAccess});
                }
                else
                {
                    Cache.Instance.Variables.Remove(new Meta {Name = variable, Owner = owner});
                    throw new Exception("Can´t unload object from method!");
                }
            }

            if (VariableIsReachableAll(variable))
            {
                Cache.Instance.Variables.Remove(Cache.Instance.Variables.First(a => a.Key.Name == variable).Key);
                return $"Unloaded variable {variable}!";
            }
            if (Exists(variable, scopes).Exists)
            {
                Cache.Instance.Variables.Remove(new Meta { Name = variable, Owner = owner });
                return $"Unloaded variable {variable}!";
            }
            throw new Exception($"Variable {variable} does not exist or the access was denied!");
        }

        public string GetObjectVar(string obj, string varname, List<string> scopes, Interpreter interpreter)
        {
            if (!Exists(obj, scopes).Exists)
            {
                throw new Exception($"Object {obj} does not exist!");
            }

            varname = $"${varname.TrimStart('$')}";
            var varObj = GetVariable(obj,scopes);

            if (varObj is FileInterpreter)
            {
                IVariable fileVariable = null;
                var arrayValue = Empty;
                if (RegexCollection.Store.ArrayVariable.IsMatch(varname))
                {
                    arrayValue = GetArrayValue(varname, new List<string>{(varObj as FileInterpreter).FAccess }, interpreter, varObj as FileInterpreter);
                }
                else
                {
                    fileVariable = GetVariable(varname.TrimStart('$'), new List<string> { (varObj as FileInterpreter).FAccess });
                }

                if (fileVariable != null)
                {
                    if (fileVariable.Access == AccessTypes.CLOSED)
                    {
                        throw new Exception($"The access to variable {varname} was denied!");
                    }

                    if (fileVariable is Variable)
                    {
                        return (fileVariable as Variable).Value;
                    }

                    var output = interpreter.GetOutput();
                    interpreter.SetOutput(new NoOutput(), output.eOutput);
                    var fileInterpreter = varObj as FileInterpreter;
                    var result = interpreter.InterpretLine(varname, new List<string> { fileInterpreter.FAccess }, fileInterpreter);
                    interpreter.SetOutput(output.output, output.eOutput);
                    return result;
                }
                return arrayValue;
            }
            throw new Exception($"Variable {obj} is not of type object!");
        }

        public string DumpVars(DataTypes dt)
        {
            var sb = new StringBuilder();

            foreach (var keyValuePair in dt != DataTypes.NONE ? Cache.Instance.Variables.Where(a => a.Value.DataType == dt).ToList() : Cache.Instance.Variables.ToList())
            {
                // ReSharper disable once MergeCastWithTypeCheck
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
                    sb.Append($"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=library\n");
                }
                else
                {
                    sb.Append($"{keyValuePair.Key.Name}@{keyValuePair.Key.Owner}=object\n");
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

        public string IncludeLib(string lineToInterprete, List<string> scopes, Interpreter interpreter)
        {
            var group = RegexCollection.Store.With.Match(lineToInterprete).Groups.OfType<Group>().Select(a => a.Value).ToList();

            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), new NoOutput());
            var result = interpreter.InterpretLine(group[2], scopes, null).Trim('\'');
            result = IsNullOrEmpty(result) ? group[2] : result;
            interpreter.SetOutput(output.output,output.eOutput);

            var fn = IsNullOrEmpty(group[1]) ? result : group[1];
            var varname = IsNullOrEmpty(group[3]) ? fn : group[3];

            var file = new FileInfo(fn);

            if (!IsNullOrEmpty(file.Extension))
            {
                return LoadFile(fn,group[3], scopes, interpreter,group[4]);
            }

            var exist = Exists(varname, scopes);
            if (exist.Exists)
            {
                throw new Exception(exist.Message);
            }
            var path = $"{Cache.Instance.LibraryLocation}\\{fn}.dll";

            if (!File.Exists(path))
            {
                throw new Exception($"Library {fn} does not exist!");
            }

            try
            {
                Cache.Instance.Variables.Add(new Meta
                {
                    Name = varname,
                    Owner = scopes[0]
                },
                new Library(Cache.Instance.GetOrder())
                {
                    Access = AccessTypes.GLOBAL,
                    DataType = DataTypes.NONE,
                    LibObject = Activator.CreateInstanceFrom(path, $"{fn}.Library")
                });
            }
            catch (Exception)
            {
                throw new Exception($"Error while loading library {fn}!");
            }

            interpreter.Output.WriteLine($"Succesfully loaded library {fn}!");
            return Empty;
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
                        throw new Exception($"Invalid concat: {Join("", args)}!");
                    }
                    result += s.TrimStart('\'').TrimEnd('\'');
                }
            }

            return (true, $"'{result}'");
        }

        public (bool Success, string Result) EvaluateCalculation(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
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
                lineToInterprete = ReplaceWithVars(lineToInterprete, scopes, interpreter, file);
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
                var split = lineToInterprete.StringSplit(' ', new[] { '\'', '[', ']', '(', ')' }).ToArray();

                lineToInterprete = Empty;

                foreach (var t in split)
                {
                    var repl = t.Replace("~", "->");
                    if (RegexCollection.Store.Function.IsMatch(repl) || RegexCollection.Store.MethodCall.IsMatch(repl) || RegexCollection.Store.VarCall.IsMatch(repl) || t.StartsWith("$"))
                    {
                        var output = interpreter.GetOutput();
                        interpreter.SetOutput(new NoOutput(), new NoOutput());
                        lineToInterprete += interpreter.InterpretLine(repl, scopes, file);
                        interpreter.SetOutput(output.output,output.eOutput);
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
                    if (args[i].ToUpper().EqualsFromList(Cache.Instance.ReplacementWithoutBinary.Keys))
                    {
                        isBool = true;
                    }
                    args[i] = Cache.Instance.Replacement[args[i].ToUpper()];
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
                catch (Exception)
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

        public string Raw(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);

            string result;
            try
            {
                result = interpreter.InterpretLine(RegexCollection.Store.Raw.Match(lineToInterprete).Groups[1].Value, scopes, file);
            }
            catch (Exception e)
            {
                interpreter.SetOutput(output.output, output.eOutput);
                throw e;
            }
            interpreter.SetOutput(output.output, output.eOutput);

            return result.Trim('\'');
        }

        public string EvaluateOut(string lineToInterprete, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            lineToInterprete = lineToInterprete.TrimEnd(' ');
            var groups = RegexCollection.Store.Output.Match(lineToInterprete).Groups.OfType<Group>().ToArray();

            var output = interpreter.GetOutput();
            interpreter.SetOutput(new NoOutput(), output.eOutput);

            string result;
            try
            {
                result = interpreter.InterpretLine(IsNullOrEmpty(groups[1].Value) ? groups[2].Value : groups[1].Value, scopes, file);
            }
            catch (Exception e)
            {
                interpreter.SetOutput(output.output, output.eOutput);
                throw e;
            }
            interpreter.SetOutput(output.output,output.eOutput);

            return $"'{result.Replace("\\/","/")}'";
        }

        #endregion

        #region Custom functions

        public string CallCustomFunction(Group[] groups)
        {
            var firstOrDefault = Cache.Instance.Functions.FirstOrDefault(a => a.Name == groups[1].Value);
            return firstOrDefault?.Execute(groups[2].Value.StringSplit(',').Select(a => a.Replace("'", "")));
        }

        #endregion

        public string CallMethod(string lineToInterprete, List<string> scopes, Interpreter interpreter)
        {
            var groups = RegexCollection.Store.MethodCall.Match(lineToInterprete).Groups.OfType<Group>()
                .Select(a => a.Value).ToList();

            if (Exists(groups[1], scopes).Exists)
            {
                var variable = GetVariable(groups[1],scopes);

                if (variable is Library)
                {
                    var methodGroups = RegexCollection.Store.Function.Match(groups[2]).Groups.OfType<Group>().Select(a => a.Value).ToArray();

                    MethodInfo mi = (variable as Library).LibObject.Unwrap().GetType().GetMethod(methodGroups[1]);

                    var args = methodGroups[2].StringSplit(',', new[] { '\'', '[', ']', '(', ')' }).ToArray();

                    for (var i = 0; i < args.Length; i++)
                    {
                        args[i] = interpreter.InterpretLine(args[i], scopes, null);
                    }

                    return mi.Invoke((variable as Library).LibObject.Unwrap(), args);
                }

                if (variable is FileInterpreter)
                {
                    return (variable as FileInterpreter).CallFunction(groups[2], interpreter,scopes);
                }
                throw new Exception($"Variable {groups[1]} is not of type object!");
            }

            throw new Exception($"Object {groups[1]} does not exist!");
        }

        public class AliasManager
        {
            public static void Add(string replacement, string toReplace)
            {
                Cache.Instance.Alias.Add(replacement, toReplace.TrimEnd(' '));
            }

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
                        Add(groups[2],groups[1]);
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
                if (Cache.Instance.Alias.Count > 0)
                {
                    var stringDict = new Dictionary<string, string>();

                    foreach (Match variable in RegexCollection.Store.IsWord.Matches(input))
                    {
                        var guid = Guid.NewGuid().ToString().ToLower();
                        input = input.Replace(variable.Value, guid);
                        stringDict.Add(guid, variable.Value);
                    }

                    var dict = new Dictionary<string, string>();
                    foreach (var keyValuePair in Cache.Instance.Alias)
                    {
                        var guid = Guid.NewGuid().ToString().ToLower();
                        input = input.Replace(keyValuePair.Value, guid);
                        dict.Add(guid, keyValuePair.Key);
                    }

                    input = dict.Aggregate(input, (current, variable) => current.Replace(variable.Key, variable.Value));
                    input = stringDict.Aggregate(input, (current, variable) => current.Replace(variable.Key, variable.Value));
                }
                return input;
            }
        }

        public string VarCallAssign(string obj, string varname, string value, List<string> scopes, Interpreter interpreter, FileInterpreter file)
        {
            if (!Exists(obj, scopes).Exists)
            {
                throw new Exception($"Object {obj} does not exist!");
            }

            varname = varname.TrimStart('$');
            var varObj = GetVariable(obj, scopes);
            var fileAccess = new List<string> {(varObj as FileInterpreter)?.FAccess};

            if (varObj is FileInterpreter)
            {
                IVariable fileVariable;
                var isArray = false;
                var arrayPos = 0;
                var arrayName = Empty;

                if (RegexCollection.Store.ArrayVariable.IsMatch("$" + varname))
                {
                    var groups = RegexCollection.Store.ArrayVariable.Match("$" + varname);
                    arrayPos = int.Parse(groups.Groups[2].Value);
                    arrayName = groups.Groups[1].Value;
                    arrayName = arrayName.Trim('$');
                    fileVariable = GetVariable(arrayName, fileAccess);
                    isArray = true;
                }
                else
                {
                    fileVariable = GetVariable(varname, fileAccess);
                }

                if (fileVariable.Access == AccessTypes.REACHABLE || fileVariable.Access == AccessTypes.GLOBAL)
                {
                    var output = interpreter.GetOutput();
                    interpreter.SetOutput(new NoOutput(), output.eOutput);
                    value = RegexCollection.Store.IsPureWord.IsMatch(value) ? value : interpreter.InterpretLine(value, scopes, file);
                    interpreter.SetOutput(output.output, output.eOutput);

                    if (IsNullOrEmpty(value))
                    {
                        return "Value is empty!";
                    }

                    if (isArray)
                    {
                        SetArrayAtPos(arrayName,fileAccess,value,arrayPos);
                        return $"{arrayName}[{arrayPos}] is {value}";
                    }
                    SetVariable(varname, value, fileAccess);
                    return $"{varname} is {value}";
                }
                throw new Exception($"Variable {varname} does not exist or the access was denied!");
            }
            throw new Exception($"Variable {obj} is not of type object!");
        }

        public string GetFields(List<string> scopes)
        {
            return $"{{'{Join("','", from a in Cache.Instance.Variables orderby a.Value.Order ascending where scopes.Contains(a.Key.Owner) select a.Key.Name)}'}}";
        }
    }
}