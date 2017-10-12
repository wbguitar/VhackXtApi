using Newtonsoft.Json.Linq;
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
        Update upd;
        public override void Set(IConfig cfg, vhAPI api)
        {
            upd = new Update(cfg);
            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** STOPPING IP SCANNER");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** RESUMING IP SCANNER");
            };

            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            config = cfg;
            var console = api.getConsole();

            Period = cfg.ipScannerPolling;

            InternalPause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING IP SCANNER");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            InternalResume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING IP SCANNER");
                    hackTimer.Change(TimeSpan.Zero, Period);
                }
            };



            hackTimer = new Timer(async (o) =>
            {
                // if not on upgrade we'll skip attack
                if (!vhUtils.IsContestRunning() && UpgradeMgr.Instance.CurStatus != UpgradeMgr.Status.Upgrade)
                    return;

                if (!Monitor.TryEnter(localSemaphore))
                    return;

                // wait a random bit
                Thread.Sleep(rand.Next(0, (int)Period.TotalSeconds / 2));

                try
                {
                    // first make a search for new ips to hack
                    var img = await console.FindHostsAndAttack();
                    // then looks in the syslog if there's some ips that needs to be pwn3d
                    if (MyInfo.LastUpdInfo != null)
                    {
                        var myIp = MyInfo.LastUpdInfo.IP;
                        var sysLog = (await upd.getSysLog())["data"] as JArray;
                        foreach (var log in sysLog)
                        {
                            if ((string)log["from"] == myIp)
                            {
                                var newIp = (string)log["to"];
                                if (newIp != "Anonymous")
                                {
                                    IPs dbIp = null;
                                    if (!cfg.persistanceMgr.IpExist(newIp))
                                    {
                                        var js = await console.ScanIp(newIp);

                                        if (js == null)
                                        {
                                            cfg.logger.Log("********* BLOCKED BY FBI!!! **********");
                                            return;
                                        }
                                        dbIp = new IPs(js);
                                        cfg.persistanceMgr.AddIp(dbIp);
                                    }
                                    else
                                    {
                                        dbIp = cfg.persistanceMgr.GetIp(newIp);
                                    }

                                    // we'll sit and wait 10 minutes before trying to attack, just to not stir up suspicions...
                                    dbIp.LastAttack = DateTime.Now - TimeSpan.FromMinutes(10);
                                    cfg.persistanceMgr.UpdateIp(dbIp);
                                }
                            }
                        }
                    }
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