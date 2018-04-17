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
    public class DailyTimer: AHackTimer<DailyTimer>
    {
        public override void Set(IConfig cfg, vhAPI api)
        {
            Period = TimeSpan.FromSeconds(10);
            
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
                    return;

                if (!Monitor.TryEnter(this))
                    return;
                
                try
                {
                    var mi = await MyInfo.Fetch(api.getConsole());
                    var console = api.getConsole();
                    //var cluster = await console.ClusterSystem(mi.UHash);
                    var res = await console.UpgradeClusterSW(vhConsole.UpgradeCluster.Network);
                    res = await console.UpgradeClusterSW(vhConsole.UpgradeCluster.Protection);


                    //
                    //var hash = await api.getStats(Stats.uhash);
                    //var data = await upd.GetDailyData(hash);
                    //System.Console.WriteLine(data);

                    for (int i = 1; i <= 5; i++)
                    {
                        var data = await upd.GetDaily(i);
                        System.Console.WriteLine(data);
                    }

                }
                catch (Exception e)
                {
                    
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
