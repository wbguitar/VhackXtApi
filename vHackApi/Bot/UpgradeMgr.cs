using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class UpgradeMgr : AHackTimer<UpgradeMgr>
    {
        public enum Status
        {
            Idle,
            Upgrade,
            EndTasks
        }
        
        private UpgradeMgr()
        { }

        public override void Set(IConfig cfg, vhAPI api)
        {
            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** STOPPING UPGRADE MANAGER");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** RESUMING UPGRADE MANAGER");
            };


            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            var console = api.getConsole();
            var upd = new Update(cfg);

            hackTimer = new Timer(
                async (o) =>
                {
                    if (!Monitor.TryEnter(localSemaphore))
                        return;

                    try
                    {
                        var info = await MyInfo.Fetch(console);
                        var tasks = await upd.getTasks();

                        var curStatus = CurrentStatus(info, tasks);

                        if (curStatus == Status.Idle)
                            return;

                        if (curStatus == Status.Upgrade)
                            await doUpgrade(info, upd, cfg, tasks);

                        if (curStatus == Status.EndTasks)
                            await doBoost(info, upd, cfg, tasks);
                    }
                    catch (Exception e)
                    {
                        cfg.logger.Log(e.ToString());
                    }
                    finally
                    {
                        Monitor.Exit(localSemaphore);
                    }
                }
                , null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
        }

        public Status CurStatus { get; private set; }

        private Status CurrentStatus(MyInfo info, JObject tasks)
        {
            // if no money or no boost => idle
            //if (info.Boost == 0 || info.Netcoins < 1000)
            //if (info.Netcoins < 1000)
            //    return Status.Idle;

            var tasksData = (JArray)tasks["data"];
            if (tasksData == null || tasksData.Count < info.RAM) // slots are available to add task => upgrade
            {
                if (info.Money > 5000) // TODO: can upgrade? should calculate if money > what needs to upgrade 
                    CurStatus = Status.Upgrade;
                else
                    CurStatus = Status.Idle;
            }
            else
                CurStatus = Status.EndTasks;

            return CurStatus;
        }

        int packagesBlock = 10;
        private async Task doBoost(MyInfo info, Update upd, IConfig cfg, JObject tasks)
        {
            //var res = await upd.getTasks();

            var finishAllFor = (int?)tasks["fAllCosts"];
            if (finishAllFor == null)
                return;
            
            if (finishAllFor < cfg.finishAllFor)
            {
                if (info.Netcoins < finishAllFor)
                {
                    // not enough netcoins -> try with packages
                    await doPackages(info, upd, cfg);
                }
                else
                {
                    cfg.logger.Log("{0} money needed to end tasks, finish", finishAllFor);
                    await upd.finishAll();
                }
            }
            else if (info.Boost > 0)
            {
                cfg.logger.Log("{0} money needed to end tasks, continue", finishAllFor);
                await upd.useBooster();
            }
            else if (info.Packages > packagesBlock)
            {
                await doPackages(info, upd, cfg);
            }
            //else
            {
                // upgrade botnet PCs anyway
                await doUpgradePc(info, upd);
            }
        }

        private async Task doUpgradePc(MyInfo info, Update upd)
        {
            var bnInfo = await upd.botnetInfo();
            long idToUpgrade = 0;
            long minLevel = long.MaxValue;
            for (int i = 0; i < bnInfo.Count; i++)
            {
                var it = bnInfo[i];
                var price = (long)it["bPRICE"];
                var level = (long)it["bLVL"];
                var id = (long)it["bID"];

                if (level == 100)
                    continue; // max level reached

                if (price > info.Money)
                    continue;

                if (minLevel > level)
                {
                    minLevel = level;
                    idToUpgrade = id;
                }                
            }

            if (idToUpgrade > 0)
                await upd.upgradeBotnet(idToUpgrade.ToString());
        }

        private async Task doPackages(MyInfo info, Update upd, IConfig cfg)
        {
            if (info.Packages < packagesBlock)
                return;

            for (int i = 0; i < packagesBlock; i++)
            {
                var pack = await upd.openPackage(info.UHash);
                if (pack != null)
                {
                    var package = PackageResults.FromType((int)pack["type"]);
                    cfg.logger.Log("Opened package {0}", package);
                }
            }
        }

        private async Task doUpgrade(MyInfo info, Update upd, IConfig cfg, JObject tasks)
        {
            var tasksData = (JArray)tasks["data"];
            if (tasksData == null) // means no tasks, create an empty jarray to avoid exceptions
                tasksData = new JArray();

            // if full do nothing
            if (info.RAM == tasksData.Count)
                return;

            for (int i = 0; i < info.RAM - tasksData.Count; i++)
            {
                var task = cfg.upgradeStrategy.NextTaskToUpgrade();
                var temp = await upd.startTask(task);
                var started = (int)temp["result"];
                if (started == 0)
                    cfg.logger.Log("Added task {0}, {1} slots available", task, info.RAM - tasksData.Count - i - 1); // ok TODO
                else if (started == 3)
                    cfg.logger.Log("No slot are available to upgrade"); // full
                else if (started == 1) // no money
                {
                    cfg.logger.Log("No more money to add tasks");
                    break;
                }
                else
                {
                    Debug.Assert(false);
                    break;
                }
            }
        }


    }
}