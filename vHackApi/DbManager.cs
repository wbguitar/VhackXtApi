using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using vHackApi.Interfaces;
using vHackApi.Api;
using vHackApi.Interfaces.Reflection;
using System.Data.Entity;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;

namespace vHackApi
{
    public partial class vhackdbEntities
    {
        public vhackdbEntities(string connString)
            : base(connString)
        { }
    }

    [XmlInclude(typeof(Attacks))]
    public partial class IPs: IXmlSerializable
    {
        public static readonly DateTime MinDateTime = DateTime.Parse("2001/01/01");
        public IPs(JObject jsons) :base()
        {
            try
            {
                this.IP = (string)jsons["ipaddress"];

                this.Firewall = ((string)jsons["fw"]).Contains('?') ? 0 : (long)jsons["fw"];
                this.Antivirus = ((string)jsons["av"]).Contains('?') ? 0 : (long)jsons["av"];
                this.Spam = ((string)jsons["spam"]).Contains('?') ? 0 : (long)jsons["spam"];
                this.SDK = ((string)jsons["sdk"]).Contains('?') ? 0 : (long)jsons["sdk"];
                this.IPSpoofing = ((string)jsons["ipsp"]).Contains('?') ? 0 : (long)jsons["ipsp"];
                this.Spyware = ((string)jsons["spyware"]).Contains('?') ? 0 : (long)jsons["spyware"];
                this.Money = ((string)jsons["money"]).Contains('?') ? 0 : (int)jsons["money"];
                this.WinChance = ((string)jsons["winchance"]).Contains('?') ? 0 : (long)jsons["winchance"];

                this.Anonymous = (string)jsons["anonymous"] == "YES";
                this.Name = (string)jsons["username"];

                var savings = (long)jsons["savings"];
                var winelo = (string)jsons["winelo"];

                this.Hostname = "unknown";

                this.LastUpdate = DateTime.Now;

                this.LastAttack = MinDateTime;

                //this.Attacks = new List<Attacks>();


            }
            catch (Exception)
            {

            }
        }

        public bool Equals(IPs other)
        {
            try
            {
                return other != null ? other.IP == this.IP : false;
            }
            catch (Exception e)
            {
                throw;
            }
        }
        
        public override bool Equals(object obj)
        {
            try
            {
                return Equals(obj as IPs);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public override int GetHashCode()
        {
            try
            {
                return IP != null ? IP.GetHashCode() : 0;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            Name = reader.GetAttribute("Name");
            Anonymous = bool.Parse( reader.GetAttribute("Anonymous"));
            Antivirus = Int32.Parse(reader.GetAttribute("Antivirus"));
            Firewall = Int32.Parse(reader.GetAttribute("Firewall"));
            Hostname = reader.GetAttribute("Hostname");
            IP = reader.GetAttribute("IP");
            IPSpoofing = Int32.Parse(reader.GetAttribute("IPSpoofing"));
            LastUpdate = DateTime.Parse(reader.GetAttribute("LastUpdate"));
            Money = Int32.Parse(reader.GetAttribute("Money"));
            RepOnSuccess = Int32.Parse(reader.GetAttribute("RepOnSuccess"));
            SDK = Int32.Parse(reader.GetAttribute("SDK"));
            Spam = Int32.Parse(reader.GetAttribute("Spam"));
            Spyware = Int32.Parse(reader.GetAttribute("Spyware"));
            WinChance = Int32.Parse(reader.GetAttribute("WinChance"));
            
            Boolean isEmptyElement = reader.IsEmptyElement; // (1)
            Attacks.Clear();
            reader.ReadStartElement("Attacks");
            {
                var attr = reader.ReadAttributeValue();
                var count = int.Parse(reader.GetAttribute("count"));
                var ser = new XmlSerializer(typeof(Attacks));
                for (int i = 0; i < count; i++)
                {
                    var item = (Attacks)ser.Deserialize(reader);
                    Attacks.Add(item);
                }
            }
            reader.ReadEndElement();
        }

        public override string ToString()
        {
            try
            {
                return this.IP;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Name", Name);
            writer.WriteAttributeString("Anonymous", this.Anonymous.ToString());
            writer.WriteAttributeString("Antivirus", this.Antivirus.ToString());
            writer.WriteAttributeString("Firewall", this.Firewall.ToString());
            writer.WriteAttributeString("Hostname", this.Hostname);
            writer.WriteAttributeString("IP", this.IP);
            writer.WriteAttributeString("IPSpoofing", this.IPSpoofing.ToString());
            writer.WriteAttributeString("LastUpdate", this.LastUpdate.ToString());
            writer.WriteAttributeString("Money", this.Money.ToString());
            writer.WriteAttributeString("RepOnSuccess", this.RepOnSuccess.ToString());
            writer.WriteAttributeString("SDK", this.SDK.ToString());
            writer.WriteAttributeString("Spam", this.Spam.ToString());
            writer.WriteAttributeString("Spyware", this.Spyware.ToString());
            writer.WriteAttributeString("WinChance", this.WinChance.ToString());

            writer.WriteStartElement("Attacks");
            writer.WriteAttributeString("count", Attacks.Count.ToString());
            {
                var ser = new XmlSerializer(typeof(Attacks));
                foreach (var item in Attacks)
                {
                    //writer.WriteStartElement("Attack");
                    //writer.WriteAttributeString("dt", item.Dt.ToString());
                    //writer.WriteAttributeString("ip", item.IP);
                    //writer.WriteAttributeString("MoneyOwned", item.MoneyOwned.ToString());
                    //writer.WriteAttributeString("MoneyWon", item.MoneyWon.ToString());
                    //writer.WriteAttributeString("RepWon", item.RepWon.ToString());
                    //writer.WriteEndElement();
                    ser.Serialize(writer, item);
                }
            }
            writer.WriteEndElement();

        }
    }

    [XmlType]
    public class XmlIPs: IPs
    {
        public XmlIPs() : base() { }
        public XmlIPs(JObject other) : base(other) { }
        public XmlIPs(IPs other)
        {
            this.Anonymous = other.Anonymous;
            this.Antivirus = other.Antivirus;
            this.Attacks = other.Attacks.ToList();
            this.Firewall = other.Firewall;
            this.Hostname = other.Hostname;
            this.IP = other.IP;
            this.IPSpoofing = other.IPSpoofing;
            this.LastAttack = other.LastAttack;
            this.LastUpdate = other.LastUpdate;
            this.Money = other.Money;
            this.Name = other.Name;
            this.RepOnSuccess = other.RepOnSuccess;
            this.SDK = other.SDK;
            this.Spam = other.Spam;
            this.Spyware = other.Spyware;
            this.WinChance = other.WinChance;
        }

        [XmlIgnore]
        public override ICollection<Attacks> Attacks
        {
            get { return AttacksXml; }
            set { AttacksXml = value.ToList(); }
        }

        [XmlArray]
        public IList<Attacks> AttacksXml { get; set; }
    }
    
    public partial class Attacks : IComparable<Attacks>, IComparable, IXmlSerializable
    {
        public int CompareTo(object obj)
        {
            try
            {
                return CompareTo(obj as Attacks);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public int CompareTo(Attacks other)
        {
            try
            {
                return Dt.CompareTo(other.Dt);
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.MoveToContent();
            this.Dt = DateTime.Parse(reader.GetAttribute("Dt"));
            this.IP = reader.GetAttribute("IP");
            this.MoneyOwned = Int32.Parse(reader.GetAttribute("MoneyOwned"));
            this.MoneyWon = Int32.Parse(reader.GetAttribute("MoneyWon"));
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteAttributeString("Dt", Dt.ToString());
            writer.WriteAttributeString("Anonymous", IP);
            writer.WriteAttributeString("Antivirus", MoneyOwned.ToString());
            writer.WriteAttributeString("Firewall", MoneyWon.ToString());
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
            Update();
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
            Update();
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

        public IPs GetIp(string ip)
        {
            Update();
            return model.IPs.FirstOrDefault(it => ip == it.IP);
        }

        static object semaphore = new object();
        public bool UpdateIp(IPs newIp)
        {
            Update();
            var oldIp = GetIp(newIp.IP);
            if (oldIp == null)
                return false;

            lock (semaphore)
            {
                try
                {
                    if (string.IsNullOrEmpty(newIp.Hostname))
                        newIp.Hostname = "unknown";

                    //newIp.CopyProperties(ref oldIp);
                    oldIp.Hostname = newIp.Hostname;
                    oldIp.Anonymous = newIp.Anonymous;
                    oldIp.Antivirus = newIp.Antivirus;
                    oldIp.Attacks = newIp.Attacks;
                    oldIp.Firewall = newIp.Firewall;
                    //oldIp.IP = newIp.IP;
                    oldIp.IPSpoofing = newIp.IPSpoofing;
                    oldIp.LastAttack = newIp.LastAttack;
                    oldIp.LastUpdate = newIp.LastUpdate;
                    oldIp.Money = newIp.Money;
                    oldIp.Name = newIp.Name;
                    oldIp.RepOnSuccess = newIp.RepOnSuccess;
                    oldIp.SDK = newIp.SDK;
                    oldIp.Spam = newIp.Spam;
                    oldIp.Spyware = newIp.Spyware;
                    oldIp.WinChance = newIp.WinChance;
                    //oldIp.Attacks = newIp.Attacks;//.Distinct().ToList();
                    oldIp.LastUpdate = DateTime.Now;
                    model.SaveChanges();
                }
                catch (Exception e)
                {
                    config.logger.Log("UpdateIp error {0}: {1}", newIp.IP, e.Message);
                    return false;
                } 
            }

            return true;
        }

        public bool AddAttack(string ip, Attacks attack)
        {
            Update();
            var found = GetIp(ip);
            if (found == null)
                return false;

            if (found.Attacks == null)
                found.Attacks = new HashSet<Attacks>();

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


        public IQueryable<IPs> GetIpsQ()
        {
            Update();
            return model.IPs;
        }

        public IQueryable<IPs> ScannableIpsQ()
        {
            Update();
            var now = DateTime.Now;

            return from ip in model.IPs
                       where ip.LastAttack != null && DbFunctions.DiffHours(now, ip.LastAttack) > 1
                   //where now.Hour - ip.LastAttack.Hour > 1
                   select ip;




            //return GetIps()
            //        .Where(ip =>
            //        {
            //            return (now - ip.LastAttack) > TimeSpan.FromHours(1);

            //            //if (ip.Attacks == null || ip.Attacks.Count == 0)
            //            //    return true; // not yet attacked

            //            //var lastAttack = ip.Attacks
            //            //    .Select(att => att.Dt)
            //            //    .OrderByDescending(dt => dt)
            //            //    .FirstOrDefault();

            //            //return (now - lastAttack) > TimeSpan.FromHours(1);
            //        });
        }

        public IEnumerable<IPs> GetIps()
        {
            Update();
            lock (semaphore)
            {
                foreach (var ip in model.IPs)
                {
                    yield return ip;
                }
            }
        }

        public IEnumerable<IPs> ScannableIps()
        {
            Update();
            lock (semaphore)
            {
                var now = DateTime.Now;
                foreach (var ip in model.IPs)
                {
                    if ((now - ip.LastAttack) > TimeSpan.FromHours(1))
                        yield return ip;
                } 
            }
        }
    }

}