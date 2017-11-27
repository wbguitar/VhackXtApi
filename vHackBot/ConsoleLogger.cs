using System;
using vHackApi.Interfaces;

namespace vHackBot
{
    class ConsoleLogger : ILogger
    {
        #region ILogger Members

        public void Log(string format, params object[] parms)
        {
            var msg = (parms.Length == 0) ? format : string.Format(format, parms);
            Console.WriteLine("{0} - {1}", DateTime.Now, msg);
        }

        #endregion ILogger Members
    }
}