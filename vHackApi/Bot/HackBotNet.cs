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
            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** Stopping HackBotNet");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** Resuming HackBotNet");
            };

            InternalPause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING HackBotNet");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            InternalResume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING HackBotNet");
                    hackTimer.Change(TimeSpan.Zero, Period);
                }
            };

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
                
                try
                {
                    //await api.attackbotnetserver();
                    // TODO: BOTNET 
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