using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
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

            var cfg = ConfigHelper.BuildConfig();
            var server = new Server.Server(cfg.Address,cfg.Port,cfg.RoutingEnabled,cfg.Browser,cfg.Interpreter,cfg.Routes,cfg.Forward);
            server.Start();
            // ReSharper disable once FunctionNeverReturns
        }
    }
}