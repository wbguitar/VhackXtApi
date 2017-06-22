using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using vHackApi.Api;

namespace vHackApi.Interfaces
{
    public interface IHackTimer : IDisposable
    {
        void Set(IConfig cfg, vhAPI api);
    }
}
