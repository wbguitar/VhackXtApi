using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public abstract class AHackTimer
    {
        protected static object globalSemaphore = new object();
        protected object localSemaphore = new object();
        /// <summary>
        /// Used internally for pause, if overridden in inherited classes
        /// </summary>
        protected TimeSpan pause;

        protected Action Pause = () => { };
        protected Action Resume = () => { };

        protected Timer hackTimer;

        static readonly int everyNow = 10;
        static readonly int andThen = 30;
        static readonly int aWhileLo = 2;
        static readonly int aWhileHi = 6;

        private static Random r = new Random();

        private DateTime nextPause = NextPauseSchedule();

        /// <summary>
        /// Every now and then pauses for a while
        /// </summary>
        private Thread tTimeForAPause;

        public AHackTimer()
        {
            tTimeForAPause = new Thread(() =>
            {
                while (true)
                {
                    if (DateTime.Now >= nextPause)
                    {
                        pause = SetPauseDuration();
                        Pause();
                        Thread.Sleep(pause);
                        nextPause = NextPauseSchedule();
                        Resume();
                    }

                    Thread.Sleep(1);
                }
            });

            tTimeForAPause.Start();
        }

        /// <summary>
        /// Used as period by the timer
        /// </summary>
        public TimeSpan Period { get; protected set; }

        public abstract void Set(IConfig cfg, vhAPI api);

        private static DateTime NextPauseSchedule()
        {
            return DateTime.Now + TimeSpan.FromMinutes(r.Next(everyNow, andThen));
        }

        private static TimeSpan SetPauseDuration()
        {
            return TimeSpan.FromMinutes(r.Next(aWhileLo, aWhileHi));
        }
    }
}