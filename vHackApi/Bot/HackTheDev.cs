using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Chat;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public class HackTheDev : AHackTimer<HackTheDev>
    {
        private HackTheDev() { }
        private vhChat _chat;
        public vhChat Chat => _chat;

        public override void Set(IConfig cfg, vhAPI api)
        {
            

            Pause = () =>
            {
                hackTimer.Change(0, Timeout.Infinite);
                cfg.logger.Log("*** Pausing HackTheDev");
            };

            Resume = () =>
            {
                Set(cfg, api);
                cfg.logger.Log("*** Resuming HackTheDev");
            };

            InternalPause = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** PAUSING HackTheDev");
                    hackTimer.Change(TimeSpan.Zero, pause);
                }
            };

            InternalResume = () =>
            {
                if (hackTimer != null)
                {
                    cfg.logger.Log("*** RESUMING HackTheDev");
                    hackTimer.Change(TimeSpan.Zero, Period);
                }
            };

            if (hackTimer != null)
            {
                hackTimer.Dispose();
                hackTimer = null;
            }

            var console = api.getConsole();

            var lastAttackTm = DateTime.MinValue;
            hackTimer = new Timer(
                async (o) =>
                {
                    if (DateTime.Now - lastAttackTm <= TimeSpan.FromHours(1))
                        return;

                    if (!Monitor.TryEnter(this))
                        return;

                    try
                    {
                        var s = await console.AttackIp("127.0.0.1");
                        if (s == 0)
                            lastAttackTm = DateTime.Now;
                    }
                    catch (Exception e)
                    {
                        cfg.logger.Log(e.ToString());
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                }
                , null, TimeSpan.Zero, cfg.hackDevPolling);

            _chat = new vhChat(cfg, api);
            _chat.PrivateMessage += async (rule, email, nick, msg) =>
            {
                System.Console.ForegroundColor = ConsoleColor.Blue;
                //cfg.logger.Log($"{DateTime.Now} {nick}({rule}): {msg}");
                cfg.logger.Log($"VHCHAT - {nick}[{email}]({rule}): {msg}");
                System.Console.ResetColor();
                
                // if message from bot try to hack the dev (maybe it has been reset)
                if (nick == "vHackXTBot")
                {
                    var s = await console.AttackIp("127.0.0.1");
                    if (s == 0)
                        lastAttackTm = DateTime.Now;
                }

            };

            _chat.Run();
        }
    }
}