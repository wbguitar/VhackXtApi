using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Topshelf;
using vHackApi;
using vHackApi.Api;
using vHackApi.Bot;
using vHackApi.Console;
using vHackApi.Interfaces;
using vHackApi.HTTP.Griffin;
using static vHackApi.HTTP.vhApiServer;

namespace vHackBot
{
    public class Program
    {
        public class Runnable: IDisposable
        {
            private Thread t1;
            public void Start()
            {
                var cfg1 = new Config();
                t1 = new Thread(() => Run(cfg1));

                ConsoleUtils.OnConsole += (sig) =>
                {
                    if (sig == ConsoleUtils.CtrlType.CTRL_CLOSE_EVENT ||
                        sig == ConsoleUtils.CtrlType.CTRL_C_EVENT)
                    {
                        Dispose();
                        return false;
                    }

                    return true;
                };

                t1.Start();
                //t1.Join();
            }

            public void Stop()
            {
                Dispose();
            }

            public void Dispose()
            {
                lock (this)
                {
                    if (timers != null)
                    {
                        for (var index = 0; index < timers.Count; index++)
                        {
                            var timer = timers[index];
                            timer.Dispose();
                            timers[index] = null;
                        }

                        timers = null;
                    }

                    if (t1 != null)
                    {
                        t1.Abort();
                        t1 = null;
                    } 
                }
            }
        }

        private static void Main(string[] args)
        {
            //var cfg1 = new Config();
            //var t1 = new Thread(() => Run(cfg1));

            //ConsoleUtils.OnConsole += (sig) =>
            //{
            //    if (sig == ConsoleUtils.CtrlType.CTRL_CLOSE_EVENT ||
            //        sig == ConsoleUtils.CtrlType.CTRL_C_EVENT)
            //    {
            //        timers.ForEach(tm => tm.Dispose());
            //        return false;
            //    }

            //    return true;
            //};

            //t1.Start();
            //t1.Join();


            var exit = HostFactory.Run(x =>                                 //1
            {
                x.Service<Runnable>(s =>                        //2
                {
                    s.ConstructUsing(name => new Runnable());     //3
                    s.WhenStarted(tc => tc.Start());              //4
                    s.WhenStopped(tc => tc.Stop());               //5
                   });
                x.RunAsLocalSystem();                            //6

                x.SetDescription("Vhack service host");        //7
                x.SetDisplayName("VhackSvc");                       //8
                x.SetServiceName("VhackSvc");                       //9
            });
        }

        private static List<IHackTimer> timers;

        private static void Run(IConfig cfg)
        {
            try
            {
                
                //if (!DbManager.Instance.Initialize(cfg))
                //    return;

                //try
                //{
                //    foreach (var ip in DbManager.Instance.GetIps())
                //    {
                //        XmlMgr.Default.IPs.Add(ip);
                //    }
                //}
                //catch (Exception e)
                //{

                //}

                //XmlMgr.Default.Save();
                XmlMgr.Default.Load();

                DbManager.Instance.Initialize(cfg);

                IPSelectorRandom.Default.Init(cfg);

                vhUtils.config = cfg;

                var builder = new vhAPIBuilder()
                    .useConfig(cfg);

                vhAPI api = builder.getAPI();
                var upd = new vHackApi.Update(cfg);
                var info = upd.botnetInfo();

                //var career = upd.getCareerStatus(api.UserHash);
                

                //GlobalConfig.Init(cfg, api);

                ProportionalUpgradeStrategy.Default.Init(cfg, api);



                // sets and starts timers
                timers = new List<IHackTimer>
                {
                    HackTheDev.Instance,
                    IPScanner.Instance,
                    HackBotNet.Instance,
                    IPAttack.Instance,
                    UpgradeMgr.Instance,
                };


                // sets the timers
                //timers.ForEach(tm => tm.Set(GlobalConfig.Config, GlobalConfig.Api));
                timers.ForEach(tm => tm.Set(cfg, api));

                log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));

                vHackApi.Bot.Log.ContestLogger = new ContestLogger();
                vHackApi.Bot.Log.ContestLogger.Log("**** START");
                //var logger = log4net.LogManager.GetLogger(typeof(Program)).Logger.Repository.GetAppenders()
                //    .FirstOrDefault(app => app.Name == "RollingChat");
                var logger = log4net.LogManager.GetLogger("ChatLogger");
                HackTheDev.Instance.Chat.MessageReceived += (s) =>
                {
                    if (string.IsNullOrEmpty(s))
                        return;

                    logger.Info(s.Trim());
                };


                // HTTP server
                var cfgParser = new ConfigParser();
                cfgParser.ConfigParsed += (c) =>
                {
                    if (c != null)
                    {
                        cfg.logger.Log($"New config received from remote client:\n{c.ToString()}");
                        if (c.maxAntivirus > 0)
                            Properties.Settings.Default.maxAntivirus = c.maxAntivirus;
                        if (c.winchance > 0)
                            Properties.Settings.Default.WinChance = c.winchance;
                        if (c.waitstep > 0)
                            Properties.Settings.Default.WaitStep = c.waitstep;
                        if (c.maxFirewall > 0)
                            Properties.Settings.Default.maxFirewall = c.maxFirewall;
                        if (c.maxAntivirus > 0)
                            Properties.Settings.Default.maxAntivirus = c.maxAntivirus;
                        if (c.finishAllFor > 0)
                            Properties.Settings.Default.finishAllFor = c.finishAllFor;
                        if (c.pcOrAttack > 0)
                            Properties.Settings.Default.pcOrAttack = c.pcOrAttack;

                        cfg.hackTheDevPaused = c.hackTheDevPaused;
                        cfg.hackBotNetPaused = c.hackBotNetPaused;
                        cfg.ipScannerPaused = c.ipScannerPaused;
                        cfg.ipAttackPaused = c.ipAttackPaused;

                        cfg.getImgBy = c.getImgBy;

                        Properties.Settings.Default.hackIfNotAnonymous = c.hackIfNotAnonymous;
                        Properties.Settings.Default.Save();
                    }
                    else
                        cfg.logger.Log($"Null config received from remote client");
                };

                cfgParser.ParseError += (e) =>
                {
                    cfg.logger.Log($"Error parsing config from remote client: {e.Message}");
                };

                var srv = new vhApiServer(cfg, cfgParser);

                new Thread(() => srv.Listen()).Start();


                // wait for exit
                Thread.Sleep(Timeout.Infinite); // TODO: waits for CTRL + C

            }
            catch (Exception e)
            {
                cfg.logger.Log(e.ToString());
            }
        }
    }
}