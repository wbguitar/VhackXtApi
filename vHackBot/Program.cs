using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using vHackApi;
using vHackApi.Api;
using vHackApi.Bot;
using vHackApi.Console;
using vHackApi.Interfaces;
using System.Net;

namespace vHackBot
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var cfg1 = new Config();
            var cfg2 = new Config();
            cfg2.proxy = new WebProxy("94.200.103.99", 3128);
            var cfg3 = new Config();
            cfg3.proxy = null;
            var t1 = new Thread(() => Run(cfg1));
            var t2 = new Thread(() => Run(cfg2));
            var t3 = new Thread(() => Run(cfg3));
            
            ConsoleUtils.OnConsole += (sig) =>
            {
                if (sig == ConsoleUtils.CtrlType.CTRL_CLOSE_EVENT ||
                    sig == ConsoleUtils.CtrlType.CTRL_C_EVENT)
                {
                    timers.ForEach(tm => tm.Dispose());
                    return false;
                }

                return true;
            };

            //t1.Start();
            //Thread.Sleep(100);
            //t2.Start();
            //Thread.Sleep(100);
            t3.Start();

            if (t1.IsAlive) t1.Join();
            if (t2.IsAlive) t2.Join();  
            if (t3.IsAlive) t3.Join();
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

            public virtual string username => Properties.Settings.Default.user;

            public virtual string password => Properties.Settings.Default.pass;

            public virtual bool hackIfNotAnonymous => Properties.Settings.Default.hackIfNotAnonymous;

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

            public bool safeScan => Properties.Settings.Default.safeScan;

            //public IIPselector ipSelector => IPSelectorASAP.Instance;
            public IIPselector ipSelector => IPSelectorRandom.Default;

            public IUpgradeStrategy upgradeStrategy => ProportionalUpgradeStrategy.Default;
            //public IUpgradeStrategy upgradeStrategy => StatisticalUpgradeStrategy.Ston.Default;

            //public IPersistanceMgr persistanceMgr => DbManager.Instance;
            public IPersistanceMgr persistanceMgr => XmlMgr.Default;

            public IWebProxy proxy { get; set; } = new WebProxy(Properties.Settings.Default.proxyAddress, Properties.Settings.Default.proxyPort);

            public int finishAllFor => Properties.Settings.Default.finishAllFor;

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

        private static List<Timer> timers = new List<Timer>();

        private static void Run(IConfig cfg)
        {
            try
            {
                
                if (!DbManager.Instance.Initialize(cfg))
                    return;

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

                IPSelectorRandom.Default.Init(cfg);

                vhUtils.config = cfg;

                var builder = new vhAPIBuilder()
                    .useConfig(cfg);

                vhAPI api = builder.getAPI();

                GlobalConfig.Init(cfg, api);


                ProportionalUpgradeStrategy.Default.Init(cfg, api);

                // sets and starts timers
                var timers = new List<IHackTimer>
                {
                    HackTheDev.Instance,
                    IPScanner.Instance,
                    HackBotNet.Instance,
                    IPAttack.Instance,
                    UpgradeMgr.Instance,
                };

                // sets the timers
                timers.ForEach(tm => tm.Set(GlobalConfig.Config, GlobalConfig.Api));


                // dispose on CTRL+C
                //Console.CancelKeyPress += (s, e) =>
                //{
                //    if (e.SpecialKey == ConsoleSpecialKey.ControlC)
                //        timers.ForEach(tm => tm.Dispose());
                //};

                //Process.GetCurrentProcess().Exited += (s, e) =>
                //{
                //    timers.ForEach(tm => tm.Dispose());
                //};

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

                // wait for exit
                Thread.Sleep(Timeout.Infinite); // TODO: waits for CTRL + C

            }
            catch (Exception e)
            {
                cfg.logger.Log(e.ToString());
            }
        }
    }

    public class ConsoleUtils
    {
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        public delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        public enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        public static event EventHandler OnConsole
        {
            add
            {
                SetConsoleCtrlHandler(value, true);
            }

            remove
            {
                SetConsoleCtrlHandler(value, false);
            }
        }
        
    }
}