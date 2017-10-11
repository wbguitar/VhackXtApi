using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using vHackApi;
using vHackApi.Chat;
using System.Threading;
using System.Net.Sockets;
using vHackApi.Api;
using System.Text;
using System.Diagnostics;
using vHackApi.Interfaces;

namespace testChat
{
    [TestClass]
    public class Chat
    {
        [TestMethod]
        public void TestMethod1()
        {
            
            //var chat = new Client("chat.vhackxt.com", 7531);
            var chat = new Client("164.132.9.247", 15000);
            chat.OnResponse += Chat_OnResponse;

            chat.Connect();
            chat.WaitConnect();

            chat.Send("hello");
            chat.WaitSend();

            chat.Receive();
            chat.WaitReceive();

            
        }

        private void Chat_OnResponse(Client.State obj)
        {
            Console.WriteLine(obj.sb.ToString());
        }

        class Logger : ILogger
        {
            public void Log(string format, params object[] parms)
            {
                var msg = string.Format(format, parms);
                Console.WriteLine(msg);
            }

        }
        //class Config : IConfig 
        //{
        //    Logger consoleLogger = new Logger();

        //    public string username => "wonderboy";

        //    public string password => "rancido";

        //    public string tessdata => @"C:\Program Files (x86)\tesseract-ocr\tessdata";

        //    public int waitstep => 1000;

        //    public int winchance => 75;

        //    public Tasks[] updates => null;

        //    public ILogger logger => consoleLogger;

        //    public int maxFirewall
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public int maxAntivirus
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public TimeSpan hackDevPolling
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public TimeSpan hackBotnetPolling
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public string dbConnectionString
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public IIPselector ipSelector
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }

        //    public IUpgradeStrategy upgradeStrategy
        //    {
        //        get
        //        {
        //            throw new NotImplementedException();
        //        }
        //    }
        //}

        static IConfig cfg = new vHackBot.Program.Config();
        static vHackApi.Api.vhAPI api;
        static Chat()
        {
            api = new vHackApi.Api.vhAPIBuilder()
               .useConfig(cfg)
               .getAPI();
        }

        [TestMethod]
        public void TestChat()
        {
            ChatUtils.OnChatMessage += (s) => cfg.logger.Log(s);
            ChatUtils.ConnectToChat(api);
            Thread.Sleep(60000);
        }

        [TestMethod]
        public void TestChat1()
        {
           ChatUtils.RunChat(api);    
        }

        [TestMethod]
        public void TestChat2()
        {
            
            Thread.Sleep(1000000);
        }
    }




}
