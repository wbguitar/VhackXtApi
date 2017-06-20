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
        vhConsole console;
        public void Init(vhAPI api) => console = api.getConsole();

        Ratios percents = new Ratios
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

        public Tasks NextTaskToUpgrade()
        {
            var info = MyInfo.Fetch(console).Result;
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

        /// <summary>
        /// TODO
        /// </summary>
        public class StatisticalUpgradeStrategy : Singleton<StatisticalUpgradeStrategy>, IUpgradeStrategy
        {
            public Tasks NextTaskToUpgrade()
            {
                throw new NotImplementedException();
            }
        }

    }
}






