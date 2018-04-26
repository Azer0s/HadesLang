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
            var configs = new List<ConfigHelper>();

            if (_args.Length == 0)
            {
                configs.Add(ConfigHelper.BuildConfig("config.hd"));
            }
            else
            {
                foreach (var s in _args)
                {
                    configs.Add(ConfigHelper.BuildConfig(s));
                    ConfigHelper.Reset();
                }
            }

            foreach (var cfg in configs)
            {
                var server = new Server.Server(cfg.Address, cfg.Port, cfg.RoutingEnabled, cfg.Browser, cfg.Interpreter, cfg.Routes, cfg.Forward);
                Task.Run(() =>
                {
                    server.Start();
                });
            }
        }
    }
}
