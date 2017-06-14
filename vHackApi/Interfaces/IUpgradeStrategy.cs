using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;

namespace vHackApi.Interfaces
{
    /// <summary>
    /// Implements a strategy to select the tasks that needs to be upgraded
    /// </summary>
    public interface IUpgradeStrategy
    {
        Tasks NextTaskToUpgrade();
    }
}
