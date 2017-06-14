using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using vHackApi.Interfaces;
using vHackApi.Api;
using vHackApi.Interfaces.Reflection;

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
                this.Money = ((string)jsons["money"]).Contains('?') ? 0 : (int)jsons["money"];
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

    public class DbManager: Interfaces.Reflection.Singleton<DbManager>, IPersistanceMgr
    {
        private DbManager() { }

        private static readonly string DbVersion = "1.0.0";
        private vhackdbEntities model;
        private IConfig config;

        public void Update() => model = new vhackdbEntities();

        public bool Initialize(IConfig cfg)
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

        public bool IpExist(string ip) => model.IPs.Any(it => it.IP == ip);

        public IPs AddIp(IPs ip)
        {
            lock (semaphore)
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
        }

        public bool RemoveIp(string ip)
        {
            var found = GetIp(ip);
            if (found == null)
                return false;

            lock (semaphore)
            {
                try
                {
                    model.IPs.Remove(found);
                    model.SaveChanges();
                }
                catch (Exception e)
                {
                    config.logger.Log("RemoveIp error: {0}", e.Message);
                    return false;
                } 
            }
            return true;
        }

        public IPs GetIp(string ip) => model.IPs.FirstOrDefault(it => ip == it.IP);

        public IEnumerable<IPs> GetIps() => model.IPs.AsEnumerable();

        static object semaphore = new object();
        public bool UpdateIp(IPs newIp)
        {
            var oldIp = GetIp(newIp.IP);
            if (oldIp == null)
                return false;

            lock (semaphore)
            {
                try
                {
                    newIp.CopyProperties(ref oldIp);
                    oldIp.LastUpdate = DateTime.Now;
                    model.SaveChanges();
                }
                catch (Exception e)
                {
                    config.logger.Log("UpdateIp error: {0}", e.Message);
                    return false;
                } 
            }

            return true;
        }

        public bool AddAttack(string ip, Attacks attack)
        {
            var found = GetIp(ip);
            if (found == null)
                return false;

            if (found.Attacks == null)
                found.Attacks = new List<Attacks>();

            lock (semaphore)
            {
                try
                {
                    attack.IP = ip;
                    found.LastAttack = attack.Dt = DateTime.Now;
                    found.Attacks.Add(attack);

                    model.SaveChanges();
                }
                catch (Exception e)
                {
                    config.logger.Log("AddAttack error: {0}", e.Message);
                    return false;
                } 
            }
            return true;
        }

        public IEnumerable<IPs> ScannableIps()
        {
            var now = DateTime.Now;
            return GetIps()
                    .Where(ip =>
                    {
                        return (now - ip.LastAttack) > TimeSpan.FromHours(1);

                        //if (ip.Attacks == null || ip.Attacks.Count == 0)
                        //    return true; // not yet attacked

                        //var lastAttack = ip.Attacks
                        //    .Select(att => att.Dt)
                        //    .OrderByDescending(dt => dt)
                        //    .FirstOrDefault();

                        //return (now - lastAttack) > TimeSpan.FromHours(1);
                    });
        }
    }

}