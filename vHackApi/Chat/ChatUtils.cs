using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;
using WB.Commons.Net.Bin;
using WB.Commons.Net.Bin.Interfaces;
using WB.IIIParty.Commons.Protocol;
using System.Linq;
using System.Web.UI;

namespace vHackApi.Chat
{
    public class vhChat: IDisposable
    {
        public enum Rule
        {
            None,
            Me,
            User,
            Mod,
            Admin,
            Vip,
            Bot
        }

        private vhAPI _api;
        private IConfig _cfg;

        private readonly Encoding _encoding = Encoding.ASCII;
        private static readonly object _semaphore = new object();

        public vhChat(IConfig cfg, vhAPI api)
        {
            _api = api;
            _cfg = cfg;
        }

        public event Action<Rule, string, string, string> PrivateMessage = (rule, email, nick, msg) => { };
        public event Action<string> MessageReceived = (msg) => { };
        private Action<string> _sendMessage = (s) => { };
        public void SendChat(string msg)
        {
            _sendMessage(msg);
        }

        

        public void Run()
        {
            var sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            var myId = string.Empty;
            var str2 = string.Empty;
            var connect = new Action(delegate{
                //sock.Connect("51.255.93.109", 7531);
                sock.Connect(_cfg.chatIp, _cfg.chatPort);

                str2 = "v[" + _cfg.chatUser;
                myId = _api.getStats(Stats.id).Result;
                //String str2 = "v[" + api.getUsername();
                var msg = ("NICK " + str2 + "\r\n");
                msg += ("USER " + myId + " 0 * : vHack XT@Android\r\n");

                sock.Send(_encoding.GetBytes(msg));
            });

            //connect();

            var vhackxy = "#vHackXT";

            var procLine = new Action<string>((readLine) =>
            {
                
                if (readLine.ToLower().StartsWith("ping "))
                {
                    String str3 = "PONG " + readLine.Substring(5) + "\r\n";

                    sock.Send(_encoding.GetBytes(str3));
                }
                if (readLine.Contains(":(channel is full) transfering you to #"))
                {

                }
                if (readLine.Contains(" 433 *"))
                {
                    var str = "NICK " + str2 + "_" + "\r\n";
                    sock.Send(_encoding.GetBytes(str));
                }
                if (readLine.Contains("376"))
                {
                    var str = "PRIVMSG vHackXTGuard :.join " + _api.getStats(Stats.id).Result + " " +
                              _api.getStats(Stats.hash).Result +
                              "\r\n";
                    sock.Send(_encoding.GetBytes(str));
                }
                if (readLine.Contains("PRIVMSG " + vhackxy + " :"))
                {
                    //System.Console.WriteLine(readLine);

                    
                    var m = Regex.Match(readLine, @"\:([^\!]*)\!([^\@]*)\@([^ ]*) PRIVMSG \#vHackXT \:([^\n]*)");
                    if (m.Success)
                    {
                        var nick = m.Groups[1].Value;
                        var id = m.Groups[2].Value;
                        var email = m.Groups[3].Value;
                        var rule = Rule.User;
                        if (email.Contains("mod.vhack.biz"))
                            rule = Rule.Mod;
                        else if (email.Contains("bot.vhack.biz"))
                            rule = Rule.Bot;
                        else if (email.Contains("admin.vhack.biz"))
                            rule = Rule.Admin;
                        else if (email.Contains("vip.vhack.biz"))
                            rule = Rule.Vip;

                        var pmsg = m.Groups.Cast<Group>().Last().Value;
                        var m1 = Regex.Match(pmsg, @"(\p{Cs})");
                        PrivateMessage(rule, $"{id}@{email}", nick, pmsg);
                    }
                    else
                    {
                        PrivateMessage(Rule.None, null, null, readLine);
                    }
                }
            });

            _sendMessage = (s) =>
            {
                sock.Send(_encoding.GetBytes("PRIVMSG " + vhackxy + " :" + s + "\r\n"));
                PrivateMessage(Rule.Me, myId, _cfg.chatUser, s);
            };

            var sleeps = new TimeSpan[]
            {
                TimeSpan.FromSeconds(10),
                TimeSpan.FromSeconds(30),
                TimeSpan.FromMinutes(1),
                TimeSpan.FromMinutes(5),
                TimeSpan.FromMinutes(10),
                TimeSpan.FromMinutes(30),
            };
            var sleepIdx = 0;
            Task.Run(() =>
            {
                while (run)
                {
                    lock (_semaphore)
                    {
                        try
                        {
                            if (!sock.Connected)
                                connect();

                            var bytes = new byte[1024];
                            var count = sock.Receive(bytes);

                            // conversion to get unicode chars (emoji and special chars)
                            var ubytes = Encoding.Convert(Encoding.UTF8, Encoding.Unicode, bytes);
                            var lines = Encoding.Unicode.GetString(ubytes);


                            foreach (var l in lines.Split(new[] { "\r\n" }, StringSplitOptions.None))
                            {
                                var line = l.Replace("\0", "").Trim();
                                if (string.IsNullOrEmpty(line))
                                    continue;

                                try
                                {
                                    MessageReceived(line);
                                    procLine(line);
                                }
                                catch (Exception exc)
                                {
                                    _cfg.logger.Log("Error parsing chat lines: {0}", exc.ToString());
                                }
                            }
                        }
                        catch (SocketException exc)
                        {
                            _cfg.logger.Log("Chat socket error: {0}", exc.ToString());

                            //if (exc.SocketErrorCode == SocketError.NetworkDown ||
                            //    exc.SocketErrorCode == SocketError.NetworkReset ||
                            //    exc.SocketErrorCode == SocketError.NetworkUnreachable ||
                            //    exc.SocketErrorCode == SocketError.TimedOut)
                            {
                                _cfg.logger.Log("Pausing for {0}", sleeps[sleepIdx]);
                                Thread.Sleep(sleeps[sleepIdx]);
                                sleepIdx += (sleepIdx >= sleeps.Length - 1) ? 0 : 1;
                            }

                        }
                        catch (Exception exc)
                        {
                            _cfg.logger.Log("Error in chat client: {0}", exc.ToString());
                        } 
                    }
                }
            });
        }

        private bool run = true;

        public void Dispose()
        {
            run = false;
        }
    }

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

        public static String Vhackxy { get; private set; } = "#vHackXT";
        //static BufferedWriter bufferedWriter;
        public static Client Client { get; private set; }

        public static void SendChatMessage(String message)
        {
            try
            {

                vhConsole.config.logger.Log("Sending: {0}", message);
                //bufferedWriter.write("PRIVMSG " + vhackxy + " :" + message + "\r\n");
                //bufferedWriter.flush();

                Client.Send("PRIVMSG " + Vhackxy + " :" + message + "\r\n");

            }
            catch (IOException e)
            {
                // TODO Auto-generated catch block
                vhConsole.config.logger.Log(e.StackTrace);
            }
        }

        static Thread _threadA;
        public static void ConnectToChat(vhAPI api)
        {
            _threadA = new Thread(() =>
            {
                try
                {

                    Client = new Client("51.255.93.109", 7531);
                   

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
                    var msg = $"NICK {str2}\r\nUSER {id} 0 * : vHack XT@Android\r\n";
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
            _threadA.Start();
            _threadA.Join();
        }

        private static void Client_OnResponse(Client.State s)
        {
            
        }

        private static int counter = 1;
        public static void RunChat(vhAPI api)
        {
            var sock = new Socket(SocketType.Stream, ProtocolType.Tcp);
            //sock.Connect("chat.vhackxt.com", 7531); // ip: 85.25.237.247
            sock.Connect("51.255.93.109", 7531);


            String str2 = "v[" + "wonderboy"; //api.getUsername();
            //String str2 = "v[" + api.getUsername();
            var msg = ("NICK " + str2 + "\r\n");
            msg += ("USER " + api.getStats(Stats.id).Result + " 0 * : vHack XT@Android\r\n");

            sock.Send(Encoding.ASCII.GetBytes(msg));

            var vhackxy = "#vHackXT";

            var procLine = new Action<string>((readLine) =>
            {
                if (readLine.ToLower().StartsWith("ping "))
                {
                    String str3 = "PONG " + readLine.Substring(5) + "\r\n";
                    System.Console.WriteLine($"{counter++} {str3}");
                    sock.Send(Encoding.ASCII.GetBytes(str3));
                }
                if (readLine.Contains(":(channel is full) transfering you to #"))
                {
                    
                }
                if (readLine.Contains(" 433 *"))
                {
                    var str = "NICK " + str2 + "_" + "\r\n";
                    sock.Send(Encoding.ASCII.GetBytes(str));
                    System.Console.WriteLine($"{counter++} {str}");
                }
                if (readLine.Contains("376"))
                {
                    var str = "PRIVMSG vHackXTGuard :.join " + api.getStats(Stats.id).Result + " " + api.getStats(Stats.hash).Result +
                              "\r\n";
                    sock.Send(Encoding.ASCII.GetBytes(str));
                    System.Console.WriteLine($"{counter++} {str}");
                }
                if (readLine.Contains("PRIVMSG " + vhackxy + " :"))
                {
                    System.Console.WriteLine(readLine);

                    var m = Regex.Match(readLine, @"\:v\[(\w*)!(\d*)@([\d|\w]*\.)*IP PRIVMSG \#vHackXT \:(.*)");
                    if (m.Success)
                    {
                        var nick = m.Groups[1].Value;
                        var id = m.Groups[2].Value;
                        var pmsg = m.Groups[4].Value;
                        PrivateMessage(Convert.ToInt16(id), nick, pmsg);
                    }
                }
            });

            while (true)
            {
                var bytes = new byte[1024];
                var count = sock.Receive(bytes);
                var lines = Encoding.ASCII.GetString(bytes, 0, count);

                foreach (var line in lines.Split(new[] { "\r\n" }, StringSplitOptions.None))
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    System.Console.ForegroundColor = ConsoleColor.Green;
                    System.Console.WriteLine(line.Trim());
                    System.Console.ForegroundColor = ConsoleColor.Blue;
                    procLine(line.Trim());
                }
            }
        }

        public static event Action<int, string, string> PrivateMessage = (i, nick, msg) => { };

        static async void procLine(string readLine, String str2, vhAPI api)
        {
            if (readLine.ToLower().StartsWith("ping "))
            {
                String str3 = "PONG " + readLine.Substring(5) + "\r\n";
                Client.Send(str3);
            }
            if (readLine.Contains(":(channel is full) transfering you to #"))
            {
                Vhackxy = readLine
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
            if (readLine.Contains("PRIVMSG " + Vhackxy + " :"))
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

            ci = new ClientImpl("51.255.93.109", "7531");

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