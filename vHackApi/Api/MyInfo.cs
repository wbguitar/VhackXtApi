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

        public long Id => (long)json["id"];
        public long IP => (long)json["ip"];
        public long Money => (long)json["money"];
        public long Internet => (long)json["inet"];
        public long HDD => (long)json["hdd"];
        public long CPU => (long)json["cpu"];
        public long RAM => (long)json["ram"];
        public long Firewall => (long)json["fw"];
        public long Antivirus => (long)json["av"];
        public long SDK => (long)json["sdk"];
        public long IPSpoofing => (long)json["ipsp"];
        public long Spam => (long)json["spam"];
        public long Scan => (long)json["scan"];
        public long Spyware => (long)json["adw"];
        /// <summary>
        /// Numbero of adware infections
        /// </summary>
        public long ActiveAdware => (long)json["actadw"];
        public long Netcoins => (long)json["netcoins"];
        public long Energy => (long)json["energy"];
        public long Score => (long)json["score"];
        //public long IP => (long)json["urmail"];
        //public long IP => (long)json["active"];
        public long Reputation => (long)json["elo"];
        //public string IP => (string)json["clusterID"];
        //public string IP => (string)json["position"];
        //public string IP => (string)json["syslog"];
        //public string IP => (string)json["lastcmsg"];
        public long Rank => (long)json["rank"];
        //public string IP => (string)json["event"];
        public long Packages => (long)json["bonus"];
        //public string IP => (string)json["mystery"];
        //public string IP => (string)json["vipleft"];
        public string Hash => (string)json["hash"];
        public string UHash => (string)json["uhash"];
        //public string IP => (string)json["use"];
        //public string IP => (string)json["tournamentActive"];
        public long Boost => (long)json["boost"];
        /// <summary>
        /// Number of active spyware on remote machines
        /// </summary>
        public string ActiveSpyware => (string)json["actspyware"];
        //public string IP => (string)json["tos"];
        //public string IP => (string)json["unreadmsg"];

    }
}
