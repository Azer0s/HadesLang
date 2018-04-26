using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Output;
using ServiceStack.Redis;
using Variables;
using static HadesWeb.Util.Log;

namespace HadesWeb.Helper
{
    class ConfigHelper
    {
        #region Helper vars

        private static Interpreter.Interpreter _interpreter;
        private static bool _routingEnabled;
        private static string _address;
        private static string _port;
        private static bool _browser;
        private static string _routingFile;
        private static readonly Dictionary<string, string> _routes = new Dictionary<string, string>();
        private static readonly List<string> _forward = new List<string>();

        #endregion

        #region Public vars

        public Interpreter.Interpreter Interpreter { get; set; }
        public bool RoutingEnabled { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
        public bool Browser { get; set; }
        public string RoutingFile { get; set; }
        public Dictionary<string, string> Routes { get; set; }
        public List<string> Forward { get; set; }

        #endregion

        private ConfigHelper(){}

        public static ConfigHelper BuildConfig()
        {
            _interpreter = new global::Interpreter.Interpreter(new NoOutput(), new NoOutput());

            string redis = null;

            #region Config

            _interpreter.RegisterFunction(new Function("config", a =>
            {
                var parameters = a.ToList();
                if (parameters.Count != 2)
                {
                    return "false";
                }

                parameters[0] = parameters[0].ToString().Trim('\'').Trim();
                parameters[1] = parameters[1].ToString().Trim('\'').Trim();

                switch (parameters[0].ToString())
                {
                    case "address":
                        _address = parameters[1].ToString();
                        break;
                    case "port":
                        _port = parameters[1].ToString();
                        break;
                    case "log":
                        _log = bool.Parse(parameters[1].ToString());
                        break;
                    case "redis":
                        redis = parameters[1].ToString();
                        break;
                    case "startBrowser":
                        _browser = bool.Parse(parameters[1].ToString());
                        break;
                    case "routing":
                        _routingFile = parameters[1].ToString();
                        break;
                }

                return "true";
            }));

            _interpreter.InterpretLine("with config", new List<string> { }, null);

            if (_log)
            {
                if (!string.IsNullOrEmpty(redis))
                {
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
                else
                {
                    Error("Redis connection string was not set!");
                }
            }

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


                var route = parameters[0].ToString().Trim('\'').Trim();
                var action = parameters[1].ToString().Trim('\'').Trim();

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
                    _routes.Add(route, action);
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

                var fileType = parameters[0].ToString().Trim('\'').Trim();

                try
                {
                    _forward.Add(fileType);
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
                _interpreter.InterpretLine($"with '{_routingFile}'", new List<string> { "web" }, null);
            }

            if (_routes.Count != 0)
            {
                _routingEnabled = true;
            }
            else
            {
                Info("No routing file given - switching to autorouting!");
            }

            #endregion

            return new ConfigHelper
            {
                Address = _address,
                Browser = _browser,
                Forward = _forward,
                Interpreter = _interpreter,
                Port = _port,
                Routes = _routes,
                RoutingEnabled = _routingEnabled,
                RoutingFile = _routingFile
            };
        }
    }
}
