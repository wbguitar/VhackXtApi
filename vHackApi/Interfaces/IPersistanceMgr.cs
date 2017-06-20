using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;

namespace vHackApi.Interfaces
{
    public interface IPersistanceMgr
    {
        bool AddAttack(string iP, Attacks att);

        IPs AddIp(IPs dbIp);

        IPs GetIp(string iP);

        IEnumerable<IPs> GetIps();

        bool IpExist(string iP);

        IEnumerable<IPs> ScannableIps();

        void Update();

        bool UpdateIp(IPs ip);
    }
}