using System;

namespace vHackApi.Api
{
    public class Tasks : IEquatable<string>
    {
        private string task;
        private int upgradePrice;

        private Tasks(string task, int upg)
        {
            this.task = task;
            upgradePrice = upg;
        }

        public override string ToString() => task;

        public bool Equals(string other)
        {
            return other.ToLower().Equals(task.ToLower());
        }

        public int UpgradePrice => upgradePrice;

        // Software (limitless)
        public static readonly Tasks Spam = new Tasks("spam", 650);
        public static readonly Tasks Antivirus = new Tasks("av", 1400);
        public static readonly Tasks Sdk = new Tasks("sdk", 1200);
        public static readonly Tasks Firewall = new Tasks("fw", 800);
        public static readonly Tasks IPSpoofing = new Tasks("ipsp", 700);
        public static readonly Tasks Scan = new Tasks("scan", 650);
        public static readonly Tasks Spyware = new Tasks("adw", 650);

        // Hardware (limited)
        public static readonly Tasks CPU = new Tasks("cpu", 100);
        public static readonly Tasks RAM = new Tasks("ram", 100);
        public static readonly Tasks HDD = new Tasks("hdd", 100);
        public static readonly Tasks Internet = new Tasks("inet", 100);
    }
}