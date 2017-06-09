using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vHackApi.Interfaces
{
    public interface IConfig
    {
        string username { get; }
        string password { get; }
        string tessdata { get; }
        int waitstep { get; }
        int winchance { get; }

        Tasks[] updates { get; }

        ILogger logger { get; }
    }
}
