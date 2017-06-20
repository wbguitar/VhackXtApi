using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class HackTheDev : AHackTimer<HackTheDev>
    {
        private HackTheDev() { }

        //private static HackTheDev inst;
        //public static HackTheDev Instance
        //{
        //    get
        //    {
        //        if (inst == null)
        //            inst = new HackTheDev();

        //        return inst;
        //    }
        //}

        public override void Set()
        {
            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            var cfg = GlobalConfig.Config;
            var api = GlobalConfig.Api;

            var console = api.getConsole();

            hackTimer = new Timer(
                async (o) =>
                {
                    try
                    { var s = await console.AttackIp("127.0.0.1"); }
                    catch (Exception e)
                    {
                        cfg.logger.Log(e.ToString());
                    }
                }
                , null, TimeSpan.Zero, GlobalConfig.Config.hackDevPolling);
        }
    }
}