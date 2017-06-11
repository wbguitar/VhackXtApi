using System;
using System.Collections.Generic;
using System.Linq;
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

        string dbConnectionString { get; }

        Tasks[] updates { get; }

        ILogger logger { get; }
    }
}
