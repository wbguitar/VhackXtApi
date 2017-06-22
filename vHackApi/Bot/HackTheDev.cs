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
        
        public override void Set(IConfig cfg, vhAPI api)
        {
            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

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
                , null, TimeSpan.Zero, cfg.hackDevPolling);
        }
    }
}