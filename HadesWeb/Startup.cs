using System.Collections.Generic;
using System.Threading.Tasks;
using HadesWeb.Helper;

namespace HadesWeb
{
    class Startup
    {
        private readonly string[] _args;

        public Startup(string[] args)
        {
            _args = args;
        }

        public void Start()
        {
            var cfg = _args.Length == 0 ? ConfigHelper.BuildConfig("config.hd") : ConfigHelper.BuildConfig(_args[0]);
            var server = new Server.Server(cfg.Address, cfg.Port, cfg.RoutingEnabled, cfg.Browser, cfg.Interpreter, cfg.Routes, cfg.Forward);
            server.Start();
        }
    }
}
