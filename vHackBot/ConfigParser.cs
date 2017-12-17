using System;
using System.Net;
using Newtonsoft.Json.Linq;
using vHackApi.Interfaces;

namespace vHackBot
{
    class ConfigParser : IConfigParser
    {
        public class Config : IConfig
        {
            public Config() { }
            public Config(JObject json)
            {
                // {
                //     "waitstep": 2000,
                //     "winchance": 75,
                //     "maxFirewall": 15000,
                //     "finishAllFor": 2000,
                //     "maxAntivirus": 15000,
                //     "hackIfNotAnonymous": true,
                // }


                waitstep = getValue(json, "waitstep", -1);
                winchance = getValue(json, "winchance", -1);
                maxFirewall = getValue(json, "maxFirewall", -1);
                finishAllFor = getValue(json, "finishAllFor", -1);
                maxAntivirus = getValue(json, "maxAntivirus", -1);

                hackIfNotAnonymous = getValue(json, "hackIfNotAnonymous", false);
                ipAttackPaused = getValue(json, "ipAttackPaused", false);
                ipScannerPaused = getValue(json, "ipScannerPaused", false);
                hackBotNetPaused = getValue(json, "hackBotNetPaused", false);
                hackTheDevPaused = getValue(json, "hackTheDevPaused", false);

                pcOrAttack = getValue(json, "pcOrAttack", 13);

                getImgBy = getValue(json, "getImgBy", 0);
            }

            public override string ToString()
            {
                return $@"{{
    waitstep: {waitstep},
    winchance: {winchance},
    maxFirewall: {maxFirewall},
    finishAllFor: {finishAllFor},
    maxAntivirus: {maxAntivirus},
    hackIfNotAnonymous: {hackIfNotAnonymous},
    ipAttackPaused: {ipAttackPaused},
    ipScannerPaused: {ipScannerPaused},
    hackBotNetPaused: {hackBotNetPaused},
    hackTheDevPaused: {hackTheDevPaused},
    getImgBy: {getImgBy},
}}";
            }

            T getValue<T>(JObject json, string key, T def)
            {
                var tok = json.GetValue(key);
                if (tok == null)
                    return def;
                return tok.Value<T>();
            }

            public string rootUrl { get { throw new NotSupportedException(); } }

            public string username { get { throw new NotSupportedException(); } }

            public string password { get { throw new NotSupportedException(); } }

            public string tessdata => null;

            public int waitstep { get; private set; }

            public int winchance { get; private set; }

            public int maxFirewall { get; private set; }

            public int maxAntivirus { get; private set; }

            public bool safeScan { get { throw new NotSupportedException(); } }

            public int finishAllFor { get; private set; }

            public bool hackIfNotAnonymous { get; private set; }

            public int pcOrAttack { get; set; }
            public int getImgBy { get; set; }

            public TimeSpan hackDevPolling { get { throw new NotSupportedException(); } }

            public TimeSpan hackBotnetPolling { get { throw new NotSupportedException(); } }

            public string dbConnectionString { get { throw new NotSupportedException(); } }
            public string chatIp { get { throw new NotSupportedException(); } }
            public int chatPort { get { throw new NotSupportedException(); } }
            public string chatUser { get { throw new NotSupportedException(); } }

            public ILogger logger { get { throw new NotSupportedException(); } }

            public IIPselector ipSelector { get { throw new NotSupportedException(); } }

            public IUpgradeStrategy upgradeStrategy { get { throw new NotSupportedException(); } }

            public IPersistanceMgr persistanceMgr { get { throw new NotSupportedException(); } }

            public IWebProxy proxy { get { throw new NotSupportedException(); } }

            public string vhServerHost { get; private set; }

            public int vhServerPort { get; private set; }

            public bool ipAttackPaused { get; set; }

            public bool ipScannerPaused { get; set; }

            public bool hackTheDevPaused { get; set; }

            public bool hackBotNetPaused { get; set; }

            public TimeSpan ipAttackPolling { get { throw new NotSupportedException(); } }

            public TimeSpan ipScannerPolling { get { throw new NotSupportedException(); } }
        }
        public event Action<IConfig> ConfigParsed = (cfg) => { };
        public event Action<Exception> ParseError = (e) => { };

        public void ParseConfig(JObject config)
        {
            try
            {
                var cfg = new Config(config);
                ConfigParsed(cfg);
            }
            catch (Exception e)
            {
                ParseError(e);
            }
        }
    }
}