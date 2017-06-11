using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using vHackApi;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackBot
{
    class Program
    {
        static void Main(string[] args)
        {
            Run();
        }

        class ConsoleLogger : ILogger
        {

            #region ILogger Members

            public void Log(string format, params object[] parms)
            {
                var msg = string.Format(format, parms);
                Console.WriteLine("{0} - {1}", DateTime.Now, msg);
            }

            #endregion
        }

        class Config : IConfig
        {
            #region IConfig Members

            public string username
            {
                get { return "wonderboy"; }
            }

            public string password
            {
                get { return "rancido"; }
            }

            public Tasks[] updates
            {
                get { return new[] { Tasks.spam }; }
            }


            ConsoleLogger cl = new ConsoleLogger();
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

            #endregion
        }

        class Config1 : IConfig
        {
            #region IConfig Members

            public string username
            {
                get { return "c4ndym4n"; }
            }

            public string password
            {
                get { return "rancido"; }
            }

            public Tasks[] updates
            {
                get { return new[] { Tasks.spam }; }
            }


            ConsoleLogger cl = new ConsoleLogger();
            public ILogger logger
            {
                get { return cl; }
            }

            public string tessdata => Properties.Settings.Default.TessdataPath;

            public int waitstep => Properties.Settings.Default.WaitStep;

            public int winchance => Properties.Settings.Default.WinChance;

            public string dbConnectionString
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public int maxFirewall
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public int maxAntivirus
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            #endregion
        }

        static IConfig cfg = new Config();
        static List<Timer> timers = new List<Timer>();
        static async void Run()
        {
            try
            {
                if (!DbManager.Initialize(cfg))
                    return;

                var builder = new vhAPIBuilder()
                    .config(cfg);

                vhAPI api = builder.getAPI();

                // TODO Auto-generated method stub


                Debug.Print(String.Format("azz {0}", api.getStats(Stats.money)));

                var console = api.getConsole();
                var info = await console.MyInfo();
                //var user = await console.scanUser();
                //var pos = await console.GetTournamentPosition();
                //var clus = await console.ScanCluster("PCO");
                //var tour = await console.getTournament();

                var uhash = (string)info["uhash"];
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

                    if (DbManager.IpExist(dbip.IP))
                        DbManager.UpdateIp(dbip);
                    else
                        DbManager.AddIp(dbip);
                }




                //var sol = await console.attackIp("52.35.189.199", 8000);
                //var sol = await console.attackIp("127.0.0.1", 8000);
                object semaphore = new object();
                new Thread((o) =>
                {
                    while (true)
                    {
                        try
                        {
                            var s = console.AttackIp("127.0.0.1");
                        }
                        catch (Exception e)
                        {
                            cfg.logger.Log(e.ToString());
                        }

                        Thread.Sleep(TimeSpan.FromSeconds(15));
                    }
                }).Start();

                
                new Thread(() => callback(console))
                .Start()
                ;

                timers.Add(
                    new Timer(async (o) => await api.attackbotnetserver(0, uhash)
                    , null, TimeSpan.Zero, TimeSpan.FromSeconds(60))
                //, null, TimeSpan.Zero, TimeSpan.FromSeconds(10))
                );

                //var upd = new Update(cfg);

                //var tasks = await upd.getTasks();
                //var spywares = await upd.SpywareInfo();
                ////var removed = await upd.removeSpyware();
                //var taskAmount = await upd.getTaskAmount();
                //var taskIDs = await upd.getTaskIDs();
                //var started = await upd.startTask(Tasks.spam);
                ////var finished = await upd.finishTask((string)taskIDs[0]);
                //var bnInfo = await upd.botnetInfo();
                //var upgraded = await upd.upgradeBotnet((string)bnInfo[0]["bID"]);
                //var used = await upd.useBooster();
                //upd.doTasks(TimeSpan.FromSeconds(1));

                //var res = await console.getIP(10000);


                //Target[] targets = await console.getTargets();
                //for (int i = 0; i < targets.Length; i++)
                //{
                //    try
                //    {
                //        Target t = targets[i];
                //        Connection c = await t.connect();
                //        Debug.Print(String.Format("Scanning ip %s", c.getIP()));
                //        if (c.getSuccessRate() < 70)
                //        {
                //            Debug.Print("Target too strong");
                //            continue;
                //        }

                //        TransferResult tr = await c.trojanTransfer();

                //        if (tr.getSuccess())
                //        {
                //            Debug.Print(String.Format("Got $%d, %d rep", tr.getMoneyAmount(), tr.getRepGained()));
                //        }
                //        else
                //        {
                //            Debug.Print(String.Format("Lost %d rep", tr.getRepLost()));
                //        }

                //    }
                //    catch (Exception e)
                //    {
                //        Debug.Print(e.StackTrace);
                //    }
                //}
            }
            catch (Exception e)
            {
                //Debug.Print(String.Format("Error: %s", e.Message));
                cfg.logger.Log(e.ToString());
            }
        }

        private static async void callback(vhConsole console)
        {
            while (true)
            {
                try
                {
                    var img = await console.FindHostsAndAttack(vhConsole.config.maxFirewall); 
                }
                catch (Exception e)
                {
                    
                    cfg.logger.Log(e.ToString());
                }

                Thread.Sleep(vhConsole.WaitStep);
            }
        }
    }
}