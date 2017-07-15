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

        static IConfig cfg = new vHackBot.Program.Config();
        static void Main(string[] args)
        {
            var api = new vHackApi.Api.vhAPI(cfg);
            vHackApi.Chat.ChatTest.Test(api);
        }
    }
}
