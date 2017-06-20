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

        //private static IPAttack inst;

        //public static IPAttack Instance
        //{
        //    get
        //    {
        //        if (inst == null)
        //            inst = new IPAttack();

        //        return inst;
        //    }
        //}

        public override void Set()
        {
            var cfg = GlobalConfig.Config;
            var api = GlobalConfig.Api;

            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            Period = TimeSpan.FromSeconds(2);

            var console = api.getConsole();

            hackTimer = new Timer(async (o) => await timerCallback(o), new object[] { cfg, api, console }, TimeSpan.Zero, Period);
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