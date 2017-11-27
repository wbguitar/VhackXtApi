using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Console;
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

        ILogger logger;
        private vhAPI _api;
        
        public override void Set(IConfig cfg, vhAPI api)
        {
            logger = cfg.logger;
            _api = api;

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

                        // upgrade botnet PCs anyway
                        await doUpgradePc(info, upd);

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

        private Status curStatus;

        public Status CurStatus
        {
            get { return curStatus; }
            set
            {
                if (curStatus != value)
                    logger.Log($"Status changed to {value}");
                curStatus = value;
            }
        }

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

        int packagesBlock = 5;
        int goldPackagesBlock = 2;
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
                    await doGoldPackages(info, upd, cfg);
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
            //else 
            {
                if (info.Packages > packagesBlock)
                    await doPackages(info, upd, cfg);
                if (info.GoldPackages > goldPackagesBlock)
                    await doGoldPackages(info, upd, cfg);
            }
            //else
            {
                
            }
        }

        private int[] _levelsTable = new int[]
        {
            18,
            220,
            580,
            800,
            1120,
            1310,
            1640,
            1940,
            2222,
            2600,
            2800,
            3200, // 3 en
            3750, // 4 en
            4150, // 4 en
            4700, // 4 en

        };

        enum UpgradePCStatus
        {
            UpgradePC,
            Attack,
            FillEnergy,
        };
        const int upgrPrice = 2000000;  // price for upgrading 1 PC
        const int maxEnergy = 25;       //
        const int pcOrAttack = 16;      // energy limit to choose between upgrade PC or attack
        const int maxLevel = 250;

        private UpgradePCStatus current = UpgradePCStatus.FillEnergy;

        bool isFullyUpgraded(JObject bnInfo)
        {
            var data = bnInfo["data"];
            var count = (int)bnInfo["count"];
            for (int i = 0; i < count; i++)
            {
                var it = bnInfo["data"][i];
                var running = (long)it["running"];
                if (running != 0 ||
                    (long)it["smash"] < maxLevel ||
                    (long)it["mwk"] < maxLevel ||
                    (long)it["av"] < maxLevel ||
                    (long)it["av"] < maxLevel)
                    return false;
            }
            return true;
        }

        UpgradePCStatus getUpgrade(int energy, long money)
        {
            if (current == UpgradePCStatus.FillEnergy &&
                energy < maxEnergy)
                return UpgradePCStatus.FillEnergy;

            if (current == UpgradePCStatus.FillEnergy &&
                energy >= maxEnergy)
            {
                current = UpgradePCStatus.UpgradePC;
                return current;
            }

            if (energy >= pcOrAttack)
            {
                // if not enough money we go with attack
                if (money <= upgrPrice)
                    current = UpgradePCStatus.Attack;
                else
                    current = UpgradePCStatus.UpgradePC;
                return current;
            }

            if (6 < energy && energy < pcOrAttack)
            {
                current = UpgradePCStatus.Attack;
                return current;
            }

            current = UpgradePCStatus.FillEnergy;
            return current;
        }

        private async Task doUpgradePc(MyInfo info, Update upd)
        {
            var bnInfo = await upd.botnetInfo();
            var career = await upd.getCareerStatus(vhConsole.uHash);
            if (bnInfo == null)
                return;

            var count = (int) bnInfo["count"];
            var energy = (int) bnInfo["energy"];
            var pieces = (int) bnInfo["pieces"];
            var nextlvl = (int) career["nextlevel"];

            if (pieces >= 30)
            {
                var pcName = $"BotnetPC_{count:D3}";
                JObject buildResult = null;
                try
                {
                    buildResult = await upd.buildPC(vhConsole.uHash, pcName);
                }
                catch (Exception e)
                {
                }
            }

            var money = info.Money;
            var upgrade = getUpgrade(energy, info.Money);
            var upgradedCount = -1;
            if (upgrade == UpgradePCStatus.UpgradePC)
            {
                upgradedCount = 0;
                for (int i = 0; i < count; i++)
                {
                    var it = bnInfo["data"][i];
                    var running = (long) it["running"];

                    if (running == 0)
                    {
                        //var dict = new Dictionary<Update.OfWhat, long>();

                        //dict[Update.OfWhat.fw] = (long)it["fw"];
                        //dict[Update.OfWhat.av] = (long)it["av"];
                        //dict[Update.OfWhat.smash] = (long)it["smash"];
                        //dict[Update.OfWhat.mwk] = (long)it["mwk"];

                        //// update the minor value
                        //var whatToUpdate = dict.OrderBy(pair => pair.Value).First().Key;





                        //// strategy 1: update ONLY what's requested by current chapter
                        //var whatToUpdate = Update.OfWhat.smash; // first chapter smash
                        //if (nextlvl < 21)
                        //    whatToUpdate = Update.OfWhat.mwk; // 2nd malware kit
                        //else if (nextlvl < 31)
                        //    whatToUpdate = Update.OfWhat.fw;
                        //else if (nextlvl < 41)
                        //    whatToUpdate = Update.OfWhat.av;

                        var whatToUpdate = Update.OfWhat.smash; // first chapter smash
                        if (nextlvl > 10)
                            whatToUpdate = Update.OfWhat.mwk; // 2nd malware kit

                        if ((long) it[whatToUpdate.ToString()] == 250)
                        {
                            var bak = whatToUpdate;
                            //// strategy 2: update in order of chapter's request priority
                            //whatToUpdate = Update.OfWhat.smash;
                            //if ((long)it["smash"] == maxLevel) // && bak != Update.OfWhat.mwk)
                            //    whatToUpdate = Update.OfWhat.mwk;
                            //else if ((long)it["mwk"] == maxLevel) // && bak != Update.OfWhat.av)
                            //    whatToUpdate = Update.OfWhat.av;
                            //else if ((long)it["av"] == maxLevel) // && bak != Update.OfWhat.fw)
                            //    whatToUpdate = Update.OfWhat.fw;
                            //else if ((long)it["fw"] == maxLevel)
                            //    continue;

                            // strategy 2: update in order of chapter's request priority
                            whatToUpdate = Update.OfWhat.none;
                            if ((long) it["smash"] < maxLevel) // && bak != Update.OfWhat.mwk)
                                whatToUpdate = Update.OfWhat.smash;
                            else if ((long) it["mwk"] < maxLevel) // && bak != Update.OfWhat.av)
                                whatToUpdate = Update.OfWhat.mwk;
                            else if ((long) it["av"] < maxLevel) // && bak != Update.OfWhat.fw)
                                whatToUpdate = Update.OfWhat.av;
                            else if ((long) it["fw"] < maxLevel)
                                whatToUpdate = Update.OfWhat.fw;
                        }

                        var hostname = (string) it["hostname"];
                        logger.Log("Upgrading {0} {1}", hostname, whatToUpdate);
                        if (whatToUpdate != Update.OfWhat.none)
                        {

                            var uhash = "userHash_not_needed";
                            //var uhash = vhConsole.uHash;
                            try
                            {
                                // TODO: try-catch cause server answer is badly formatted, but it works anyway
                                var res = await upd.upgradePC(uhash, hostname, whatToUpdate);
                            }
                            catch (Exception)
                            {
                            }
                            upgradedCount++;
                        }
                    }
                    else
                        upgradedCount++; // we count also running updates
                }
            }

            // update what to upgrade
            if (upgradedCount == 0) // if no pc has been upgraded we jump right to the attack; if -1 then we are on attack or on fill energy
                upgrade = UpgradePCStatus.Attack;
            //else
            //{
            //    // update values and see if we need to attack
            //    info = await MyInfo.Fetch(_api.getConsole());
            //    bnInfo = await upd.botnetInfo();
            //    energy = (int) bnInfo["energy"];
            //    money = info.Money;
            //    upgrade = getUpgrade(energy, money);
            //}


            if (upgrade == UpgradePCStatus.Attack)
            {
                if (nextlvl <= _levelsTable.Length)
                {
                    // if we have enough strenght we try to attack next level
                    var minV = _levelsTable[nextlvl - 1];
                    var strength = (int)bnInfo["strength"];
                    // TODO: bypassed for now
                    //if (strength > minV * 1.35)
                    //{
                    //    var startRes = await upd.startLevel("userHash_not_needed", nextlvl);
                    //}
                }

                // attack the last level of last chapter (earn much money)
                var lvlToSmash = nextlvl - nextlvl % 10; // TODO: WHY WITH 20 DOESN'T WORK?
                //var lvlToSmash = 10;
               
                var smashResult = await upd.startLevel(info.UHash, lvlToSmash);
                logger.Log("Attacking botnet level {0}, result {1}", lvlToSmash, smashResult["result"]);
            }


            // HIJACK!!
            var hsinfo = await upd.hotspotInfo();
            var mystrength = (long)bnInfo["strength"];
            if ((int) hsinfo["myhotspot"] == 0) // no hotspot owned
            {
                var arr = (JArray) hsinfo["data"];
                for (int i = 0; i < arr.Count; i++)
                {
                    var host = (string) arr[i]["host"];
                    var strength = (long)arr[i]["strength"];
                    if (mystrength >= strength)
                    {
                        // lets hijack this sucker!!!11
                        var hjres = await upd.startHijack(vhConsole.uHash, host);
                        if ((int) hjres["result"] == 0)
                        {
                            logger.Log("Succesfully hijacked host {0}", host);
                            break;
                        }
                        else
                            logger.Log("Unable hijack host {0}", host);
                    }
                }
            }
        }

        private async Task doPackages(MyInfo info, Update upd, IConfig cfg)
        {
            // TODO: SHOULD BE REIMPLEMENTED IN V.12
            if (info.Packages < packagesBlock)
                return;

            for (int i = 0; i < packagesBlock; i++)
            {
                //var pack = await upd.openPackage(info.UHash);
                var pack = await upd.openFreeBonus(info.UHash);
                if (pack != null)
                {
                    var package = PackageResults.FromType((int)pack["type"]);
                    cfg.logger.Log("Opened package {0}", package);
                }
            }
        }

        private async Task doGoldPackages(MyInfo info, Update upd, IConfig cfg)
        {
            if (info.GoldPackages < packagesBlock)
                return;

            for (int i = 0; i < packagesBlock; i++)
            {
                //var pack = await upd.openPackage(info.UHash);
                var pack = await upd.openGoldBox(info.UHash);
                if (pack != null)
                {
                    var package = GoldPackageResults.FromType((int)pack["type"]);
                    cfg.logger.Log("Opened gold package {0}", package);
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