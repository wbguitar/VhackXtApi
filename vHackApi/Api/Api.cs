using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Api
{
    public enum Stats
    {
        money, ip, inet, hdd, cpu, ram, fw, av, sdk, ipsp, spam, scan, adw, netcoins, bonus, elo, hash, id, uhash, score, boost, clusterID, position, rank, actspyware
    }

    public class vhAPI
    {
        IConfig config;
        protected string userHash;
        private JObject stats = null;

        vhConsole console;
        public vhConsole getConsole()
        {
            if (console == null)
                 console = new vhConsole(config, userHash);

            return console;
        }

        // public UpgradeManager getUpgradeManager() {
        //     UpgradeManager manager = new UpgradeManager(username, password, userHash);
        //     return manager;
        // }	
        // public SpywareManager getSpywareManager() {
        //     SpywareManager manager = new SpywareManager(username, password, userHash);
        //     return manager;
        // }

        // public BotnetManager getBotnetManager() {
        //     BotnetManager manager = new BotnetManager(username, password, userHash);
        //     return manager;
        // }

        // public ClusterManager getClusterManager() {
        //     ClusterManager manager = new ClusterManager(username, password, userHash);
        //     return manager;
        // }


        //public PackageManager getPackageManager() {
        //    PackageManager packageOpener = new PackageManager(username, password, userHash, this);
        //    return packageOpener;
        //}

        public async Task<string> getStats(Stats stat)
        {
            await fetchStats();
            var val = (string)stats.GetValue(stat.ToString());
            return val;
        }

        public async Task<string> getCachedStats(Stats stat)
        {
            if (stats == null)
            {
                await fetchStats();
            }
            return (string)stats.GetValue(stat.ToString());
        }

        public async Task fetchStats()
        {

            JObject stats1 = await vhUtils.JSONRequest("user::::pass::::uhash", config.username + "::::" + config.password + "::::" + userHash, "vh_update.php");
            try
            {
                Thread.Sleep(vhConsole.WaitStep);
            }
            catch (ThreadInterruptedException e)
            {
                Debug.Print(e.StackTrace);
            }
            //Utils.JSONRequest("user::::pass::::uhash", username + "::::" + password + "::::" + userHash, "vh_update.php");
            stats = stats1;
        }

        public string getUsername()
        {
            return config.username;
        }

        public string getPassword()
        {
            return config.password;
        }

        //public Chat getChat() {
        //    Chat chat = new Chat();
        //    return chat;
        //}

        public vhAPI(IConfig cfg)
        {
            config = cfg;
            userHash = getStats(Stats.uhash).Result;
        }

        //@Deprecated
        //public vHackAPI getAPI() {
        //    return this;
        //}

        public string humanizeString(string unumanized)
        {
            switch (unumanized)
            {
                case "fw":
                    return "Firewall";
                case "av":
                    return "Antivirus";
                case "ipsp":
                    return "IP-Spoofing";
                case "adw":
                    return "AdWare";
                case "scan":
                    return "Scan";
                case "inet":
                    return "Internet";
                case "money":
                    return "Money";
                case "hdd":
                    return "HDD";
                case "cpu":
                    return "CPU";
                case "netcoins":
                    return "Netcoins";
                case "ip":
                    return "IP";
                case "ram":
                    return "RAM";
                case "sdk":
                    return "SDK";
                case "spam":
                    return "Spam";
                case "bonus":
                    return "Packages";
                case "elo":
                    return "Rank";
                case "hash":
                    return "Hash";
                case "id":
                    return "ID";
                case "btntpc":
                    return "Botnet PC";
                case "boost":
                    return "Booster";
                default:
                    return null;
            }

        }

        public async Task<JObject> botnetserverinfo()
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                 config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                 "vh_botnetInfo.php");
        }

        public async Task<JObject> botnetInfo(string userHash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash", 
                config.username + "::::" + config.password + "::::" + userHash,
                "vh_botnetInfo.php"); ;
        }

        public async Task<JObject> upgradeComputer(string userHash, int i)
        {
            /*
             * Supplied with a bot net computer ID, upgrade the computer.
             *  :param id: bot net computer id
             *  :return: str
             *  containing: 
             *  "money":"3479746","old":"13","costs":"4100000","lvl":"41","mm":"7579746","new":"42","strength":"42"}
             */

            return await vhUtils.JSONRequest("user::::pass::::uhash::::bID",
                              config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + i.ToString(),
                              "vh_upgradeBotnet.php"); ;
        }

        public async Task attackbotnetserver(string userhash = null)
        {
            if (string.IsNullOrEmpty(userhash))
                userhash = vhConsole.uHash;

            JObject response = null;
            var bnInfo = await botnetInfo(userhash);
            if (bnInfo == null)
            {
                config.logger.Log("Unable to fetch botnet info");
                return;
            }

            JToken value = null;
            var count = (int)bnInfo["count"];
            //if (count >= 1 && (int)bnInfo["canAtt1"] == 1)
            if (bnInfo.TryGetValue("canAtt1", out value) && (int)value == 1)
                response = await vhUtils.JSONRequest("user::::pass::::uhash::::cID",
                    config.username + "::::" + config.password + "::::" + userhash + "::::" + "1",
                    "vh_attackCompany.php");

            //if (count >= 2 && (int)bnInfo["canAtt2"] == 1)
            if (bnInfo.TryGetValue("canAtt2", out value) && (int)value == 1)
                response = await vhUtils.JSONRequest("user::::pass::::uhash::::cID",
                    config.username + "::::" + config.password + "::::" + userhash + "::::" + "2",
                    "vh_attackCompany2.php");

            //if (count >= 3 && (int)bnInfo["canAtt3"] == 1)
            if (bnInfo.TryGetValue("canAtt3", out value) && (int)value == 1)
                response = await vhUtils.JSONRequest("user::::pass::::uhash::::cID",
                    config.username + "::::" + config.password + "::::" + userhash + "::::" + "3",
                    "vh_attackCompany3.php");
            //temp = Utils.JSONRequest("user::::pass::::uhash::::cID",
            //                          config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + "4",
            //                             "vh_attackCompany4.php");
        }
    }
}