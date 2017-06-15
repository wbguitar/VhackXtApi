using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    /// <summary>
    /// Gives a predefined task to upgrade
    /// </summary>
    public class FixedUpgradeStrategy : IUpgradeStrategy
    {
        private Tasks task;

        public FixedUpgradeStrategy(Tasks task) { this.task = task; }
        public Tasks NextTaskToUpgrade() { return task; }
    }

    public class StatisticalUpgradeStrategy : IUpgradeStrategy
    {
        public Tasks NextTaskToUpgrade()
        {
            // TODO: algorythm that select the best task to upgrade chosen by current local stats
            throw new NotImplementedException(); 
        }
    }
}
