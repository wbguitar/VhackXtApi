using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class HackBotNet : AHackTimer<HackBotNet>
    {
        private HackBotNet() { }

        public override void Set(IConfig cfg, vhAPI api)
        {
            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            var upd = new Update(cfg);
            hackTimer = new Timer(async (o) =>
            {
                if (!Monitor.TryEnter(this))
                    return;
                Monitor.Enter(this);
                try
                {
                    await api.attackbotnetserver();
                    await upd.removeSpyware();
                }
                finally
                {
                    Monitor.Exit(this);
                }
            }
            , null, TimeSpan.Zero, cfg.hackBotnetPolling);
        }
    }
}