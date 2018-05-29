using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Antlr.Runtime;
using Griffin.Logging.Loggers;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    
    public static class Log
    {
        class FakeLogger : ILogger
        {
            public void Log(string format, params object[] parms)
            {

            }
        }

        public static ILogger ContestLogger { get; set; } = new FakeLogger();
    }

    public class IPAttack : AHackTimer<IPAttack>
    {
        private IPAttack()
        { }

        IConfig config;

        public override void Set(IConfig cfg, vhAPI api)
        {
            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** Stopping IPAttack");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** Resuming IPAttack");
            };

            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            config = cfg;

            Period = cfg.ipAttackPolling;

            var console = api.getConsole();

            hackTimer = new Timer(async (o) => await timerCallback(o), new object[] { cfg, api, console }, TimeSpan.Zero, Period);
            
            safeScan = cfg.safeScan;


            InternalPause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING IP ATTACK");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            InternalResume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING IP ATTACK");
                    hackTimer.Change(TimeSpan.Zero, Period);
                }
            };
        }

        private async Task timerCallback(object state)
        {
            // if not on upgrade we'll skip attack
            if (!vhUtils.IsContestRunning() && UpgradeMgr.Instance.CurStatus != UpgradeMgr.Status.Upgrade)
                return;

            var cfg = (state as object[])[0] as IConfig;
            var api = (state as object[])[1] as vhAPI;
            var c = (state as object[])[2] as vhConsole;

            if (!Monitor.TryEnter(this))
                return;

            // wait a random bit
            Thread.Sleep(rand.Next(0, (int)Period.TotalSeconds / 5)*1000);

            try
            {
                var cluster = await c.CheckCluster();
                if (cluster != null && (int) cluster["blocked"] == 1)
                {
                    cfg.logger.Log("CLUSTER UNDER ATTACK, bypass host attack");
                    return;
                }
                // during contests better run as an IP scanner, so that can find ips 
                // that are watched by FBI
                if (vhUtils.IsContestRunning())
                {
                    var res = await c.FindHostsAndAttack();
                    return;
                }

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
                        if (ip.LastAttack != IPs.MinDateTime)
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
                cfg.logger.Log("IPAttack error: {0}", e.Message);
            }
            finally
            {
                Monitor.Exit(this);
            }
        }

    }
}