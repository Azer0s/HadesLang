﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Interpreter;
using Output;
using StringExtension;

namespace HadesWeb.Server
{
    public class ViewEngine
    {
        public static (byte[],int) RenderFile(string viewFile, Interpreter.Interpreter interpreter, string controllerBasePath = "wwwroot/controller/{0}.hd", string viewBasePath = "wwwroot/view/{0}.hdhtml")
        {
            var controllerPath = string.Format(controllerBasePath, viewFile);
            var viewPath = string.Format(viewBasePath, viewFile);

            //Only controller
            if (!File.Exists(viewPath) && File.Exists(controllerPath))
            {
                var woutput = new WebOutput();
                interpreter.SetOutput(woutput, woutput);
                var code = interpreter.InterpretLine($"with '{controllerPath}'", new List<string> {"web"}, null);

                return (Encoding.UTF8.GetBytes(woutput.Output.ToString()),int.Parse(code));
            }

            //Render hdhtml
            if (File.Exists(viewPath) && File.Exists(controllerPath))
            {
                interpreter.SetOutput(new NoOutput(), new NoOutput());
                var fi = new FileInterpreter(controllerPath, -1,interpreter);
                fi.Execute(interpreter, new List<string> {"web"});
                var view = File.ReadAllLines(viewPath).ToList();

                //Imports
                while (view.Any(a => RegexCollection.Store.Import.IsMatch(a)))
                {
                    var directives = view.Where(a => RegexCollection.Store.Import.IsMatch(a)).Select(a => a).ToList();

                    foreach (var directive in directives)
                    {
                        if (RegexCollection.Store.Import.IsMatch(directive))
                        {
                            var importPath = RegexCollection.Store.Import.Match(directive).Groups[1].Value;
                            List<string> importedLines;
                            try
                            {
                                importedLines = File.ReadLines(importPath).ToList();
                            }
                            catch (Exception)
                            {
                                return (Encoding.UTF8.GetBytes($"File {importPath} could not be read!"), 500);
                            }

                            if (importedLines.Count > 0)
                            {
                                view.InsertRange(view.IndexOf(directive), importedLines);
                            }
                            view.Remove(directive);
                        }
                        else
                        {
                            return (Encoding.UTF8.GetBytes($"Invalid import directive {directive}"), 500);
                        }
                    }
                }

                //Import load
                while (view.Any(a => RegexCollection.Store.ImportLoad.IsMatch(a)))
                {
                    var directives = view.Where(a => RegexCollection.Store.ImportLoad.IsMatch(a)).Select(a => a)
                        .ToList();

                    foreach (var directive in directives)
                    {
                        if (RegexCollection.Store.ImportLoad.IsMatch(directive))
                        {
                            var importPath = RegexCollection.Store.ImportLoad.Match(directive).Groups[1].Value;
                            List<string> importedLines;
                            try
                            {
                                importedLines = new HttpClient()
                                    .GetAsync(importPath).Result
                                    .Content.ReadAsStringAsync()
                                    .Result.Split('\n').ToList();
                            }
                            catch (Exception)
                            {
                                return (Encoding.UTF8.GetBytes($"URL {importPath} could not be opened!"), 500);
                            }

                            if (importedLines.Count > 0)
                            {
                                view.InsertRange(view.IndexOf(directive), importedLines);
                            }

                            view.Remove(directive);
                        }
                        else
                        {
                            return (Encoding.UTF8.GetBytes($"Invalid import-load directive {directive}"), 500);
                        }
                    }
                }

                for (var i = 0; i < view.Count; i++)
                {
                    view[i] = RegexCollection.Store.IgnoreTabsAndSpaces.Match(view[i]).Groups[1].Value;
                }

                for (var i = 0; i < view.Count; i++)
                {
                    //For-loops
                    if (RegexCollection.Store.ViewFor.IsMatch(view[i]))
                    {
                        var end = GetEnd(RegexCollection.Store.ViewEndFor, view, i);

                        if (end != -1)
                        {
                            var forloopexecution = view.Skip(i + 1).Take(end - i - 1);
                            var groups = RegexCollection.Store.ViewFor.Match(view[i]).Groups.Select(a => a.Value)
                                .ToList();
                            var temporaryDt = groups[1];
                            var temporaryVar = groups[2];
                            var arrayVar = groups[3];

                            var guid = Guid.NewGuid().ToString();
                            var scopes = new List<string> { guid, fi.FAccess };

                            interpreter.Evaluator.CreateVariable($"{temporaryVar} as {temporaryDt} closed",
                                scopes, interpreter, fi);

                            var array = RegexCollection.Store.ArrayValues.IsMatch(arrayVar)
                                ? arrayVar
                                : interpreter.InterpretLine(arrayVar, scopes, fi);
                            array = array.TrimStart('{').TrimEnd('}');

                            var newLines = new List<string>();

                            foreach (var iterator in array.StringSplit(',').ToList())
                            {
                                interpreter.Evaluator.AssignToVariable($"{temporaryVar} = {iterator}",
                                    scopes, false, interpreter, fi);
                                newLines.AddRange(forloopexecution.Select(s => ReplaceVariables(s, interpreter, fi, scopes)));                            
                            }

                            view.RemoveRange(i,end - i + 1);
                            view.InsertRange(i,newLines);
                            i = i + newLines.Count;

                            interpreter.Evaluator.Unload("all", new List<string> { guid });
                            continue;
                        }

                        return (Encoding.UTF8.GetBytes($"Invalid for loop (Line: {i})!"), 500);
                    }

                    //If condition
                    if (RegexCollection.Store.ViewIf.IsMatch(view[i]))
                    {
                        var end = GetEnd(RegexCollection.Store.ViewEndIf, view, i);
                        var guid = Guid.NewGuid().ToString();
                        var scopes = new List<string> { guid, fi.FAccess };

                        if (end != -1)
                        {
                            var ifexecution = view.Skip(i + 1).Take(end - i - 1);
                            var groups = RegexCollection.Store.ViewIf.Match(view[i]).Groups.Select(a => a.Value)
                                .ToList();
                            var isIf = groups[1] == "if";
                            var condition = groups[2];
                            var newLines = new List<string>();

                            if (bool.TryParse(interpreter.InterpretLine(condition,scopes, null), out var result))
                            {
                                if ((isIf && result) || (!isIf && !result))
                                {
                                    newLines.AddRange(ifexecution.Select(s => ReplaceVariables(s, interpreter, fi, scopes)));
                                }
                            }
                            else
                            {
                                return (Encoding.UTF8.GetBytes($"Condition yields invalid result (Line: {i})!"), 500);
                            }

                            view.RemoveRange(i, end - i + 1);
                            view.InsertRange(i, newLines);
                            i = i + newLines.Count;

                            interpreter.Evaluator.Unload("all", new List<string> { guid });
                            continue;
                        }

                        return (Encoding.UTF8.GetBytes($"Invalid condition (Line: {i})!"), 500);
                    }

                    view[i] = ReplaceVariables(view[i], interpreter, fi, new List<string>{"web"});
                }

                return (Encoding.UTF8.GetBytes(string.Join("",view)), 200);
            }

            return (new byte[]{}, 500);
        }

        private static int GetEnd(Regex endReg, List<string> view, int start)
        {
            var end = -1;
            for (var j = start; j < view.Count; j++)
            {
                if (endReg.IsMatch(view[j]))
                {
                    end = j;
                    goto continueForLoop;
                }
            }

            continueForLoop:;
            return end;
        }

        private static string ReplaceVariables(string line, Interpreter.Interpreter interpreter, FileInterpreter fi, List<string> scopes)
        {
            if (RegexCollection.Store.ViewVariable.IsMatch(line))
            {
                return RegexCollection.Store.ViewVariable.Replace(line,
                    match => interpreter.InterpretLine(match.Groups[1].Value, scopes, fi).Trim('\''));
            }

            return line;
        }
    }
}