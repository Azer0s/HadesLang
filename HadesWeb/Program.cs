using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Colorful;
using Output;
using ServiceStack.Redis;
using StringExtension;
using Variables;
using Interpreter = Interpreter.Interpreter;
using Console = Colorful.Console;
using static HadesWeb.BrowserHelper;
using static HadesWeb.FileHelper;
using static HadesWeb.Log;

namespace HadesWeb
{
    class Program
    {
        private static global::Interpreter.Interpreter _interpreter;
        private static bool _routingEnabled = false;
        private static string _address;
        private static string _port;
        private static bool _browser = false;
        private static string _routingFile;
        private static readonly Dictionary<string,string> Routes = new Dictionary<string, string>();
        private static readonly List<string> Forward = new List<string>();
        
        public static void Main()
        {
            Console.Clear();
            Console.Title = "HadesWeb Server";

            var name = string.Empty;
            foreach (var c in "HadesWeb Server")
            {
                name += c;
                Console.Clear();
                Console.WriteAscii(name,Blue);
                Thread.Sleep(70);
            }

            Info("Initializing Hades Interpreter...");
            _interpreter = new global::Interpreter.Interpreter(new NoOutput(), new NoOutput());

            #region Config

            var lines = File.ReadAllLines("config").ToList();
            _address = lines.Where(a => a.StartsWith("address"))
                .Select(a => a.Replace("address:", "").Replace(" ", "")).First();
            _port = lines.Where(a => a.StartsWith("port"))
                .Select(a => a.Replace("port:", "").Replace(" ", "")).First();

            try
            {
                _log = bool.Parse(lines.Where(a => a.StartsWith("log"))
                    .Select(a => a.Replace("log:", "").Replace(" ", "")).First());
            }
            catch (Exception)
            {
                // ignored
            }

            if (_log)
            {
                var redis = lines.Where(a => a.StartsWith("port"))
                    .Select(a => a.Replace("port:", "").Replace(" ", "")).First();

                try
                {
                    Info("Trying to connect to redis...");
                    Manager = new RedisManagerPool(redis);
                    Manager.GetClient();
                    Success("Connection to redis successful!");
                }
                catch (Exception)
                {
                    _log = false;
                    Error("Couldn't connect to redis!");
                }
            }

            try
            {
                _browser = bool.Parse(lines.Where(a => a.StartsWith("startBrowser"))
                    .Select(a => a.Replace("startBrowser:", "").Replace(" ", "")).First());
            }
            catch (Exception)
            {
                // ignored
            }
            _routingFile = lines.Where(a => a.StartsWith("routing"))
                .Select(a => a.Replace("routing:", "").Replace(" ", "")).FirstOrDefault();

            #endregion       

            #region Routing

            _routingEnabled = false;
                        
            //Function endpoint for HadesInterpreter
            _interpreter.RegisterFunction(new Function("route", a =>
            {
                var parameters = a as object[] ?? a.ToArray();

                if (parameters.Length != 2)
                {
                    return "false";
                }
                
                var route = parameters[0].ToString();
                var action = parameters[1].ToString();

                try
                {
                    if (route.EndsWith("/*"))
                    {
                        if (action.EndsWith("/*"))
                        {
                            route = route.Replace("/*", "/(.+)");
                            Info($"Added wildcard rout {route.Replace("/(.+)", "/*")} with action {action}!");
                        }
                        else
                        {
                            Error($"Can't add non wildcarded action ({action}) for wildcarded route ({route})!");
                            return "false";
                        }
                    }
                    else
                    {
                        Info($"Added route {route} with action {action}!");
                    }
                    Routes.Add(route, action);
                }
                catch (Exception)
                {
                    Error($"Error while adding route {route}!");
                    return "false";
                }
                
                return "true";
            }));

            _interpreter.RegisterFunction(new Function("forward", a =>
            {
                var parameters = a as object[] ?? a.ToArray();

                if (parameters.Length != 1)
                {
                    return "false";
                }

                var fileType = parameters[0].ToString();

                try
                {
                    Forward.Add(fileType);
                    Info($"Added forward route to filetype {fileType}!");
                }
                catch (Exception)
                {
                    return "false";
                }

                return "true";
            }));
            
            //Check if route config exists
            if (!string.IsNullOrEmpty(_routingFile) && File.Exists(_routingFile))
            {
                _interpreter.InterpretLine($"with '{_routingFile}'", new List<string> {"web"}, null);
            }

            if (Routes.Count != 0)
            {
                _routingEnabled = true;
            }
            else
            {
                Info("No routing file given - switching to autorouting!");
            }

            #endregion
            
            var listener = new HttpListener();
            listener.Prefixes.Add($"http://{_address}:{_port}/");
            Info($"Listening @ {_address} on port {_port}");
            listener.Start();

            if (_browser)
            {
                OpenUrl($"http://{_address}:{_port}/");
            }

            while (true)
            {
                var context = listener.GetContext();
                var request = context.Request;
                var response = context.Response;
                var returnBytes = new byte[] { };

                Info($"Request - {request.RawUrl}");

                if (_routingEnabled)
                {
                    var action = string.Empty;
                    var basepath = "wwwroot/controller/{0}.hd";

                    if (request.RawUrl.EndsWithFromList(Forward))
                    {
                        returnBytes = GetFile(request.RawUrl);
                    }
                    else
                    {
                        if (Routes.ContainsKey(request.RawUrl))
                        {
                            action = Routes[request.RawUrl];
                        }
                        else if (Routes.ContainsKey(request.RawUrl.Replace(".hd","")))
                        {
                            action = Routes[request.RawUrl.Replace(".hd", "")];
                        }
                        else if (Routes.Where(a => a.Key.EndsWith("/(.+)") && a.Key != "/").Select(a => a.Key).Any(a => Regex.IsMatch(request.RawUrl,a)))
                        {
                            var pair = Routes.Where(a => Regex.IsMatch(request.RawUrl, a.Key) && a.Key != "/").Select(a => a)
                                .First();
                            action = Regex.Replace(request.RawUrl, pair.Key,
                                a => pair.Value.Replace("/*", $"/{a.Groups[1].Value}"));
                            basepath = "wwwroot{0}.hd";
                        }
                        else
                        {
                            Error($"No route specified for action {request.RawUrl}!");
                        }

                        if (string.IsNullOrEmpty(action))
                        {
                            response.StatusCode = 500;
                        }
                        else
                        {
                            returnBytes = RegexCollection.Store.HasExtension.IsMatch(action)
                                ? GetFile(action)
                                : InterpretFile(action, response,_interpreter,basepath);
                        }
                    }       
                }
                //Autoroute
                else
                {
                    if (request.RawUrl == "/" || request.RawUrl == "/#")
                    {
                        returnBytes = InterpretFile("index", response,_interpreter);
                    }
                    else
                    {
                        returnBytes =
                            request.RawUrl.EndsWith(".hd") || File.Exists($"wwwroot/controller{request.RawUrl}.hd")
                                ? InterpretFile(request.RawUrl.TrimStart('/').Replace(".hd", ""), response,_interpreter)
                                : GetFile(request.RawUrl);
                    }
                }

                response.ContentLength64 = returnBytes.Length;
                var output = response.OutputStream;
                output.Write(returnBytes, 0, returnBytes.Length);
                output.Close();
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}
