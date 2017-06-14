using System;
using System.Linq;
using vHackApi.Interfaces;
using System.Data.SQLite.Linq;
using System.Collections.Generic;

namespace vHackApi.Bot
{
    public class IPSelectorASAP : Singleton<IPSelectorASAP>, IIPselector
    {
        public IPs NextHackabletIp(IPersistanceMgr pm)
        {
            IEnumerable<IPs> ips = pm.ScannableIps();
            var first = ips
                 .Where(ip => ip.IP != "127.0.0.1") // filter dev ip
                 .OrderBy(ip => ip.Money)
                 .ThenByDescending(ip => ip.LastAttack)
                 .FirstOrDefault();

            return first;
        }
    }

    public class IPSelectorRandom : Singleton<IPSelectorRandom>, IIPselector
    {
        static Random r = new Random();
        public IPs NextHackabletIp(IPersistanceMgr pm)
        {
            IPs ret = null;
            foreach (var ip in pm.ScannableIps())
            {
                if (ip.IP == "127.0.0.1")
                    continue;

                ret = ip;
                if (r.Next(0, 100) > 50)
                    return ip;
            }

            return ret;

            // the below commented part is formally correct but throws exception, maybe dew to a sqlite EF driver bug
            // that's the reason for the less elegant solution used

            //var scannables = DbManager.ScannableIps()
            //     .Where(ip => ip.IP != "127.0.0.1")
            //     .ToList(); // filter dev ip

            //var i = r.Next(0, scannables.Count());
            //return scannables.ElementAt(i);
        }
    }
}