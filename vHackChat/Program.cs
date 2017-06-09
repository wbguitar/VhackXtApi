using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhackXtApi;

namespace vHackChat
{
    class Program
    {
        class ConsoleLogger : VhackXtApi.ILogger
        {

            #region ILogger Members

            public void Log(string format, params object[] parms)
            {
                var msg = string.Format(format, parms);
                Console.WriteLine("{0} - {1}", DateTime.Now, msg);
            }

            #endregion
        }

        class Config : VhackXtApi.IConfig
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

            public string tessdata => @"C:\Program Files (x86)\tesseract-ocr\tessdata";

            public int waitstep => 1000;

            public int winchance => 75;

            #endregion
        }

        static Config cfg = new Config();
        static void Main(string[] args)
        {
            var api = new VhackXtApi.Api.vhAPI(cfg);
            VhackXtApi.Chat.ChatTest.Test(api);
        }
    }
}
