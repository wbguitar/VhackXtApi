using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.HTTP
{
    public class vhApiServer : HttpServer
    {
        vhAPI api;
        IConfig config;
        public vhApiServer(IConfig cfg, int port) : base(port)
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

        public interface IConfigParser
        {
            void ParseConfig(JObject config);
        }

        public IConfigParser ConfigParser { get; set; }
        
        public override async void handlePOSTRequest(HttpProcessor p, StreamReader inputData)
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
                    await p.outputStream.WriteLineAsync("OK");
                }
                catch (Exception exc)
                {
                    p.writeFailure();
                    await p.outputStream.WriteLineAsync($"Error parsing object: {exc.Message}");
                    await p.outputStream.FlushAsync();
                }
            }
            else
            {
                p.writeFailure();
                await p.outputStream.WriteLineAsync("Wrong url format");
                await p.outputStream.FlushAsync();
            }
        }
    }
}
