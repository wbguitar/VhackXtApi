using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Interfaces;

using Griffin.Networking.Buffers;
using Griffin.Networking.Messaging;
using Griffin.Networking.Protocol.Http;
using Griffin.Networking.Protocol.Http.Protocol;
using Griffin.Networking.Servers;
using System.Net;
using Newtonsoft.Json;
using System.Threading;
using Antlr3.ST;
using Antlr3.ST.Language;
using System.Web.Script.Serialization;
using vHackApi.Bot;

namespace vHackApi.HTTP
{
    public class vhApiServer : HttpServer
    {
        vhAPI api;
        IConfig config;
        public vhApiServer(IConfig cfg) : base(cfg.vhServerHost, cfg.vhServerPort)
        {
            config = cfg;

            try
            {
                var builder = new vhAPIBuilder().useConfig(cfg);
                api = builder.getAPI();
            }
            catch (Exception exc)
            {
                config.logger.Log($"Error creating vhAPI: {exc.Message}");
            }
        }

        #region GET
        public override async void handleGETRequest(HttpProcessor p)
        {
            if (p.http_url == "/hack")
                await handleHack(p);
            else if (p.http_url == "/info")
            {
                await handleGetInfo(p);
            }
            else
                await handleError(p);
        }

        private async Task handleHack(HttpProcessor p)
        {
            p.writeSuccess();
            await p.outputStream.WriteLineAsync("nack!");
        }

        private async Task handleError(HttpProcessor p)
        {
            //await p.outputStream.WriteLineAsync("Wrong url format");
            p.writeFailure();
            await p.outputStream.WriteLineAsync("Wrong url format");
        }

        private async Task handleGetInfo(HttpProcessor p)
        {
            var info = await MyInfo.Fetch(api.getConsole());
            p.writeSuccess();
            await p.outputStream.WriteLineAsync(info.Json.ToString());
        }

        #endregion


        public IConfigParser ConfigParser { get; set; }
        
        public override void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
        {
            if (p.http_url == "/config")
            {
                try
                {
                    var txt = inputData.ReadToEnd();

                    var cfg = JObject.Parse(txt);

                    if (ConfigParser != null)
                        ConfigParser.ParseConfig(cfg);

                    p.writeSuccess();
                    p.outputStream.WriteLine("OK");
                }
                catch (Exception exc)
                {
                    p.writeSuccess();
                    //await p.outputStream.WriteLineAsync($"Error parsing object: {exc.Message}");
                    //await p.outputStream.FlushAsync();
                    p.outputStream.WriteLine($"Error parsing object: {exc.Message}");
                    p.outputStream.Flush();

                }
            }
            else
            {
                p.writeSuccess();
                //await p.outputStream.WriteLineAsync("Wrong url format");
                //await p.outputStream.FlushAsync();
                p.outputStream.WriteLine("Wrong url format");
                p.outputStream.Flush();
            }
        }
    }

    namespace Griffin
    {
        public class vhApiServer: MessagingServer
        {
            private class ServiceFactory : IServiceFactory
            {
                public INetworkService CreateClient(EndPoint remoteEndPoint)
                {
                    return new Service();
                }
            }

            // and the handler
            private class Service : HttpService
            {

                private static readonly BufferSliceStack Stack = new BufferSliceStack(50, 32000);

                public Service()
                    : base(Stack)
                { }


                public override void Dispose()
                {
                }

                public override void OnRequest(IRequest request)
                {
                    Send(RouteManager.Default.GetResponse(request));
                }
            }

            private class RouteManager: Singleton<RouteManager>
            {
                IConfig config;
                vhAPI api;
                public IConfigParser configParser;

                JObject jsonInfo;
                Timer timerInfo;

                public void Init(IConfig cfg, IConfigParser parser)
                {
                   

                    config = cfg;
                    configParser = parser; 

                    try
                    {
                        var builder = new vhAPIBuilder().useConfig(cfg);
                        api = builder.getAPI();

                        timerInfo = new Timer((o) =>
                        {
                            var info = MyInfo.Fetch(api.getConsole()).Result;
                            lock(this)
                            {
                                jsonInfo = info.Json;
                            }
                        }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
                    }
                    catch (Exception exc)
                    {
                        config.logger.Log($"Error creating vhAPI: {exc.Message}");
                        
                    }
                }

                void setResponse(ref IResponse response, string json)
                {
                    response.Body = new MemoryStream();
                    response.ContentType = "text/json";
                    var buffer = Encoding.UTF8.GetBytes(json.ToString());
                    response.Body.Write(buffer, 0, buffer.Length);
                    response.Body.Position = 0;
                }

                public IResponse GetResponse(IRequest request)
                {
                    //var response = request.CreateResponse(HttpStatusCode.OK, "Welcome");

                    //response.Body = new MemoryStream();
                    //response.ContentType = "text/plain";
                    //var buffer = Encoding.UTF8.GetBytes("Hello world");
                    //response.Body.Write(buffer, 0, buffer.Length);
                    //response.Body.Position = 0;
                    IResponse response = null;
                    var path = request.Uri.LocalPath.ToLower();
                    var mainPage = $"http://{config.vhServerHost}:{config.vhServerPort}";
                    if (request.Method == "GET")
                    {
                        if (path == "/hack")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "Nack");
                        }
                        else if (path == "/")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "Index");
                            var htmlTmpl = new StreamReader(File.OpenRead("HTML/index.html")).ReadToEnd();
                            var template = new Antlr3.ST.StringTemplate(htmlTmpl, typeof(TemplateLexer));
                            

                            template.SetAttribute("config_form", $"{mainPage}/config_form");
                            template.SetAttribute("iplist", $"{mainPage}/iplist");

                            var html = template.ToString();
                            setResponse(ref response, html);
                            response.ContentType = "text/html";
                        }
                        else if (path == "/info")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "GET Info request");
                            //var info = MyInfo.Fetch(api.getConsole()).Result;

                            lock (this)
                            {
                                //response.Body = new MemoryStream();
                                //response.ContentType = "text/json";
                                //var buffer = Encoding.UTF8.GetBytes(jsonInfo.ToString());
                                //response.Body.Write(buffer, 0, buffer.Length);
                                //response.Body.Position = 0;



                                setResponse(ref response, jsonInfo.ToString());
                            }
                        }
                        else if (path == "/config")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "GET Config request");
                            var json = $@"
{{
    ""waitstep"": ""{config.waitstep}"",
    ""winchance"": ""{config.winchance}"",
    ""maxFirewall"": ""{config.maxFirewall}"",
    ""finishAllFor"": ""{config.finishAllFor}"",
    ""maxAntivirus"": ""{config.maxAntivirus}"",
    ""hackIfNotAnonymous"": ""{config.hackIfNotAnonymous}""
    ""ipAttackPaused"": ""{config.ipAttackPaused}""
    ""ipScannerPaused"": ""{config.ipScannerPaused}""
    ""hackBotNetPaused"": ""{config.hackBotNetPaused}""
    ""hackTheDevPaused"": ""{config.hackTheDevPaused}""
}}";

                            setResponse(ref response, json);
                        }
                        else if (path == "/config_form")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "GET Config Form request");
                            var htmlTmpl = new StreamReader(File.OpenRead("HTML/config.html")).ReadToEnd();
                            //response.Body = File.OpenRead("HTML/config.html");
                            var template = new Antlr3.ST.StringTemplate(htmlTmpl, typeof(TemplateLexer));
                            template.SetAttribute("mainPage", mainPage);

                            template.SetAttribute("waitstep", config.waitstep);
                            template.SetAttribute("winchance", config.winchance);
                            template.SetAttribute("maxFirewall", config.maxFirewall);
                            template.SetAttribute("maxAntivirus", config.maxAntivirus);
                            template.SetAttribute("finishAllFor", config.finishAllFor);
                            template.SetAttribute("hackIfNotAnonymous", config.hackIfNotAnonymous ? "checked" : "uchecked");

                            template.SetAttribute("ipAttackPaused", config.ipAttackPaused ? "checked" : "uchecked");
                            template.SetAttribute("ipScannerPaused", config.ipScannerPaused ? "checked" : "uchecked");
                            template.SetAttribute("hackBotNetPaused", config.hackBotNetPaused ? "checked" : "uchecked");
                            template.SetAttribute("hackTheDevPaused", config.hackTheDevPaused ? "checked" : "uchecked");


                            var html = template.ToString();

                            setResponse(ref response, html);
                            response.ContentType = "text/html";
                        }
                        else if (path == "/iplist")
                        {
                            var now = DateTime.Now;
                            var ips = config.persistanceMgr.GetIps()
                                .Where(ip => now - ip.LastAttack > TimeSpan.FromHours(1))
                                .OrderByDescending(ip => ip.Money)
                                .ThenBy(ip => ip.LastAttack)
                                .Take(20);

                            var iplistTpl = new Antlr3.ST.StringTemplate(new StreamReader(File.OpenRead("HTML/iplist.html")).ReadToEnd()
                                , typeof(TemplateLexer));
                            

                            var sb = new StringBuilder();
                            var ipTplStr = new StreamReader(File.OpenRead("HTML/iptemplate.html")).ReadToEnd();
                            foreach (var ip in ips)
                            {
                                var ipTpl = new Antlr3.ST.StringTemplate(ipTplStr, typeof(TemplateLexer));
                                ipTpl.SetAttribute("ip", ip.IP);
                                ipTpl.SetAttribute("lastUpd", ip.LastUpdate.ToString("ddd dd MMM yyyy   hh:mm:ss"));
                                ipTpl.SetAttribute("money", string.Format("{0:C}", ip.Money));
                                var s = ipTpl.ToString();
                                sb.Append(s);
                            }

                            var iplist = sb.ToString();
                            iplistTpl.SetAttribute("iptable", iplist);
                            iplistTpl.SetAttribute("mainPage", mainPage);

                            response = request.CreateResponse(HttpStatusCode.OK, "GET IP list request");
                            setResponse(ref response, iplistTpl.ToString());
                            response.ContentType = "text/html";
                        }
                       
                    }
                    else if (request.Method == "POST")
                    {
                        if (path == "/config")
                        {

                            try
                            {
                                using (var rd = new StreamReader(request.Body))
                                {
                                    var json = rd.ReadToEnd();
                                    if (request.ContentType == "application/x-www-form-urlencoded")
                                    {
                                        var dict = System.Web.HttpUtility.ParseQueryString(json);
                                        json = new JavaScriptSerializer().Serialize(
                                                     dict.Keys.Cast<string>()
                                                         .ToDictionary(k => k, k => dict[k]));


                                    }

                                    var cfg = JObject.Parse(json);
                                    if (cfg["hackIfNotAnonymous"] != null)
                                    {
                                        if ((string)cfg["hackIfNotAnonymous"] == "on")
                                            cfg["hackIfNotAnonymous"] = "true";
                                    }

                                    if (cfg["ipAttackPaused"] != null)
                                    {
                                        if ((string)cfg["ipAttackPaused"] == "on")
                                        {
                                            cfg["ipAttackPaused"] = "true";
                                            IPAttack.Instance.Pause();
                                        }
                                        
                                    }
                                    else
                                    {
                                        cfg["ipAttackPaused"] = "false";
                                        IPAttack.Instance.Resume();
                                    }

                                    if (cfg["ipScannerPaused"] != null)
                                    {
                                        if ((string)cfg["ipScannerPaused"] == "on")
                                        {
                                            cfg["ipScannerPaused"] = "true";
                                            IPScanner.Instance.Pause();
                                        }
                                       
                                    }
                                    else
                                    {
                                        cfg["ipScannerPaused"] = "false";
                                        IPScanner.Instance.Resume();
                                    }

                                    if (cfg["hackBotNetPaused"] != null)
                                    {
                                        if ((string)cfg["hackBotNetPaused"] == "on")
                                        {
                                            cfg["hackBotNetPaused"] = "true";
                                            HackBotNet.Instance.Pause();
                                        }
                                        
                                    }
                                    else
                                    {
                                        cfg["hackBotNetPaused"] = "false";
                                        HackBotNet.Instance.Resume();
                                    }

                                    if (cfg["hackTheDevPaused"] != null)
                                    {
                                        if ((string)cfg["hackTheDevPaused"] == "on")
                                        {
                                            cfg["hackTheDevPaused"] = "true";
                                            HackTheDev.Instance.Pause();
                                        }
                                        
                                    }
                                    else
                                    {
                                        cfg["hackTheDevPaused"] = "false";
                                        HackTheDev.Instance.Resume();
                                    }

                                    if (configParser != null)
                                        configParser.ParseConfig(cfg);
                                }

                                response = request.CreateResponse(HttpStatusCode.OK, "POST config request");
                                response.Redirect($"http://{config.vhServerHost}:{config.vhServerPort}/config_form");
                            }
                            catch (Exception exc)
                            {
                                response = request.CreateResponse(HttpStatusCode.BadRequest, $"Error parsing content: {exc.Message}");
                            }
                        }
                        else if (path == "/attackip")
                        {
                            using (var rd = new StreamReader(request.Body))
                            {
                                var data = rd.ReadToEnd();
                                var dict = System.Web.HttpUtility.ParseQueryString(data);
                                var ip = dict["ip"];
                                var res = api.getConsole().AttackIp(ip).Result;

                                response = request.CreateResponse(HttpStatusCode.OK, "POST config request");
                                response.Redirect($"http://{config.vhServerHost}:{config.vhServerPort}/iplist");
                            }
                        }
                    }
                    else
                    {
                        config.logger.Log("Only GET and POST method are supported");
                        response = request.CreateResponse(HttpStatusCode.BadRequest, $"Unsupported method {request.Method}");
                    }

                    if (response == null)
                        response = request.CreateResponse(HttpStatusCode.BadRequest, $"Wrong {request.Method} path: {path}");

                    return response;
                }


            }

            IConfig config;
            public vhApiServer(IConfig cfg, IConfigParser parser) 
                : base(new ServiceFactory(), new MessagingServerConfiguration(new HttpMessageFactory()))
            {
                RouteManager.Default.Init(cfg, parser);
                config = cfg;
            }

            public void Listen()
            {
                //Listen(System.Net.IPAddress.Parse(config.vhServerHost), config.vhServerPort);
                Listen(System.Net.IPAddress.Any, config.vhServerPort);
            }

            public void Listen(System.Net.IPAddress ip, int port)
            {
                try
                {
                    var ep = new IPEndPoint(ip, port);
                    base.Start(ep);
                    config.logger.Log($"vhServer started listening {ep.ToString()}");
                }
                catch (Exception exc)
                {
                    config.logger.Log($"Couldn't start vhServer on endpoint {ip.ToString()}:{port}: {exc.Message}");
                }
            }

            
        }
    }
}
