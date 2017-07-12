using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using vHackApi.HTTP.Griffin;
using vHackApi.Interfaces;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace vHackHttpSrvTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var cfg = new vHackBot.Program.Config();

            //var srv = new vhApiServer(cfg);
            ////var srv = new ExampleHttpServer(6666);
            //srv.listen();

            var srv = new vhApiServer(cfg, new Parser());
            srv.Listen(IPAddress.Any, 6661);
            Thread.Sleep(Timeout.Infinite);
        }
    }

    class Parser: IConfigParser
    {
        public void ParseConfig(JObject config)
        {
            Console.WriteLine(config.ToString());
        }
    }
}
