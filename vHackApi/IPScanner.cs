using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi
{
    public interface IIPmanager
    {
        string NextIp();
    }

    public class IPScanner : IIPmanager
    {
        Timer t;
        vhAPI api;
        IConfig cfg;

        public IPScanner(vhAPI api, IConfig cfg)
        {
            this.api = api;
            this.cfg = cfg;
        }


        public void Start()
        {
            if (t != null)
            {
                t.Dispose();
                t = null;
            }

            t = new Timer(timerCallback, null, TimeSpan.Zero, TimeSpan.FromMilliseconds(cfg.waitstep));
        }

        private async void timerCallback(object state)
        {
            var c = api.getConsole();
            var ip = await c.FindHostsAndAttack(vhConsole.config.maxFirewall); // TODO: max firewall from config
        }

        public void Stop()
        {

        }

        public string NextIp()
        {
            throw new NotImplementedException();
        }
    }
}
