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
        private static RedisManagerPool _manager;
        private static bool _log = false;
        
        #region Log

        private static readonly Color Blue = Color.FromArgb(240, 6, 153);
        private static readonly Color Yellow = Color.FromArgb(247, 208, 2);
        private static readonly Color Purple = Color.FromArgb(69, 78, 158);
        private static readonly Color Red = Color.FromArgb(191, 26, 47);
        private static readonly Color Green = Color.FromArgb(1, 142, 66);

        private static string Time()
        {
            var now = $"[{DateTime.UtcNow:o}]";
            Console.Write(now, Yellow);
            return now;
        }

        private static void Error(string message)
        {
            var now = Time();

            if (_log)
            {
                using (var client = _manager.GetClient())
                {
                    client.Set(now, message);
                }
            }

            Console.WriteLine($" {message}", Red);
        }

        private static void Info(string message)
        {
            Time();
            Console.WriteLine($" {message}", Purple);
        }

        private static void Success(string message)
        {
            Time();
            Console.WriteLine($" {message}",Green);
        }


        #endregion
        
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
                    _manager = new RedisManagerPool(redis);
                    _manager.GetClient();
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
                    Routes.Add(route, action);
                    Info($"Added route {route} with action {action}!");
                }
                catch (Exception)
                {
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

                    if (request.RawUrl.EndsWithFromList(Forward))
                    {
                        returnBytes = GetFile(request);
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

                        if (string.IsNullOrEmpty(action))
                        {
                            response.StatusCode = 500;
                        }
                        else
                        {   
                            returnBytes = InterpretFile(action, response);
                        }
                    }       
                }
                //Autoroute
                else
                {
                    if (request.RawUrl == "/" || request.RawUrl == "/#")
                    {
                        returnBytes = InterpretFile("index", response);
                    }
                    else
                    {
                        returnBytes = request.RawUrl.EndsWith(".hd") ? InterpretFile(request.RawUrl.TrimStart('/').Replace(".hd",""), response) : GetFile(request);
                    }
                }

                response.ContentLength64 = returnBytes.Length;
                var output = response.OutputStream;
                output.Write(returnBytes, 0, returnBytes.Length);
                output.Close();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public static byte[] GetFile(HttpListenerRequest request)
        {
            var returnBytes = new byte[]{};
            try
            {
                returnBytes = File.ReadAllBytes($"wwwroot{request.RawUrl}");
                Success("Handled request successfully");
            }
            catch (Exception e)
            {
                Error($"Error while handling request - file wwwroot{request.RawUrl} does not exist!");
            }

            return returnBytes;
        }

        public static byte[] InterpretFile(string file, HttpListenerResponse response)
        {
            var fileWithPath = $"wwwroot/controller/{file}.hd";
            if (File.Exists(fileWithPath))
            {
                try
                {
                    var code = ViewEngine.RenderFile(file, _interpreter);
                    response.StatusCode = code.Item2;

                    //Cleanup
                    Cache.Instance.Variables = new Dictionary<Meta, IVariable>();

                    Success($"Handled request successfully");
                    return code.Item1;
                }
                catch (Exception e)
                {
                    Error($"Error while handling request - /{file}");
                    response.StatusCode = 500;
                    return new byte[1];
                }
            }
            Error($"Error while handling request - file {file} does not exist!");
            response.StatusCode = 500;
            return new byte[1];
        }

        private static void OpenUrl(string url)
        {
            try
            {
                Process.Start(url);
            }
            catch
            {
                // hack because of this: https://github.com/dotnet/corefx/issues/10361
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
