using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using vHackApi;
using vHackApi.Api;
using vHackApi.Bot;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Run();
        }

        private class ConsoleLogger : ILogger
        {
            #region ILogger Members

            public void Log(string format, params object[] parms)
            {
                var msg = string.Format(format, parms);
                Console.WriteLine("{0} - {1}", DateTime.Now, msg);
            }

            #endregion ILogger Members
        }

        private class Config : IConfig
        {
            #region IConfig Members

            public virtual string username
            {
                get { return "wonderboy"; }
            }

            public virtual string password
            {
                get { return "rancido"; }
            }

            public Tasks[] updates
            {
                get { return new[] { Tasks.Sdk }; }
            }

            private ConsoleLogger cl = new ConsoleLogger();

            public ILogger logger
            {
                get { return cl; }
            }

            public string tessdata => Properties.Settings.Default.TessdataPath;

            public int waitstep => Properties.Settings.Default.WaitStep;

            public int winchance => Properties.Settings.Default.WinChance;

            public string dbConnectionString => Properties.Settings.Default.dbConnString;

            public int maxFirewall => Properties.Settings.Default.maxFirewall;

            public int maxAntivirus => Properties.Settings.Default.maxAntivirus;

            public TimeSpan hackDevPolling => Properties.Settings.Default.hackDevPolling;

            public TimeSpan hackBotnetPolling => Properties.Settings.Default.hackBotnetPolling;

            //public IIPselector ipSelector => IPSelectorASAP.Instance;
            public IIPselector ipSelector => IPSelectorRandom.Instance;

            public IUpgradeStrategy upgradeStrategy => new FixedUpgradeStrategy(Tasks.Sdk);

            public IPersistanceMgr persistanceMgr => DbManager.Instance;

            #endregion IConfig Members
        }

        private class Config1 : Config
        {
            #region IConfig Members

            public override string username
            {
                get { return "c4ndym4n"; }
            }

            public override string password
            {
                get { return "rancido"; }
            }

            #endregion IConfig Members
        }

        private class Config2 : Config
        {
            #region IConfig Members

            public override string username
            {
                get { return "wonderboy1"; }
            }

            public override string password
            {
                get { return "rancido"; }
            }

            #endregion IConfig Members
        }

        private static IConfig cfg = new Config();
        private static List<Timer> timers = new List<Timer>();

        private static async void Run()
        {
            try
            {
                if (!DbManager.Instance.Initialize(cfg))
                    return;

                var builder = new vhAPIBuilder()
                    .useConfig(cfg);

                vhAPI api = builder.getAPI();

                

                //Debug.Print(String.Format("azz {0}", api.getStats(Stats.money)));

                var console = api.getConsole();
                var info = await MyInfo.Fetch(console);
                var upd = new Update(cfg);
                ////var updRes = await upd.useBooster();
                //var tasks = await upd.getTasks();
                //upd.startTask(Tasks.Sdk)




                //var uhash = (string)info["uhash"];

                // sets and starts watchdogs
                var watchdogs = new List<IHackTimer>
                {
                    HackTheDev.Instance,
                    IPScanner.Instance,
                    HackBotNet.Instance,
                    IPAttack.Instance,
                };

                watchdogs.ForEach(wd => wd.Set(cfg, api));

                //while (true) Thread.Sleep(10);
                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                cfg.logger.Log(e.ToString());
            }
        }

        private static async void callback(vhConsole console)
        {
            while (true)
            {
                try
                {
                    var img = await console.FindHostsAndAttack();
                }
                catch (Exception e)
                {
                    cfg.logger.Log(e.ToString());
                }

                Thread.Sleep(cfg.waitstep);
            }
        }

        private static async Task Test(string uhash, vhAPI api, vhConsole console, JObject info)
        {
            //var user = await console.scanUser();
            //var pos = await console.GetTournamentPosition();
            //var clus = await console.ScanCluster("PCO");
            //var tour = await console.getTournament();

            var bnInfo = await api.botnetInfo(uhash);

            //var urlBadScan = vhUtils.generateURL("user::::pass::::uhash::::target",
            //                     cfg.username + "::::" + cfg.password + "::::" + uhash + "::::" + "66.49.82.44",
            //                     "vh_loadRemoteData.php");
            //var urlGoodScan = vhUtils.generateURL("user::::pass::::uhash::::target",
            //                     cfg.username + "::::" + cfg.password + "::::" + uhash + "::::" + "225.129.253.141",
            //                     "vh_loadRemoteData.php");

            ////var jo1 = await console.ScanIp("127.0.0.1");
            //var ip = "66.49.82.44";
            //var jo1 = await console.ScanIp(ip);
            //if (jo1 == null)
            //    cfg.logger.Log("IP {0} doesn't exist", ip);

            var ip = "225.129.253.141";
            var jo1 = await console.ScanIp(ip);
            if (jo1 == null)
                cfg.logger.Log("IP {0} doesn't exist", ip);
            else
            {
                var dbip = new IPs(jo1);
                var jo2 = await console.ScanHost((string)jo1["hostname"]);

                if (cfg.persistanceMgr.IpExist(dbip.IP))
                    cfg.persistanceMgr.UpdateIp(dbip);
                else
                    cfg.persistanceMgr.AddIp(dbip);
            }
        }
       
    }
}