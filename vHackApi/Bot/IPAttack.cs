using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class IPAttack : AHackTimer<IPAttack>
    {
        private IPAttack()
        { }

        IConfig config;

        public override void Set(IConfig cfg, vhAPI api)
        {
            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            config = cfg;

            Period = TimeSpan.FromSeconds(2);

            var console = api.getConsole();

            hackTimer = new Timer(async (o) => await timerCallback(o), new object[] { cfg, api, console }, TimeSpan.Zero, Period);

            safeScan = cfg.safeScan;
        }

        private async Task timerCallback(object state)
        {
            var cfg = (state as object[])[0] as IConfig;
            var api = (state as object[])[1] as vhAPI;
            var c = (state as object[])[2] as vhConsole;

            if (!Monitor.TryEnter(this))
                return;

            Pause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING IP ATTACK");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            Resume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING IP ATTACK");
                    hackTimer.Change(TimeSpan.Zero, Period); 
                }
            };

            try
            {
                //// during contests better run as an IP scanner, so that can find ips 
                //// that are watched by FBI
                //if (vhUtils.IsContestRunning())
                //{
                //    var res = await c.FindHostsAndAttack();
                //    return;
                //}

                cfg.persistanceMgr.Update();
                var ip = cfg.ipSelector.NextHackabletIp(cfg.persistanceMgr);
                if (ip == null)
                {
                    var res = await c.FindHostsAndAttack();
                }
                else
                {
                    var res = await c.AttackIp(ip.IP);
                    if (res == 1) // skipped
                    {
                        if (ip.LastAttack != DateTime.MinValue)
                            ip.LastAttack += TimeSpan.FromMinutes(15); // retries in 15 minutes
                        else
                            ip.LastAttack = DateTime.Now - TimeSpan.FromMinutes(45);
                        if (!cfg.persistanceMgr.UpdateIp(ip))
                            ;//   Debug.Assert(false);
                    }
                }
            }
            catch (Exception e)
            {
                cfg.logger.Log("IPScanner error: {0}", e.Message);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

    }
}