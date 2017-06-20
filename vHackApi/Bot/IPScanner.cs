using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class IPScanner : AHackTimer<IPScanner>
    {
        private IPScanner() { }

        //private static IPScanner inst;

        //public static IPScanner Instance
        //{
        //    get
        //    {
        //        if (inst == null)
        //            inst = new IPScanner();

        //        return inst;
        //    }
        //}

        public override void Set(IConfig cfg, vhAPI api)
        {
            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            var console = api.getConsole();

            Period = TimeSpan.FromMilliseconds(cfg.waitstep);

            Pause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING IP SCANNER");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            Resume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING IP SCANNER");
                    hackTimer.Change(TimeSpan.Zero, Period);
                }
            };

            hackTimer = new Timer(async (o) =>
            {
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
        }
    }
}