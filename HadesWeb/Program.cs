using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HadesWeb.Helper;
using Output;
using ServiceStack.Redis;
using Variables;
using Console = Colorful.Console;
using static HadesWeb.Util.Log;
// ReSharper disable InconsistentNaming

namespace HadesWeb
{
    class Program
    {
        public static void Main(string[] args)
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

            if (args.Length == 0)
            {
                var cfg = ConfigHelper.BuildConfig("config.hd");
                var server = new Server.Server(cfg.Address,cfg.Port,cfg.RoutingEnabled,cfg.Browser,cfg.Interpreter,cfg.Routes,cfg.Forward);
                server.Start();
            }
            else
            {
                var configs = new List<ConfigHelper>();
                
                foreach (var s in args)
                {
                    configs.Add(ConfigHelper.BuildConfig(s));
                    ConfigHelper.Reset();
                }

                foreach (var cfg in configs)
                {
                    var server = new Server.Server(cfg.Address,cfg.Port,cfg.RoutingEnabled,cfg.Browser,cfg.Interpreter,cfg.Routes,cfg.Forward);
                    Task.Run(() =>
                    {
                        server.Start();
                    });
                }
            }

            Console.ReadKey();
            // ReSharper disable once FunctionNeverReturns
        }
    }
}