using System.Collections.Generic;

namespace vHackApi.Interfaces
{
    public interface IPersistanceMgr
    {
        bool AddAttack(string iP, Attacks att);

        IPs AddIp(IPs dbIp);

        IPs GetIp(string iP);

        bool IpExist(string iP);

        IEnumerable<IPs> ScannableIps();

        void Update();

        bool UpdateIp(IPs ip);
    }
}