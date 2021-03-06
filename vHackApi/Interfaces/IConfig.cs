using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;

namespace vHackApi.Interfaces
{
    public interface IConfig
    {
        string rootUrl { get; }
        string username { get; }
        string password { get; }
        string tessdata { get; }
        int waitstep { get; }
        int winchance { get; }
        int maxFirewall { get; }
        int maxAntivirus { get; }
        bool safeScan { get; }
        int finishAllFor { get; }

        string vhServerHost { get; }
        int vhServerPort { get; }

        bool hackIfNotAnonymous { get; }

        bool ipAttackPaused { get; set; }
        bool ipScannerPaused { get; set; }
        bool hackTheDevPaused { get; set; }
        bool hackBotNetPaused { get; set; }
        int pcOrAttack { get; set; }


        int getImgBy { get; set; } // 0 = score, 1 = reputation

        TimeSpan hackDevPolling { get; }
        TimeSpan hackBotnetPolling { get; }

        TimeSpan ipAttackPolling { get; }
        TimeSpan ipScannerPolling { get; }

        string dbConnectionString { get; }

        string chatIp { get; }
        int chatPort { get; }
        string chatUser { get; }


        ILogger logger { get; }

        IIPselector ipSelector { get; }

        IUpgradeStrategy upgradeStrategy { get; }

        IPersistanceMgr persistanceMgr { get; }
       
        IWebProxy proxy { get; }
    }

    public interface IConfigParser
    {
        void ParseConfig(JObject config);
    }
}
