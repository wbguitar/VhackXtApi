using vHackApi.Interfaces;

namespace vHackBot
{
    class Log4netLogger : ILogger
    {
        private log4net.ILog logger;

        public Log4netLogger()
        {
            log4net.Config.XmlConfigurator.Configure(new System.IO.FileInfo("log4net.xml"));
            logger = log4net.LogManager.GetLogger("Rolling");
        }
        public void Log(string format, params object[] parms)
        {
            if (parms == null || parms.Length == 0)
                logger.Info(format);
            else
                logger.InfoFormat(format, parms);
        }
    }
}