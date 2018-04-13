using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi
{
    public class Update
    {
        private IConfig config;

        public Update(IConfig conf)
        {
            config = conf;
        }

        public async Task<JObject> getTasks()
        {
            var temp = await vhUtils.JSONRequest("user::::pass::::uhash",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                "vh_tasks.php");
            //return (JArray)temp["data"];
            return temp;
        }

        public async Task<JArray> SpywareInfo()
        {
            var temp = await vhUtils.JSONRequest("user::::pass::::uhash:::::",
                config.username + "::::" + config.password + "::::" + "UserHash_not_needed" + ":::::",
                "vh_spywareInfo.php");
            return (JArray) temp["data"];
        }

        public async Task<JObject> removeSpyware()
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash:::::",
                config.username + "::::" + config.password + "::::" + "UserHash_not_needed" + ":::::",
                "vh_removeSpyware.php");
        }

        public async Task<int> getTaskAmount()
        {
            var temp = await getTasks();
            return temp.Count;
        }

        public async Task<string[]> getTaskIDs()
        {
            var temp = await getTasks();
            var tasks = (JArray) temp["data"];
            return tasks.Select(it => (string) it["taskid"]).ToArray();
        }

        public async Task<JObject> startTask(Tasks type)
        {
            var temp = await vhUtils.JSONRequest("user::::pass::::uhash::::utype",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + type.ToString(),
                "vh_addUpdate.php");
            //var res = (int)temp["result"]; // 0 ok, 3 full
            return temp;
        }
        public async Task<bool> finishTask(string taskID)
        {
            throw new NotImplementedException(); // not working

            var res = await vhUtils.JSONRequest("user::::pass::::uhash::::taskid",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + taskID,
                "vh_finishTask.php");
            if ((int) res == 4)
                return true;

            return false;
        }

        public async Task<JObject> finishAll()
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                "vh_finishAll.php");
        }

        public async Task<JObject> useBooster()
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash::::boost",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + "1",
                "vh_tasks.php");
        }

        //public async void doTasks(TimeSpan wait)
        //{
        //    foreach (var update in config.updates)
        //    {
        //        config.logger.Log("updating {0} level +1", update);
        //        Thread.Sleep(wait);
        //        var res = await this.startTask(config.updates.Last());
        //        if (res == 0)
        //            config.logger.Log("update failed");
        //    }
        //}

        #region BOTNET

        public async Task<JObject> hotspotInfo()
        {
            /*
             * 
{"myhotspot":"0","top1owner":"brilletjuh","top1since":"25 min.","top1host":"XT-H007VH","top1strength":"21000","top2owner":"DarKhAn","top2since":"16 min.","top2host":"XT-H008VH","top2strength":"21000","top3owner":"Nemessis","top3since":"0 min.","top3host":"XT-H003VH","top3strength":"21000",
"data":[
    {"host":"XT-H002VH","owner":"ZeroCools","since":"40 min.","strength":"21000"},
    {"host":"XT-H001VH","owner":"TLaemmer92","since":"21 min.","strength":"21000"},
    {"host":"XT-H004VH","owner":"Bugadao","since":"12 min.","strength":"21000"},
    {"host":"XT-H005VH","owner":"TrioHla","since":"5 min.","strength":"21000"},
    {"host":"XT-H006VH","owner":"AB9191","since":"0 min.","strength":"21000"}],
"energy":"19","strength":"21000"}
             */
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                "vh_hotspotInfo.php");
        }

        public async Task<JObject> startHijack(string userHash, string hotspot)
        {
            /*
             * {"result":"0","lps":"501","lps2":"201"}
             */
            return await vhUtils.JSONRequest("user::::pass::::uhash::::target",
                config.username + "::::" + config.password + "::::" + userHash + "::::" + hotspot ,
                "vh_startHijack.php");

            long currentTimeMillis = (long)(DateTime.Now - new DateTime(1970, 01, 01)).TotalMilliseconds;// (long)(((double)
            return await vhUtils.JSONRequest("target::::time::::user::::pass::::uhash",
                hotspot + "::::" +currentTimeMillis +"::::" + config.username + "::::" + config.password + "::::" + userHash,
                "vh_startHijack.php");
        }

        public async Task<JObject> botnetInfo()
        {
            /*
             *
 Result: {{
  "c2": "0",
  "count": "3",		// count PCs
  "energy": "175",
  "pieces": "24",	// on 100 we can build pc
  "money": "434752700",	
  "nref": "0",
  "data": [
    {
      "running": "0",
      "wto": "0",	// one of  fw/av/smash/mwk if running == 1
      "left": "0",
      "hostname": "WbBotnetPc001",
      "fw": "3",	// firewall
      "av": "4",	// antivirus
      "smash": "18",	// smash
      "mwk": "2",		// malware kit
      "strength": "81"	// PC strength
    },
    ],
    
    ...
    
  "strength": 219		// total strenght
}}
    Status: RanToCompletion	// ???

             */
            //var temp = await vhUtils.JSONRequest("user::::pass::::uhash",
            //    config.username + "::::" + config.password + "::::" + "userHash_not_needed",
            //    "vh_botnetInfo.php");
            //return (JArray)temp["data"];

            return await vhUtils.JSONRequest("user::::pass::::uhash",
                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                "vh_botnetInfo.php");
        }



        public enum OfWhat
        {
            mwk,
            fw,
            av,
            smash,
            none,
        }

        public async Task<JObject> upgradePC(string userHash, string hostname, OfWhat ofWhat, int inst = 0, int much = 1)
        {
            /*
             
             */

            return await vhUtils.JSONRequest("user::::pass::::uhash::::hostname::::ofwhat::::inst::::much",
                $"{config.username}::::{config.password}::::{userHash}::::{hostname}::::{ofWhat.ToString()}::::{inst}::::{much}",
                "vh_upgradePC.php");
        }

        /// <summary>
        /// Creates new botnet pc
        /// </summary>
        /// <param name="userHash"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        public async Task<JObject> buildPC(string userHash, string hostname)
        {
            /*
{"result":"5","count":"2","energy":"195","pieces":"40","money":"1350047","nref":"0","data":
[
{"running":"0","wto":"0","left":"0","hostname":"WbBotnetPc001","fw":"1","av":"1","smash":"1","mwk":"1","strength":"12"},
{"running":"0","wto":"0","left":"0","hostname":"WbBotnetPc_001","fw":"1","av":"1","smash":"1","mwk":"1","strength":"12"}
],
"strength":24}
{"result":"5","count":"2","energy":"195","pieces":"40","money":"1350047","nref":"0","data":[{"running":"0","wto":"0","left":"0","hostname":"WbBotnetPc001","fw":"1","av":"1","smash":"1","mwk":"1","strength":"12"},{"running":"0","wto":"0","left":"0","hostname":"WbBotnetPc_001","fw":"1","av":"1","smash":"1","mwk":"1","strength":"12"}],"strength":24}
             */
            return await vhUtils.JSONRequest("user::::pass::::uhash::::hostname",
                $"{config.username}::::{config.password}::::{userHash}::::{hostname}",
                "vh_buildPC.php");
        }

        /// <summary>
        /// Get status for botnet career
        /// </summary>
        /// <param name="userHash"></param>
        /// <returns></returns>
        public async Task<JObject> getCareerStatus(string userHash)
        {
            // {"count":"3","strength":36,"energy":"195","nextlevel":"1"}
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}",
                "vh_getCareerStatus.php");
        }

        /// <summary>
        /// Starts an attack (chapter divided by levels)
        /// </summary>
        /// <param name="userHash"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        public async Task<JObject> startLevel(string userHash, int level)
        {
            var startLevel = level / 10;
            level /= startLevel;
            return await vhUtils.JSONRequest("user::::pass::::uhash::::lvl",
                $"{config.username}::::{config.password}::::{userHash}::::{level}",
                $"vh_startLevel{startLevel}.php");
        }

        public async Task<JObject> startLevel2(string userHash, int level)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash::::lvl",
                $"{config.username}::::{config.password}::::{userHash}::::{level}",
                "vh_startLevel2.php");
        }

        //public async Task<JObject> upgradeBotnet(string ID)
        //{
        //    return await vhUtils.JSONRequest("user::::pass::::uhash::::bID",
        //                        config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + ID,
        //                        "vh_upgradeBotnet.php");
        //}



        #endregion

        #region PACKAGES
        //public async Task<JObject> openPackage(string userHash)
        //{
        //    return await vhUtils.JSONRequest("user::::pass::::uhash",
        //        config.username + "::::" + config.password + "::::" + "userHash_not_needed", "vh_openFreeBonus.php", 5);
        //}

        //public async Task<JObject> openAllPackages(string userHash)
        //{
        //    return await vhUtils.JSONRequest("user::::pass::::uhash",
        //        config.username + "::::" + config.password + "::::" + "userHash_not_needed", "vh_openAllBonus.php");
        //}

        /// <summary>
        /// Open one package
        /// </summary>
        /// <param name="userHash"></param>
        /// <returns></returns>
        public async Task<JObject> openFreeBonus(string userHash)
        {
            /*
             {
  "type": "1",
  "win": "1538000",
  "lvl": "0",
  "bleft": "3",
  "netcoins": "34"
}
             */
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}",
                "vh_openFreeBonus.php");
        }
        /// <summary>
        /// Open all packages
        /// </summary>
        /// <param name="userHash"></param>
        /// <returns></returns>
        public async Task<JObject> openAllBonus(string userHash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}",
                "vh_openAllBonus.php");
        }
        /// <summary>
        /// Open one gold package
        /// </summary>
        /// <param name="userHash"></param>
        /// <returns></returns>
        public async Task<JObject> openGoldBox(string userHash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}",
                "vh_openGoldBox.php");
        }
        /// <summary>
        /// Open all gold packages
        /// </summary>
        /// <param name="userHash"></param>
        /// <returns></returns>
        public async Task<JObject> openAllGold(string userHash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}",
                "vh_openAllGold.php");
        }


        #endregion

        public async Task<JObject> getSysLog(string userHash = "")
        {
            return string.IsNullOrEmpty(userHash)
                ? await vhUtils.JSONRequest("user::::pass::::uhash",
                    config.username + "::::" + config.password + "::::" + "userHash_not_needed", "vh_getSysLog.php")
                : await vhUtils.JSONRequest("user::::pass::::uhash",
                    config.username + "::::" + config.password + "::::" + userHash, "vh_getSysLog.php");
        }

        public async Task<JObject> getRanking(string userHash = "")
        {
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}", "vh_ranking.php");
        }

        public async Task<JObject> getMails(int action = 3, string userHash = "")
        {
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("user::::pass::::uhash::::action",
                $"{config.username}::::{config.password}::::{userHash}::::list", "vh_mails.php", action);
        }

        public async Task<string> readMail(int id, string userHash = "")
        {
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.StringRequest("user::::pass::::uhash::::action::::mID",
                $"{config.username}::::{config.password}::::{userHash}::::getmail::::{id}", "vh_mails.php");
        }

        public async Task<JObject> getArenaInfo(string userHash = "")
        {
            /*
             * {"attleft":"5","arenarep":"965","arank":568618,"wincount":"7","losecount":"9","data":[{"username":"DYNA1 | chrisdzid","score":"5552","elo":"973","medal":"0","target":"839074","wins":"0","lose":"1","online":"0"},{"username":"MatVeiQaaa","score":"4339","elo":"973","medal":"0","target":"544123","wins":"1","lose":"2","online":"0"},{"username":"chepe6191","score":"4473","elo":"972","medal":"0","target":"490062","wins":"14","lose":"16","online":"0"},{"username":"<PW> | NieGrzeczny","score":"19297","elo":"971","medal":"0","target":"149245","wins":"3","lose":"4","online":"0"},{"username":"NW14 | arresto","score":"17136","elo":"970","medal":"0","target":"543678","wins":"5","lose":"6","online":"0"},{"username":"INDO. | zer0days","score":"12730","elo":"969","medal":"0","target":"574661","wins":"8","lose":"10","online":"0"},{"username":"BingBongBooey","score":"11487","elo":"969","medal":"0","target":"626873","wins":"4","lose":"5","online":"0"},{"username":"F@TE | 
             * ...
             */
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("user::::pass::::uhash",
                $"{config.username}::::{config.password}::::{userHash}", "vh_arenaInfo.php");
        }

        public async Task<JObject> ArenaAttack(int target, string userHash = "")
        {
            /*
             * {"left":"21591","attleft":"4","arenarep":"990","arank":"568527","wincount":"8","losecount":"9","result":"4","winelo":""}
             */
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("user::::pass::::uhash::::target",
                $"{config.username}::::{config.password}::::{userHash}::::{target}", "vh_arenaAttack.php");
        }

        public async Task<JObject> GetDailyData(string userHash = "")
        {
            /*
             * {"user_id":"1057014","login":"0","scan1":"0","scan2":"0","transfer1":"0","transfer2":"0","time":"1523221201","result":"0","transfers":"191","scans":"119","resethours":"15","resetminutes":"22","gotmissions":"1",
                "data":
                [{"target":"154.109.199.35","reward":"50","creator":"Anonymous"},
                {"target":"104.94.149.202","reward":"50","creator":"Anonymous"},
                {"target":"87.200.143.174","reward":"50","creator":"Anonymous"},
                {"target":"235.151.11.53","reward":"50","creator":"Anonymous"},
                {"target":"239.139.233.42","reward":"50","creator":"Anonymous"},
                {"target":"91.212.73.96","reward":"50","creator":"Anonymous"},
                {"target":"85.24.53.114","reward":"50","creator":"Anonymous"},
                {"target":"233.190.106.97","reward":"50","creator":"Anonymous"},
                {"target":"133.196.140.12","reward":"50","creator":"Anonymous"},
                {"target":"222.61.199.105","reward":"50","creator":"Anonymous"},
                {"target":"248.144.43.203","reward":"50","creator":"Anonymous"},
                ...
             */
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            //return await vhUtils.JSONRequest("::::time::::user::::pass::::uhash",
            //    $"::::{vhUtils.CurrentTimeMsecs}::::{config.username}::::{config.password}::::{userHash}", 
            //    "vh_getDailyData.php");

            return await vhUtils.JSONRequest("::::user::::pass::::uhash",
                $"::::{config.username}::::{config.password}::::{userHash}",
                "vh_getDailyData.php");
        }

        public async Task<JObject> GetDaily(int dtype, string userHash = "")
        {
            /*
             * {"user_id":"1057014","login":"0","scan1":"0","scan2":"0","transfer1":"0","transfer2":"0","time":"1523221201","result":"0"}
             */
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("dtype::::time::::user::::pass::::uhash",
                $"{dtype}::::{vhUtils.CurrentTimeMsecs}::::{config.username}::::{config.password}::::{userHash}",
                "vh_getDaily.php");
        }
    }
}