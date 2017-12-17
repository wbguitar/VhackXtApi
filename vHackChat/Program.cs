using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Interfaces;
using System.Windows.Forms;

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

        static IConfig cfg = new vHackBot.Config();
        [STAThread]
        static void Main(string[] args)
        {
            //var api = new vHackApi.Api.vhAPI(cfg);
            ////vHackApi.Chat.ChatTest.Test(api);

            ////vHackApi.Chat.ChatUtils.ConnectToChat(api);

            ///*vHackApi.Chat.ChatUtils.RunChat(api);*/
            //var chat = new vHackApi.Chat.vhChat(cfg, api);
            //chat.PrivateMessage += Chat_PrivateMessage;
            ////chat.MessageReceived += Chat_MessageReceived;
            //chat.Run();
            //Thread.Sleep(Timeout.Infinite);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Chat());
        }

        private static void Chat_MessageReceived(string obj)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(obj);
            Console.ResetColor();
        }

        private static void Chat_PrivateMessage(string arg1, string arg2, string arg3)
        {
            Console.Write("[");
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write($"{arg2}@{arg1}");
            Console.ResetColor();
            Console.Write("]: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(arg3);
            Console.ResetColor();
        }
    }
}
