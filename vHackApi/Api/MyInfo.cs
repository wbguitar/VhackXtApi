using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Console;

namespace vHackApi.Api
{
    /// <summary>
    /// Json data:
    /// {{
    ///   "id": "169117",
    ///   "money": "1061867142",
    ///   "ip": "18.182.12.23",
    ///   "inet": "10",
    ///   "hdd": "10",
    ///   "cpu": "10",
    ///   "ram": "14",
    ///   "fw": "8693",
    ///   "av": "7925",
    ///   "sdk": "10974",
    ///   "ipsp": "6963",
    ///   "spam": "6408",
    ///   "scan": "8165",
    ///   "adw": "4510",
    ///   "actadw": "",
    ///   "netcoins": "20205",
    ///   "energy": "4860487904",
    ///   "score": "203188",
    ///   "urmail": "1",
    ///   "active": "1",
    ///   "elo": "5940",
    ///   "clusterID": null,
    ///   "position": null,
    ///   "syslog": null,
    ///   "lastcmsg": "0",
    ///   "rank": 96,
    ///   "event": "2",
    ///   "bonus": "450",
    ///   "mystery": "0",
    ///   "vipleft": "OFF",
    ///   "hash": "788ca1303012981e1328f0877bde9b21",
    ///   "uhash": "be577c6902613ec60e49d85997c3085ce9cc788d24a16501059f9560d5a705b6",
    ///   "use": "0",
    ///   "tournamentActive": "2",
    ///   "boost": "637",
    ///   "actspyware": "11",
    ///   "tos": "1",
    ///   "unreadmsg": "1"
    /// }}
    /// </summary>
    public class MyInfo
    {
        private JObject json;
        private MyInfo() { }

        public static async Task<MyInfo> Fetch(vhConsole c)
        {
            var info = new MyInfo();
            
            info.json = await c.MyInfo();
            if (info.json == null)
                return null;

            return info;
        }

        public int Id => (int)json["id"];
        public int IP => (int)json["ip"];
        public int Money => (int)json["money"];
        public int Internet => (int)json["inet"];
        public int HDD => (int)json["hdd"];
        public int CPU => (int)json["cpu"];
        public int RAM => (int)json["ram"];
        public int Firewall => (int)json["fw"];
        public int Antivirus => (int)json["av"];
        public int SDK => (int)json["sdk"];
        public int IPSpoofing => (int)json["ipsp"];
        public int Spam => (int)json["spam"];
        public int Scan => (int)json["scan"];
        public int Spyware => (int)json["adw"];
        /// <summary>
        /// Numbero of adware infections
        /// </summary>
        public int ActiveAdware => (int)json["actadw"];
        public int Netcoins => (int)json["netcoins"];
        public int Energy => (int)json["energy"];
        public int Score => (int)json["score"];
        //public int IP => (int)json["urmail"];
        //public int IP => (int)json["active"];
        public int Reputation => (int)json["elo"];
        //public string IP => (string)json["clusterID"];
        //public string IP => (string)json["position"];
        //public string IP => (string)json["syslog"];
        //public string IP => (string)json["lastcmsg"];
        public int Rank => (int)json["rank"];
        //public string IP => (string)json["event"];
        public int Packages => (int)json["bonus"];
        //public string IP => (string)json["mystery"];
        //public string IP => (string)json["vipleft"];
        public string Hash => (string)json["hash"];
        public string UHash => (string)json["uhash"];
        //public string IP => (string)json["use"];
        //public string IP => (string)json["tournamentActive"];
        public int Boost => (int)json["boost"];
        /// <summary>
        /// Number of active spyware on remote machines
        /// </summary>
        public string ActiveSpyware => (string)json["actspyware"];
        //public string IP => (string)json["tos"];
        //public string IP => (string)json["unreadmsg"];

    }
}
