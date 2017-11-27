using System;
using System.Net;
using vHackApi;
using vHackApi.Bot;
using vHackApi.Interfaces;

namespace vHackBot
{
    public class Config : IConfig
    {
        #region IConfig Members

        public virtual string username => Properties.Settings.Default.user;

        public virtual string password => Properties.Settings.Default.pass;

        public virtual bool hackIfNotAnonymous
        {
            get { return Properties.Settings.Default.hackIfNotAnonymous; }
            set { Properties.Settings.Default.hackIfNotAnonymous = value; }
        }

        private ConsoleLogger cl = new ConsoleLogger();
        private Log4netLogger l4logger = new Log4netLogger();
        public ILogger logger
        {
            get { return l4logger; }
        }

        public string tessdata => Properties.Settings.Default.TessdataPath;

        public int waitstep
        {
            get { return Properties.Settings.Default.WaitStep; }
            set { Properties.Settings.Default.WaitStep = value; }
        }

        public int winchance
        {
            get { return Properties.Settings.Default.WinChance; }
            set { Properties.Settings.Default.WinChance = value; }
        }

        public string dbConnectionString => Properties.Settings.Default.dbConnString;

        public int maxFirewall
        {
            get { return Properties.Settings.Default.maxFirewall; }
            set { Properties.Settings.Default.maxFirewall = value; }
        }

        public int maxAntivirus
        {
            get { return Properties.Settings.Default.maxAntivirus; }
            set { Properties.Settings.Default.maxAntivirus = value; }
        }

        public int getImgBy
        {
            get { return Properties.Settings.Default.getImgBy; }
            set { Properties.Settings.Default.getImgBy = value; }
        }

        public TimeSpan hackDevPolling => Properties.Settings.Default.hackDevPolling;

        public TimeSpan hackBotnetPolling => Properties.Settings.Default.hackBotnetPolling;

        public TimeSpan ipAttackPolling => Properties.Settings.Default.ipAttackPolling;

        public TimeSpan ipScannerPolling => Properties.Settings.Default.ipScannerPolling;

        public bool safeScan => Properties.Settings.Default.safeScan;

        //public IIPselector ipSelector => IPSelectorASAP.Instance;
        public IIPselector ipSelector => IPSelectorRandom.Default;

        public IUpgradeStrategy upgradeStrategy => ProportionalUpgradeStrategy.Default;
        //public IUpgradeStrategy upgradeStrategy => StatisticalUpgradeStrategy.Ston.Default;

        //public IPersistanceMgr persistanceMgr => DbManager.Instance;
        public IPersistanceMgr persistanceMgr => XmlMgr.Default;

        public IWebProxy proxy { get; set; } = (!string.IsNullOrEmpty(Properties.Settings.Default.proxyAddress) && Properties.Settings.Default.proxyPort != 0) ? new WebProxy(Properties.Settings.Default.proxyAddress, Properties.Settings.Default.proxyPort) : null;

        public int finishAllFor
        {
            get { return Properties.Settings.Default.finishAllFor; }
            set { Properties.Settings.Default.finishAllFor = value; }
        }

        public string vhServerHost => Properties.Settings.Default.httpHost;

        public int vhServerPort => Properties.Settings.Default.httpPort;

        public bool ipAttackPaused { get; set; }

        public bool ipScannerPaused { get; set; }

        public bool hackTheDevPaused { get; set; }

        public bool hackBotNetPaused { get; set; }

        public string chatIp => Properties.Settings.Default.chatIp;
        public int chatPort => Properties.Settings.Default.chatPort;
        public string chatUser => Properties.Settings.Default.chatUser;

        #endregion IConfig Members
    }
}