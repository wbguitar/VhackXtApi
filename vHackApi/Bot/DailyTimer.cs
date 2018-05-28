using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class DailyTimer : AHackTimer<DailyTimer>
    {
        public override void Set(IConfig cfg, vhAPI api)
        {
            Period = TimeSpan.FromSeconds(20);

            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** Stopping DailyTimer");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** Resuming DailyTimer");
            };

            InternalPause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING DailyTimer");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            InternalResume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING DailyTimer");
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
                        var console = api.getConsole();
                        //var cluster = await console.ClusterSystem(mi.UHash);
                        var res = await console.UpgradeClusterSW(vhConsole.UpgradeCluster.Network);
                        res = await console.UpgradeClusterSW(vhConsole.UpgradeCluster.Protection);



                        var hash = api.UserHash; //await api.getStats(Stats.uhash);
                        var data = await upd.GetDailyData(hash);
                        System.Console.WriteLine(data);

                        if ((int) data["login"] == 0)
                        {
                            var daily = await upd.GetDaily(1);
                        }
                        if ((int)data["scan1"] == 0)
                        {
                            var daily = await upd.GetDaily(2);
                        }
                        if ((int)data["scan2"] == 0)
                        {
                            var daily = await upd.GetDaily(3);
                        }
                        if ((int)data["transfer1"] == 0)
                        {
                            var daily = await upd.GetDaily(4);
                        }
                        if ((int)data["transfer2"] == 0)
                        {
                            var daily = await upd.GetDaily(5);
                        }

                    }
                    catch (Exception e)
                    {
                        cfg.logger.Log("DailyTimer error: {0}", e.ToString());
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
            , null, TimeSpan.Zero, Period);
        }
    }
}
