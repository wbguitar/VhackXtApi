using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    using Ratios = Dictionary<Tasks, double>;

    /// <summary>
    /// Gives a predefined task to upgrade
    /// </summary>
    public class FixedUpgradeStrategy : IUpgradeStrategy
    {
        private Tasks task;

        public FixedUpgradeStrategy(Tasks task) { this.task = task; }
        public Tasks NextTaskToUpgrade() { return task; }
    }

    /// <summary>
    /// Given a fixed proportion between the tasks, chooses the task to be upgraded based on this proportion
    /// </summary>
    public class ProportionalUpgradeStrategy : Singleton<ProportionalUpgradeStrategy>, IUpgradeStrategy
    {
        protected static vhConsole console;
        protected static vhAPI api;
        protected static IConfig cfg;

        public void Init(IConfig config, vhAPI api)
        {
            console = api.getConsole();
            cfg = config;
            ProportionalUpgradeStrategy.api = api;
        }

        protected Ratios percents = new Ratios
        {
            // sum to 1
            { Tasks.Firewall, 0.13 },
            { Tasks.Antivirus, 0.14 },
            { Tasks.Sdk, 0.25 },
            { Tasks.IPSpoofing, 0.10 },
            { Tasks.Spam, 0.17 },
            { Tasks.Scan, 0.13 },
            { Tasks.Spyware, 0.08 },
        };

        public virtual Tasks NextTaskToUpgrade()
        {
            var info = MyInfo.Fetch(console).Result;
            return chooseTask(info);
        }

        protected Tasks chooseTaskHw(MyInfo info)
        {
            var ratios = new Ratios();
            if (info.CPU < Tasks.CPU.Max)
                ratios[Tasks.CPU] = info.CPU;
            if (info.HDD < Tasks.HDD.Max)
                ratios[Tasks.HDD] = info.HDD;
            if (info.Internet < Tasks.Internet.Max)
                ratios[Tasks.Internet] = info.Internet;
            if (info.RAM < Tasks.RAM.Max)
                ratios[Tasks.RAM] = info.RAM;

            if (ratios.Count == 0)
                return null; // all HW stats are fully upgraded

            var ordered = ratios.OrderBy(r => r.Value);
            var task = ordered.First();
            return task.Key;
        }

        protected Tasks chooseTask(MyInfo info)
        {
            var hwTask = chooseTaskHw(info);
            if (hwTask != null)
                return hwTask;

            var total = info.Firewall + info.Antivirus + info.SDK
                + info.IPSpoofing + info.Spam + info.Scan + info.Spyware;

            var ratios = new Ratios
            {
                { Tasks.Firewall, (double)info.Firewall/total },
                { Tasks.Antivirus, (double)info.Antivirus/total },
                { Tasks.Sdk,(double)info.SDK/total },
                { Tasks.IPSpoofing,(double)info.IPSpoofing/total },
                { Tasks.Spam, (double)info.Spam/total },
                { Tasks.Scan, (double)info.Scan/total },
                { Tasks.Spyware,(double)info.Spyware/total },
            };

            Tasks t = Tasks.Sdk;
            foreach (var key in ratios.Keys.ToList())
            {
                ratios[key] = percents[key] - ratios[key];
            }

            var asdf = ratios.OrderByDescending(r => r.Value).ToList();
            var c = asdf.First();
            return c.Key;
        }
    }

    /// <summary>
    /// TODO
    /// </summary>
    public class StatisticalUpgradeStrategy : ProportionalUpgradeStrategy
    {
        public class Ston : Singleton<Ston>, IUpgradeStrategy
        {
            private StatisticalUpgradeStrategy strategy = new StatisticalUpgradeStrategy();
            public Tasks NextTaskToUpgrade() => strategy.NextTaskToUpgrade();
        }

        private StatisticalUpgradeStrategy() { }

        new private static Lazy<StatisticalUpgradeStrategy> inst = new Lazy<StatisticalUpgradeStrategy>(() => new StatisticalUpgradeStrategy());
        public static StatisticalUpgradeStrategy Instance => inst.Value;

        public override Tasks NextTaskToUpgrade()
        {
            var info = MyInfo.Fetch(console).Result;
            RebuildPercentages(info, cfg);
            return chooseTask(info);
        }

        /// <summary>
        /// Rebuild stats percentages based on actual informations from the other users
        /// </summary>
        /// <param name="info"></param>
        /// <param name="cfg"></param>
        public void RebuildPercentages(MyInfo info, IConfig cfg)
        {
            var ips = from ip in cfg.persistanceMgr.GetIps()
                      where ip.Firewall > 0 && ip.Antivirus > 0 && ip.SDK > 0
                            && ip.IPSpoofing > 0 && ip.Spam > 0 && ip.Spyware > 0
                      select ip;

            if (ips.Count() < 10) // bad statistic
                return;

            double avgFw = ips.Average(ip => ip.Firewall);
            double avgAv = ips.Average(ip => ip.Antivirus);
            double avgsdk = ips.Average(ip => ip.SDK);
            double avgIpsp = ips.Average(ip => ip.IPSpoofing);
            double avgSpam = ips.Average(ip => ip.Spam);
            double avgAdw = ips.Average(ip => ip.Spyware);
            double avgScan = 0; // don't know other players scan

            double total = avgFw + avgAv + avgsdk + avgIpsp + avgSpam + avgAdw + avgScan;
            percents[Tasks.Firewall] = avgFw / total;
            percents[Tasks.Antivirus] = avgAv / total;
            percents[Tasks.Sdk] = avgsdk / total;
            percents[Tasks.IPSpoofing] = avgIpsp / total;
            percents[Tasks.Spam] = avgSpam / total;
            percents[Tasks.Spyware] = avgAdw / total;
            percents[Tasks.Scan] = avgScan / total;
        }
    }
    
}






