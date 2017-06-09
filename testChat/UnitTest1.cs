using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VhackXtApi;
using VhackXtApi.Chat;
using System.Threading;
using System.Net.Sockets;
using VhackXtApi.Api;
using System.Text;
using System.Diagnostics;

namespace testChat
{
    [TestClass]
    public class UnitTest1
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
        class Config : IConfig 
        {
            Logger consoleLogger = new Logger();

            public string username => "wonderboy";

            public string password => "rancido";

            public string tessdata => @"C:\Program Files (x86)\tesseract-ocr\tessdata";

            public int waitstep => 1000;

            public int winchance => 75;

            public Tasks[] updates => null;

            public ILogger logger => consoleLogger;
        }

        static IConfig cfg = new Config();
        static VhackXtApi.Api.vhAPI api;
        static UnitTest1()
        {
            api = new VhackXtApi.Api.vhAPIBuilder()
               .config(cfg)
               .getAPI();
        }

        [TestMethod]
        public void TestChat()
        {
            ChatUtils.OnChatMessage += (s) => cfg.logger.Log(s);
            ChatUtils.connectToChat(api);
            Thread.Sleep(60000);
        }

        [TestMethod]
        public void TestChat1()
        {
            var sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            sock.Connect("chat.vhackxt.com", 7531); // ip: 85.25.237.247
            //sock.Connect("164.132.9.247", 15000);


            String str2 = "v[" + api.getUsername();
            var msg = ("NICK " + str2 + "\r\n");
            msg += ("USER " + api.getStats(Stats.id).Result + " 0 * : vHack XT@Android\r\n");

            sock.Send(Encoding.ASCII.GetBytes(msg));

            var vhackxy = "#vHackXT";

            var procLine = new Action<string>((readLine) =>
            {
                if (readLine.ToLower().StartsWith("ping "))
                {
                    String str3 = "PONG " + readLine.Substring(5) + "\r\n";
                    sock.Send(Encoding.ASCII.GetBytes(str3));
                }
                if (readLine.Contains(":(channel is full) transfering you to #"))
                {
                    //vhackxy = readLine
                    //        .Substring(readLine
                    //            .IndexOf(":(channel is full) transfering you to", 1))
                    //            .Replace(":(channel is full) transfering you to ", "")
                    //            .Replace("\r", "")
                    //            .Replace("\n", "")
                    //            .Replace(" ", "");
                }
                if (readLine.Contains(" 433 *"))
                {
                    //bufferedWriter.write("NICK " + str2 + "_" + "\r\n");
                    //bufferedWriter.flush();
                    sock.Send(Encoding.ASCII.GetBytes("NICK " + str2 + "_" + "\r\n"));
                }
                if (readLine.Contains("376"))
                {
                    sock.Send(Encoding.ASCII.GetBytes("PRIVMSG vHackXTGuard :.join " + api.getStats(Stats.id) + " " + api.getStats(Stats.hash) + "\r\n"));
                }
                if (readLine.Contains("PRIVMSG " + vhackxy + " :"))
                {
                    //Chat chat = new Chat();
                    //chat.chatMessage(readLine);

                    Console.WriteLine(readLine);
                }
            });

            while (true)
            {
                var bytes = new byte[1024];
                var count = sock.Receive(bytes);
                var lines = Encoding.ASCII.GetString(bytes, 0, count);

                foreach (var line in lines.Split(new []{ "\r\n" }, StringSplitOptions.None ))
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    procLine(line.Trim());
                }
            }
        }

        [TestMethod]
        public void TestChat2()
        {
            
            Thread.Sleep(1000000);
        }
    }




}
