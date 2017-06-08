﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhackXtApi.Api;
using System.Threading;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

namespace VhackXtApi
{
    public class Update
    {
        IConfig config;

        public Update(IConfig conf)
        {
            config = conf;
        }

        public async Task<JArray> getTasks()
        {
            var temp = await Utils.JSONRequest("user::::pass::::uhash",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                                "vh_tasks.php");
            return (JArray)temp["data"];
        }

        public async Task<JArray> SpywareInfo()
        {
            var temp = await Utils.JSONRequest("user::::pass::::uhash:::::",
                              config.username + "::::" + config.password + "::::" + "UserHash_not_needed" + ":::::",
                              "vh_spywareInfo.php");
            return (JArray)temp["data"];
        }

        public async Task<JObject> removeSpyware()
        {
            return await Utils.JSONRequest("user::::pass::::uhash:::::",
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
            return temp.Select(it => (string)it["taskid"]).ToArray();
        }


        public async Task<int> startTask(Tasks type)
        {
            var temp = await Utils.JSONRequest("user::::pass::::uhash::::utype",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + type,
                                "vh_addUpdate.php");
            var res = (int)temp["result"]; // 0 ok, 3 pieno
            return res;
        }

        public async Task<JArray> botnetInfo()
        {
            var temp = await Utils.JSONRequest("user::::pass::::uhash",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                                "vh_botnetInfo.php");
            return (JArray)temp["data"];
        }

        public async Task<JObject> upgradeBotnet(string ID)
        {
            return await Utils.JSONRequest("user::::pass::::uhash::::bID",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + ID,
                                "vh_upgradeBotnet.php");
        }

        public async Task<bool> finishTask(string taskID)
        {
            throw new NotImplementedException(); // non funziona

            var res = await Utils.JSONRequest("user::::pass::::uhash::::taskid",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + taskID,
                                "vh_finishTask.php");
            if ((int)res == 4)
                return true;

            return false;
        }

        public async Task<bool> finishAll()
        {
            var res = await Utils.JSONRequest("user::::pass::::uhash",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                                "vh_finishAll.php");
            if ((int)res == 0)
                return true;

            return false;
        }

        public async Task<JObject> useBooster()
        {
            return await Utils.JSONRequest("user::::pass::::uhash::::boost",
                                config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + "1",
                                "vh_tasks.php");
        }

        public async void doTasks(TimeSpan wait)
        {
            foreach (var update in config.updates)
            {
                config.logger.Log("updating {0} level +1", update);
                Thread.Sleep(wait);
                var res = await this.startTask(config.updates.Last());
                if (res == 0)
                    config.logger.Log("update failed");
            }
        }
    }
}
