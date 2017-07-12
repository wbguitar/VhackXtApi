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
                    if (request.Method == "GET")
                    {
                        if (path == "/hack")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "Nack");
                        }
                        else if (path == "/info")
                        {
                            response = request.CreateResponse(HttpStatusCode.OK, "GET Info request");
                            //var info = MyInfo.Fetch(api.getConsole()).Result;

                            lock (this)
                            {
                                response.Body = new MemoryStream();
                                response.ContentType = "text/json";
                                var buffer = Encoding.UTF8.GetBytes(jsonInfo.ToString());
                                response.Body.Write(buffer, 0, buffer.Length);
                                response.Body.Position = 0; 
                            }
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
                                    var cfg = JObject.Parse(rd.ReadToEnd());

                                    if (configParser != null)
                                        configParser.ParseConfig(cfg);
                                }

                                response = request.CreateResponse(HttpStatusCode.OK, "POST config request");
                            }
                            catch (Exception exc)
                            {
                                response = request.CreateResponse(HttpStatusCode.BadRequest, $"Error parsing content: {exc.Message}");
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
                Listen(System.Net.IPAddress.Parse(config.vhServerHost), config.vhServerPort);
            }

            public void Listen(System.Net.IPAddress ip, int port)
            {
                var ep = new IPEndPoint(ip, port);
                base.Start(ep);
                config.logger.Log($"vhServer started listening {ep.ToString()}");
            }

            
        }
    }
}
