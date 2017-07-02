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
        string username { get; }
        string password { get; }
        string tessdata { get; }
        int waitstep { get; }
        int winchance { get; }
        int maxFirewall { get; }
        int maxAntivirus { get; }
        bool safeScan { get; }
        int finishAllFor { get; }

        TimeSpan hackDevPolling { get; }
        TimeSpan hackBotnetPolling { get; }

        string dbConnectionString { get; }

        ILogger logger { get; }

        IIPselector ipSelector { get; }

        IUpgradeStrategy upgradeStrategy { get; }

        IPersistanceMgr persistanceMgr { get; }
       
        IWebProxy proxy { get; }
    }
}
