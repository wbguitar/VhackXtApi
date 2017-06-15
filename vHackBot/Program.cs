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

        private static void Run()
        {
            try
            {
                if (!DbManager.Instance.Initialize(cfg))
                    return;

                var builder = new vhAPIBuilder()
                    .useConfig(cfg);

                vhAPI api = builder.getAPI();

                

                // sets and starts timers
                var timers = new List<IHackTimer>
                {
                    HackTheDev.Instance,
                    IPScanner.Instance,
                    HackBotNet.Instance,
                    IPAttack.Instance,
                    //UpgradeMgr.Instance,
                };

                timers.ForEach(wd => wd.Set(cfg, api));

                Thread.Sleep(Timeout.Infinite);
            }
            catch (Exception e)
            {
                cfg.logger.Log(e.ToString());
            }
        }
    }
}