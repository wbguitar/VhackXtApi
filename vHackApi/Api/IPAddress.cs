using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vHackApi.Api
{
    public class IPAddress
    {
        public string IP { get; private set; }
        public string Attacked { get; private set; }
        public string FirewallLevel { get; private set; }

        public IPAddress(string[] arr)
        {
            IP = arr[0].Split(':')[1];
            FirewallLevel = arr[1].Split(':')[1];
            Attacked = arr[2].Split(':')[1];
        }
    }
}
