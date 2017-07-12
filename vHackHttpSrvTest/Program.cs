using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using vHackApi.HTTP;
using vHackApi.Interfaces;

namespace vHackHttpSrvTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var srv = new vhApiServer(new vHackBot.Program.Config());
            //var srv = new ExampleHttpServer(6666);
            srv.listen();
        }
    }
}
