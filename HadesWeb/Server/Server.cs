using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using HadesWeb.Helper;
using HadesWeb.Util;
using StringExtension;
using Variables;

// ReSharper disable InconsistentNaming

namespace HadesWeb.Server
{
    class Server
    {
        private readonly Interpreter.Interpreter Interpreter;
        private readonly bool RoutingEnabled;
        private readonly string Address;
        private readonly string Port;
        private readonly bool StartBrowser;
        private readonly Dictionary<string, string> Routes;
        private readonly List<string> Forward;
        private readonly List<string> Static;

        #region RegquestParams

        private HttpListenerContext context;
        private HttpListenerResponse response;
        private HttpListenerRequest request;

        #endregion

        public Server(string address, string port, bool routingEnabled, bool startBrowser, Interpreter.Interpreter interpreter, Dictionary<string, string> routes, List<string> forward, List<string> staticitems)
        {
            RoutingEnabled = routingEnabled;
            Address = address;
            Port = port;
            StartBrowser = startBrowser;
            Interpreter = interpreter;
            Routes = routes;
            Forward = forward;
            Static = staticitems;
        }

        public void Start()
        {
            #region Hades Interop Methods

            Interpreter.RegisterFunction(new Function("param", a =>
            {
                var parameters = a.ToList();

                if (parameters.ToList().Count != 1)
                {
                    return "''";
                }

                try
                {
                    var returnString = request.QueryString[parameters[0].ToString().Trim('\'').Trim()];
                    return string.IsNullOrEmpty(returnString) ? "''" : $"'{returnString}'";
                }
                catch (Exception)
                {
                    return "''";
                }
            }));

            Interpreter.RegisterFunction(new Function("getMethod", a => $"'{request.HttpMethod}'"));

            #endregion

            var listener = new HttpListener();
            listener.Prefixes.Add($"http://{Address}:{Port}/");
            Log.Info($"Listening @ {Address} on port {Port}");
            listener.Start();

            if (StartBrowser)
            {
                BrowserHelper.OpenUrl($"http://{Address}:{Port}/");
            }

            var cached = new Dictionary<string, byte[]>();

            while (true)
            {
                context = listener.GetContext();
                request = context.Request;
                response = context.Response;
                var returnBytes = new byte[] { };
                var rawUrl = request.RawUrl.Split('?')[0];

                Log.Info($"Request - {rawUrl}");

                if (Static.Contains(request.RawUrl) && cached.ContainsKey(request.RawUrl))
                {
                    returnBytes = cached[request.RawUrl];
                    goto sendBack;
                }

                if (RoutingEnabled)
                {
                    var action = string.Empty;
                    var basepath = "wwwroot/controller/{0}.hd";

                    if (rawUrl.EndsWithFromList(Forward))
                    {
                        returnBytes = FileHelper.GetFile(rawUrl);
                    }
                    else
                    {
                        if (Routes.ContainsKey(rawUrl))
                        {
                            action = Routes[rawUrl];
                        }
                        else if (Routes.ContainsKey(rawUrl.Replace(".hd", "")))
                        {
                            action = Routes[rawUrl.Replace(".hd", "")];
                        }
                        else if (Routes.Where(a => a.Key.EndsWith("/(.+)") && a.Key != "/").Select(a => a.Key).Any(a => Regex.IsMatch(rawUrl, a)))
                        {
                            var pair = Routes.Where(a => Regex.IsMatch(rawUrl, a.Key) && a.Key != "/").Select(a => a)
                                .First();
                            action = Regex.Replace(rawUrl, pair.Key,
                                a => pair.Value.Replace("/*", $"/{a.Groups[1].Value}"));
                            basepath = "wwwroot{0}.hd";
                        }
                        else
                        {
                            Log.Error($"No route specified for action {rawUrl}!");
                        }

                        if (string.IsNullOrEmpty(action))
                        {
                            response.StatusCode = 500;
                        }
                        else
                        {
                            returnBytes = RegexCollection.Store.HasExtension.IsMatch(action)
                                ? FileHelper.GetFile(action)
                                : FileHelper.InterpretFile(action, response, Interpreter, basepath);
                        }
                    }
                }
                //Autoroute
                else
                {
                    if (rawUrl == "/" || rawUrl == "/#")
                    {
                        returnBytes = FileHelper.InterpretFile("index", response, Interpreter);
                    }
                    else
                    {
                        returnBytes =
                            rawUrl.EndsWith(".hd") || File.Exists($"wwwroot/controller{rawUrl}.hd")
                                ? FileHelper.InterpretFile(rawUrl.TrimStart('/').Replace(".hd", ""), response, Interpreter)
                                : FileHelper.GetFile(rawUrl);
                    }
                }

                if (Static.Contains(request.RawUrl) && !cached.ContainsKey(request.RawUrl))
                {
                    cached.Add(request.RawUrl,returnBytes);
                }

                sendBack:
                response.ContentLength64 = returnBytes.Length;
                var output = response.OutputStream;
                output.Write(returnBytes, 0, returnBytes.Length);
                output.Close();
            }
        }
    }
}
