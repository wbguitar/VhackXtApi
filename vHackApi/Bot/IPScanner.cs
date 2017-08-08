using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class IPScanner : AHackTimer<IPScanner>
    {
        private IPScanner() { }

        IConfig config;

        public override void Set(IConfig cfg, vhAPI api)
        {
            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** Stopping IPScanner");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** Resuming IPScanner");
            };

            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            config = cfg;
            var console = api.getConsole();

            Period = TimeSpan.FromMilliseconds(cfg.waitstep);

            InternalPause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING IP SCANNER");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            InteranalResume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING IP SCANNER");
                    hackTimer.Change(TimeSpan.Zero, Period);
                }
            };

            hackTimer = new Timer(async (o) =>
            {
                // if onidle we'll skip attack
                if (UpgradeMgr.Instance.CurStatus == UpgradeMgr.Status.Idle)
                    return;

                if (!Monitor.TryEnter(localSemaphore))
                    return;

                try
                {
                    var img = await console.FindHostsAndAttack();
                }
                catch (Exception e)
                {
                    cfg.logger.Log(e.ToString());
                }
                finally
                {
                    Monitor.Exit(localSemaphore);
                }
            }
            , null, TimeSpan.Zero, Period);

            safeScan = cfg.safeScan;
        }
    }
}