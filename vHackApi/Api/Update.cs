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
            return (JArray)temp["data"];
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
            var tasks = (JArray)temp["data"];
            return tasks.Select(it => (string)it["taskid"]).ToArray();
        }

        public async Task<JObject> startTask(Tasks type)
        {
            var temp = await vhUtils.JSONRequest("user::::pass::::uhash::::utype",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + type.ToString(),
                                "vh_addUpdate.php");
            //var res = (int)temp["result"]; // 0 ok, 3 full
            return temp;
        }

        public async Task<JArray> botnetInfo()
        {
            var temp = await vhUtils.JSONRequest("user::::pass::::uhash",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                                "vh_botnetInfo.php");
            return (JArray)temp["data"];
        }

        public async Task<JObject> upgradeBotnet(string ID)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash::::bID",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + ID,
                                "vh_upgradeBotnet.php");
        }

        public async Task<bool> finishTask(string taskID)
        {
            throw new NotImplementedException(); // not working

            var res = await vhUtils.JSONRequest("user::::pass::::uhash::::taskid",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + taskID,
                                "vh_finishTask.php");
            if ((int)res == 4)
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

        public async Task<JObject> openPackage(string userHash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash", config.username + "::::" + config.password + "::::" + "userHash_not_needed", "vh_openFreeBonus.php", 5);
        }

        public async Task<JObject> openAllPackages(string userHash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash", config.username + "::::" + config.password + "::::" + "userHash_not_needed", "vh_openAllBonus.php");
        }


        public async Task<JObject> getSysLog(string userHash = "")
        {
            return string.IsNullOrEmpty(userHash) ?
                await vhUtils.JSONRequest("user::::pass::::uhash", config.username + "::::" + config.password + "::::" + "userHash_not_needed", "vh_getSysLog.php") :
                await vhUtils.JSONRequest("user::::pass::::uhash", config.username + "::::" + config.password + "::::" + userHash, "vh_getSysLog.php");
        }

        public async Task<JObject> getRanking(string userHash = "")
        {
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("user::::pass::::uhash", $"{config.username}::::{config.password}::::{userHash}", "vh_ranking.php");
        }

        public async Task<JObject> getMails(string userHash = "")
        {
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.JSONRequest("user::::pass::::uhash::::action", $"{config.username}::::{config.password}::::{userHash}::::list", "vh_mails.php", 3);
        }

        public async Task<string> readMail(int id, string userHash = "")
        {
            if (string.IsNullOrEmpty(userHash))
                userHash = "userHash_not_needed";

            return await vhUtils.StringRequest("user::::pass::::uhash::::action::::mID", $"{config.username}::::{config.password}::::{userHash}::::getmail::::{id}", "vh_mails.php");
        }

    }
}