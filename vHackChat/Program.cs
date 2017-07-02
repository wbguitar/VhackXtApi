using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Interfaces;

namespace vHackChat
{
    class Program
    {
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
            

            ConsoleLogger cl = new ConsoleLogger();
            public ILogger logger
            {
                get { return cl; }
            }

            public string tessdata => @"C:\Program Files (x86)\tesseract-ocr\tessdata";

            public int waitstep => 1000;

            public int winchance => 75;

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

            public TimeSpan hackDevPolling
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public TimeSpan hackBotnetPolling
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public string dbConnectionString
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IIPselector ipSelector
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IUpgradeStrategy upgradeStrategy
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool safeScan
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public IPersistanceMgr persistanceMgr
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            #endregion
        }

        static Config cfg = new Config();
        static void Main(string[] args)
        {
            var api = new vHackApi.Api.vhAPI(cfg);
            vHackApi.Chat.ChatTest.Test(api);
        }
    }
}
