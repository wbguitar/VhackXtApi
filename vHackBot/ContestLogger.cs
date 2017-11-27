using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackBot
{
    class ContestLogger : ILogger
    {
        private log4net.ILog logger;

        public ContestLogger()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
            logger = log4net.LogManager.GetLogger("ContestLogger");
        }

        public void Log(string format, params object[] parms)
        {
            // only log during contest
            if (vhUtils.IsContestRunning())
                logger.DebugFormat(format, parms);
        }
    }
}