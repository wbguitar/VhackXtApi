using System;
using System.Threading;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi.Bot
{
    public interface IHackTimer : IDisposable
    {
        void Set(IConfig cfg, vhAPI api);
    }

    public abstract class AHackTimer<T> : vHackApi.Interfaces.Reflection.Singleton<T>, IHackTimer
        where T : vHackApi.Interfaces.Reflection.Singleton<T>
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
        static readonly int aWhileLo = 15;
        static readonly int aWhileHi = 30;

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
            //return DateTime.Now + TimeSpan.FromMinutes(r.Next(everyNow, andThen));
            return DateTime.Now + TimeSpan.FromSeconds(r.Next(everyNow, andThen));
        }

        private static TimeSpan SetPauseDuration()
        {
            //return TimeSpan.FromMinutes(r.Next(aWhileLo, aWhileHi));
            return TimeSpan.FromSeconds(r.Next(aWhileLo, aWhileHi));
        }

        public void Dispose()
        {
            if (this.hackTimer != null)
            {
                hackTimer.Dispose();
            }
        }
    }
}