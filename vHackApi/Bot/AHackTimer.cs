using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public abstract class AHackTimer<T> : vHackApi.Interfaces.Reflection.Singleton<T>, IHackTimer
        where T : vHackApi.Interfaces.Reflection.Singleton<T>
    {
        protected static object globalSemaphore = new object();
        protected object localSemaphore = new object();
        /// <summary>
        /// Used internally for pause, if overridden in inherited classes
        /// </summary>
        protected TimeSpan pause;

        /// <summary>
        /// Pause action fired from internal bot logic
        /// </summary>
        protected Action InternalPause = () => { };

        /// <summary>
        /// Resume action fired from internal bot logic
        /// </summary>
        protected Action InternalResume = () => { };

        /// <summary>
        /// Pause action fired from http client command
        /// </summary>
        public Action Pause = () => { };

        /// <summary>
        /// Resume action fired from http client command
        /// </summary>
        public Action Resume = () => { };

        protected Timer hackTimer;

        static readonly int everyNow = (int)TimeSpan.FromMinutes(10).TotalSeconds;
        static readonly int andThen = (int)TimeSpan.FromMinutes(60).TotalSeconds;
        static readonly int aWhileLo = (int)TimeSpan.FromMinutes(5).TotalSeconds;
        static readonly int aWhileHi = (int)TimeSpan.FromMinutes(20).TotalSeconds;

        protected static Random rand = new Random();

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
                    Thread.Sleep(1);

                    if (!safeScan)
                        continue;

                    if (DateTime.Now >= nextPause)
                    {
                        pause = SetPauseDuration();
                        InternalPause();
                        Thread.Sleep(pause);
                        nextPause = NextPauseSchedule();
                        InternalResume();
                    }

                }
            });

            tTimeForAPause.Start();
        }

        /// <summary>
        /// Used as period by the timer
        /// </summary>
        public TimeSpan Period { get; protected set; }

        public abstract void Set(IConfig cfg, vhAPI api);

        protected bool safeScan = false;

        private static DateTime NextPauseSchedule()
        {
            //if (vhUtils.IsContestRunning())
            //    return DateTime.Now + TimeSpan.FromHours(2);

            return DateTime.Now + TimeSpan.FromSeconds(rand.Next(everyNow, andThen));
        }

        private static TimeSpan SetPauseDuration()
        {
            if (vhUtils.IsContestRunning())
                return TimeSpan.FromSeconds(rand.Next(30, 60));

            return TimeSpan.FromSeconds(rand.Next(aWhileLo, aWhileHi));
        }

        public void Dispose()
        {
            if (this.hackTimer != null)
            {
                hackTimer.Dispose();
            }
            
            if (tTimeForAPause != null && tTimeForAPause.IsAlive)
                tTimeForAPause.Abort();
        }
    }
}