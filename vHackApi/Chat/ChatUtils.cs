using System;
using System.IO;
using System.Text;
using System.Threading;
using vHackApi.Api;
using vHackApi.Console;
using WB.Commons.Net.Bin;
using WB.Commons.Net.Bin.Interfaces;
using WB.IIIParty.Commons.Protocol;

namespace vHackApi.Chat
{
    public class ChatUtils
    {
        public static String m9147c(String str)
        {
            String str2 = "abcdefghijklmnopqrstuvwxyz0123456789[]_{}|";
            String str3 = "";
            for (int i = 0; i < str.Length; i++)
            {
                String substring = str.Substring(i, i + 1);
                if (str2.Contains(substring.ToLower()))
                {
                    str3 = str3 + substring;
                }
            }
            if (str3.Length < 5)
            {
                return str3 + "|";
            }
            return str3;
        }

        public static String vhackxy { get; private set; } = "#vHackXT";
        //static BufferedWriter bufferedWriter;
        public static Client Client { get; private set; }

        public static void sendChatMessage(String message)
        {
            try
            {

                vhConsole.config.logger.Log("Sending: {0}", message);
                //bufferedWriter.write("PRIVMSG " + vhackxy + " :" + message + "\r\n");
                //bufferedWriter.flush();

                Client.Send("PRIVMSG " + vhackxy + " :" + message + "\r\n");

            }
            catch (IOException e)
            {
                // TODO Auto-generated catch block
                vhConsole.config.logger.Log(e.StackTrace);
            }
        }

        static Thread threadA;
        public static void connectToChat(vhAPI api)
        {
            threadA = new Thread(() =>
            {
                try
                {

                    Client = new Client("chat.vhackxt.com", 7531);

                    //bufferedWriter = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream()));
                    //BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(socket.getInputStream()));
                    //String str2 = "v[" + api.getUsername();
                    //bufferedWriter.write("NICK " + str2 + "\r\n");
                    //bufferedWriter.write("USER " + api.getStats(Stats.id) + " 0 * : vHack XT@Android\r\n");
                    //bufferedWriter.flush();


                    String str2 = "v[" + api.getUsername();

                    Client.OnResponse += (s) =>
                    {
                        var lines = s.sb.ToString();
                        foreach (var line in lines.Split(new[] { "\r\n" }, StringSplitOptions.None))
                        {
                            if (string.IsNullOrEmpty(line))
                                continue;

                            procLine(line.Trim(), str2, api);
                        }
                    };

                  

                    // starts connection
                    Client.Connect();
                    Client.WaitConnect();

                    // handshake message
                    var id = api.getStats(Stats.id).Result;
                    var msg = string.Format("NICK {0}\r\nUSER {1} 0 * : vHack XT@Android\r\n", str2, id);
                    Client.Send(msg);

                    var t = new Thread(() =>
                    {
                        while (true)
                        {
                            Client.Receive();
                            Client.WaitReceive();
                        }
                    });
                    t.Start();
                }
                catch (System.Net.WebException e)
                {
                    // TODO Auto-generated catch block
                    vhConsole.config.logger.Log(e.StackTrace);
                }
                catch (IOException e)
                {
                    // TODO Auto-generated catch block
                    vhConsole.config.logger.Log(e.StackTrace);
                }
            });
            threadA.Start();
            threadA.Join();
        }

        private static void Client_OnResponse(Client.State s)
        {
            
        }

        static async void procLine(string readLine, String str2, vhAPI api)
        {
            if (readLine.ToLower().StartsWith("ping "))
            {
                String str3 = "PONG " + readLine.Substring(5) + "\r\n";
                Client.Send(str3);
            }
            if (readLine.Contains(":(channel is full) transfering you to #"))
            {
                vhackxy = readLine
                        .Substring(readLine
                            .IndexOf(":(channel is full) transfering you to", 1))
                            .Replace(":(channel is full) transfering you to ", "")
                            .Replace("\r", "")
                            .Replace("\n", "")
                            .Replace(" ", "");
            }
            if (readLine.Contains(" 433 *"))
            {
                //bufferedWriter.write("NICK " + str2 + "_" + "\r\n");
                //bufferedWriter.flush();
                Client.Send("NICK " + str2 + "_" + "\r\n");
            }
            if (readLine.Contains("376"))
            {
                Client.Send("PRIVMSG vHackXTGuard :.join " + api.getStats(Stats.id) + " " + api.getStats(Stats.hash) + "\r\n");
                //bufferedWriter.write("PRIVMSG vHackXTGuard :.join " + api.getStats(Stats.id) + " " + api.getStats(Stats.hash) + "\r\n");
                //bufferedWriter.flush();
                //bufferedWriter.flush();
            }
            if (readLine.Contains("PRIVMSG " + vhackxy + " :"))
            {
                //Chat chat = new Chat();
                //chat.chatMessage(readLine);

                OnChatMessage(readLine);
            }
        }

        public static event Action<string> OnChatMessage = (s) => { };
    }

    namespace Reqs
    {
        public class Join : IMessage
        {
            public MessageTypes MessageType => MessageTypes.Request;

            public object SyncRef => new object();

            public int ResponseCount => 0;

            string msg;
            public Join(vhAPI api)
            {
                //String str2 = "v[" + api.getUsername();
                //String str2 = "v[" + api.getUsername() + "]";
                String str2 = api.getUsername();

                msg = ("NICK " + str2 + "\r\n");
                msg += ("USER " + api.getStats(Stats.id).Result + " 0 * : vHack XT@Android\r\n");
            }

            public string Display()
            {
                return msg;
            }

            public byte[] GetByteArray()
            {
                return Encoding.ASCII.GetBytes(msg);
            }

            public void Validate()
            {
            }
        }

        public class Message : IMessage
        {
            public MessageTypes MessageType => MessageTypes.Request;

            public object SyncRef { get; private set; }

            public int ResponseCount => 1;

            public static String vhackxy { get; private set; } = "#vHackXT";
            string str = "";
            public Message(string readLine, vhAPI api)
            {
                SyncRef = new object();

                if (readLine.ToLower().StartsWith("ping "))
                {
                    str = "PONG " + readLine.Substring(5) + "\r\n";
                    //sock.Send(Encoding.ASCII.GetBytes(str3));
                }
                if (readLine.Contains(":(channel is full) transfering you to #"))
                {
                    vhackxy = readLine
                            .Substring(readLine
                                .IndexOf(":(channel is full) transfering you to", 1))
                                .Replace(":(channel is full) transfering you to ", "")
                                .Replace("\r", "")
                                .Replace("\n", "")
                                .Replace(" ", "");
                }
                if (readLine.Contains(" 433 *"))
                {
                    String str2 = "v[" + api.getUsername();
                    //var msg = ("NICK " + str2 + "\r\n");
                    //msg += ("USER " + api.getStats(Stats.id).Result + " 0 * : vHack XT@Android\r\n");

                    str = "NICK " + str2 + "_" + "\r\n";
                }
                if (readLine.Contains("376"))
                {
                    str = "PRIVMSG vHackXTGuard :.join " + api.getStats(Stats.id) + " " + api.getStats(Stats.hash) + "\r\n";
                }
            }

            public string Display()
            {
                return str;
            }

            public byte[] GetByteArray()
            {
                return Encoding.ASCII.GetBytes(str);
            }

            public class NotValidException : Exception { }

            public void Validate()
            {
                if (string.IsNullOrEmpty(str))
                    throw new NotValidException();
            }
        }
    }

    namespace Resps
    {
        public class Message : IMessage
        {
            public MessageTypes MessageType => MessageTypes.Response;

            public object SyncRef { get; private set; }

            public int ResponseCount => 0;

            public string Msg { get; private set; }
            public Message(string msg)
            {
                Msg = msg;
                SyncRef = new object();
            }

            public string Display()
            {
                return Msg;
            }

            public byte[] GetByteArray()
            {
                return Encoding.ASCII.GetBytes(Msg);
            }

            public void Validate()
            {
            }
        }
    }

    public class MessageParser : IMessageParser
    {
        public MessageParser()
        {

        }

        public bool CanReadLength(byte[] data)
        {
            return true;
        }

        public int GetLength(byte[] data)
        {
            return data.Length;
        }

        public IMessage ParseMessage(byte[] data)
        {
            var s = Encoding.ASCII.GetString(data);


            return new Resps.Message(s);
        }

        public bool SerializeIsSupported()
        {
            return false;
        }

        public byte[] SerializeMessage(IMessage data)
        {
            throw new NotImplementedException();
        }
    }

    public class ClientImpl : ABaseClient<MessageParser>
    {
        public ClientImpl(ICliConfig config) : base(config)
        {
        }

        public ClientImpl(string _ip, string _port) : base(_ip, _port)
        {
        }


    }

    public static class ChatTest
    {
        static ClientImpl ci;
        public static void Test(vhAPI api)
        {
            if (ci != null)
            {
                ci.DisposeClientTcp();
                ci = null;
            }

            ci = new ClientImpl("85.25.237.247", "7531");

            ci.OnConnect += () => ci.SendMessage(new vHackApi.Chat.Reqs.Join(api));

            ci.MessageReceived += (msg, err) =>
            {
                if (err != null)
                    System.Console.WriteLine("Error: {0}", err.Message);

                if (msg.Display().Contains("PRIVMSG " + vHackApi.Chat.Reqs.Message.vhackxy + " :"))
                    System.Console.WriteLine(msg.Display());
                else
                {
                    try
                    {
                        var req = new vHackApi.Chat.Reqs.Message(msg.Display(), api);
                        req.Validate();
                        ci.SendMessage(req);
                    }
                    catch (vHackApi.Chat.Reqs.Message.NotValidException)
                    {
                        System.Console.WriteLine(msg.Display());
                    }

                }
            };

            ci.OnConnectFailure += () => System.Console.WriteLine("Connection failure");
            ci.LogAction += (s, parms) => System.Console.WriteLine(s, parms);

            ci.CreateClientTcp();
            ci.ClientTcp.Connect(TimeSpan.Zero);
        }
    }
}