using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackBot
{
    public static class GlobalConfig
    {
        public static vhAPI Api { get; private set; }
        public static IConfig Config { get; private set; }
        public static void Init(IConfig cfg, vhAPI api)
        {
            Api = api;
            Config = cfg;
        }
    }
}