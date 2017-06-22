using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vHackApi.Api
{
    public class PackageResults
    {
        static readonly PackageResults Money = new PackageResults("money");
        static readonly PackageResults Netcoins = new PackageResults("netcoins");
        static readonly PackageResults Antivirus = new PackageResults("av");
        static readonly PackageResults Firewall = new PackageResults("fw");
        static readonly PackageResults Ipspoofing = new PackageResults("ipsp");
        static readonly PackageResults BotnetPc = new PackageResults("pcs");
        static readonly PackageResults Sdk = new PackageResults("sdk");
        static readonly PackageResults Spam = new PackageResults("spam");
        static readonly PackageResults Scan = new PackageResults("scan");
        static readonly PackageResults Spyware = new PackageResults("adw");
        static readonly PackageResults Boost = new PackageResults("boost");

        private static PackageResults[] packs = new PackageResults[]
        {
             Money,
             Netcoins,
             Antivirus,
             Firewall,
             Ipspoofing,
             BotnetPc,
             Sdk,
             Spam,
             Scan,
             Spyware ,
             Boost,
        };

        String toString;
        private PackageResults(String toString) { this.toString = toString; }

        public static PackageResults FromType(int type)
        {
            return (type >= 0 && type < packs.Length) ? packs[type] : null;
        }

        //public override string ToString() => toString;
        public override string ToString() => this.GetType().Name;
    }


    public class PackageResult
    {
        public PackageResults Type { get; private set; }
        public int Amount { get; private set; }

        public PackageResult(PackageResults type, int amount)
        {
            Type = type;
            Amount = amount;
        }
    }
}
