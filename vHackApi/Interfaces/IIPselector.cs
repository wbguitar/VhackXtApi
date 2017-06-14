using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vHackApi.Interfaces
{
    public interface IIPselector
    {
        IPs NextHackabletIp(IPersistanceMgr pm);
    }
}
