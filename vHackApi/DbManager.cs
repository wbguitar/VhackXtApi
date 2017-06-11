using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using vHackApi.Interfaces;
using vHackApi.Api;

namespace vHackApi
{
    public partial class vhackdbEntities
    {
        public vhackdbEntities(string connString)
            : base(connString)
        { }
    }

    public partial class IPs : IEquatable<IPs>
    {
        public IPs(JObject jsons) :base()
        {
            try
            {
                this.IP = (string)jsons["ipaddress"];

                this.Firewall = ((string)jsons["fw"]).Contains('?') ? null : (long?)jsons["fw"];
                this.Antivirus = ((string)jsons["av"]).Contains('?') ? null : (long?)jsons["av"];
                this.Spam = ((string)jsons["spam"]).Contains('?') ? null : (long?)jsons["spam"];
                this.SDK = ((string)jsons["sdk"]).Contains('?') ? null : (long?)jsons["sdk"];
                this.IPSpoofing = ((string)jsons["ipsp"]).Contains('?') ? null : (long?)jsons["ipsp"];
                this.Spyware = ((string)jsons["spyware"]).Contains('?') ? null : (long?)jsons["spyware"];
                this.Money = ((string)jsons["money"]).Contains('?') ? null : (long?)jsons["money"];
                this.WinChance = ((string)jsons["winchance"]).Contains('?') ? null : (long?)jsons["winchance"];

                this.Anonymous = (string)jsons["anonymous"] == "YES";
                this.Name = (string)jsons["username"];

                var savings = (long)jsons["savings"];
                var winelo = (string)jsons["winelo"];

                this.LastUpdate = DateTime.Now;

                this.LastAttack = DateTime.MinValue;

                this.Attacks = new List<Attacks>();


            }
            catch (Exception)
            {

            }
        }

        public bool Equals(IPs other)
        {
            return other.IP == this.IP;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as IPs);
        }

        public override int GetHashCode()
        {
            return IP.GetHashCode();
        }

        public override string ToString()
        {
            return this.IP;
        }
    }
    
    public partial class Attacks : IComparable<Attacks>, IComparable
    {
        public int CompareTo(object obj)
        {
            return CompareTo(obj as Attacks);
        }

        public int CompareTo(Attacks other)
        {
            return Dt.CompareTo(other.Dt);
        }
    }

    public static class DbManager
    {
        private static readonly string DbVersion = "1.0.0";
        private static vhackdbEntities model;
        private static IConfig config;

        static DbManager()
        { }

        public static void Update() => model = new vhackdbEntities();

        public static bool Initialize(IConfig cfg)
        {
            config = cfg;

            //model = new vhackdbEntities(config.dbConnectionString);
            model = new vhackdbEntities();

            var splits = DbVersion.Split('.');
            var major = Convert.ToInt32(splits[0]);
            var minor = Convert.ToInt32(splits[1]);
            var patch = Convert.ToInt32(splits[2]);

            try
            {
                var lastVersion = model.Version
                    .ToArray()
                    .Last();

                // TODO: update db if necessary
            }
            catch (Exception e)
            {
                config.logger.Log("Cannot read database version: {0}", e.Message);
                return false;
            }

            return true;
        }

        public static bool IpExist(string ip) => model.IPs.Any(it => it.IP == ip);

        public static IPs AddIp(IPs ip)
        {
            try
            {
                var found = model.IPs.FirstOrDefault(it => ip.IP == it.IP);
                //var found = model.IPs.ToList().FirstOrDefault(it => ip.Equals(it));
                if (found != null)
                    return found;

                var added = model.IPs.Add(ip);
                model.SaveChanges();
                return added;
            }
            catch (Exception e)
            {
                config.logger.Log("DB error adding ip {0}: {1}", ip, e.Message);
                return null;
            }
        }

        public static IPs GetIp(string ip) => model.IPs.FirstOrDefault(it => ip == it.IP);

        public static IEnumerable<IPs> GetIps() => model.IPs.AsEnumerable();

        public static bool UpdateIp(IPs newIp)
        {
            var oldIp = GetIp(newIp.IP);
            if (oldIp == null)
                return false;

            try
            {
                newIp.CopyProperties(ref oldIp);
                oldIp.LastUpdate = DateTime.Now;
                model.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static bool AddAttack(string ip, Attacks attack)
        {
            var found = GetIp(ip);
            if (found == null)
                return false;

            if (found.Attacks == null)
                found.Attacks = new List<Attacks>();

            try
            {
                attack.IP = ip;
                attack.Dt = DateTime.Now;
                found.Attacks.Add(attack);

                model.SaveChanges();
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

}