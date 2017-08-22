using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using vHackApi.Api;
using vHackApi.Interfaces;

namespace vHackApi
{

    [XmlRoot]
    [XmlInclude(typeof(Attacks))]
    public class XmlMgr : Singleton<XmlMgr>, IPersistanceMgr
    {
        [XmlArray]
        public List<IPs> IPs { get; set; } = new List<IPs>();

        [XmlIgnore]
        string path = "./db.xml";

        [XmlIgnore]
        object semaphore = new object();

        public bool Save()
        {
            lock (semaphore)
            {
                try
                {
                    // make bakup
                    if (File.Exists(path))
                        File.Copy(path, path + ".bak", true);
                    
                    var doc = new XDocument(
                        new XElement("IPs",
                            IPs
                            //.ToArray()
                            .Where(ip => ip!= null)
                            .Select(ip => {
                                if (ip.Attacks == null)
                                    ip.Attacks = new HashSet<Attacks>();

                                //System.Console.WriteLine(ip.IP);

                                return new XElement("IP",
                                        new XAttribute("Anonymous", ip.Anonymous),
                                        new XAttribute("Antivirus", ip.Antivirus),
                                        new XAttribute("Firewall", ip.Firewall),
                                        new XAttribute("Hostname", !string.IsNullOrEmpty(ip.Hostname) ? ip.Hostname : "unknown"),
                                        new XAttribute("IP", !string.IsNullOrEmpty(ip.IP) ? ip.IP : ""),
                                        new XAttribute("IPSpoofing", ip.IPSpoofing),
                                        new XAttribute("LastAttack", ip.LastAttack),
                                        new XAttribute("LastUpdate", ip.LastUpdate),
                                        new XAttribute("Money", ip.Money),
                                        new XAttribute("Name", ip.Name == null ? "" : ip.Name),
                                        new XAttribute("RepOnSuccess", ip.RepOnSuccess),
                                        new XAttribute("SDK", ip.SDK),
                                        new XAttribute("Spam", ip.Spam),
                                        new XAttribute("Spyware", ip.Spyware),
                                        new XAttribute("WinChance", ip.WinChance),
                                        new XElement("Attacks", ip.Attacks
                                            .Select(att => new XElement("Attack",
                                                        new XAttribute("Dt", att.Dt == null ? vHackApi.IPs.MinDateTime : att.Dt),
                                                        new XAttribute("IP", att.IP == null ? "" : att.IP),
                                                        new XAttribute("MoneyOwned", att.MoneyOwned),
                                                        new XAttribute("MoneyWon", att.MoneyWon),
                                                        new XAttribute("RepWon", att.RepWon)))));
                            })));

                    doc.Save(path);

                    return true;
                }
                catch (Exception e)
                {
                    System.Console.WriteLine(e.ToString());
                    return false;
                } 
            }
        }

        public bool Load()
        {
            lock (semaphore)
            {
                try
                {
                    if (!File.Exists(path))
                    {
                        using (var file = File.Create(path))
                        {
                            var arr = Encoding.ASCII.GetBytes("<IPs/>");
                            file.Write(arr, 0, arr.Length);
                        }
                    }

                    var doc = XDocument.Load(path);
                    //this.IPs.Clear();
                    this.IPs = doc.Descendants("IPs")
                        .Elements("IP").Select(ip =>
                        {
                        //var ip = item.Element("IP");
                        return new IPs()
                            {
                                Anonymous = (bool)ip.Attribute("Anonymous"),
                                Antivirus = (long)ip.Attribute("Antivirus"),
                                Firewall = (long)ip.Attribute("Firewall"),
                                IP = (string)ip.Attribute("IP"),
                                IPSpoofing = (long)ip.Attribute("IPSpoofing"),
                                LastAttack = (DateTime)ip.Attribute("LastAttack"),
                                LastUpdate = (DateTime)ip.Attribute("LastUpdate"),
                                Money = (long)ip.Attribute("Money"),
                                Name = (string)ip.Attribute("Name"),
                                RepOnSuccess = (long)ip.Attribute("RepOnSuccess"),
                                SDK = (long)ip.Attribute("SDK"),
                                Spam = (long)ip.Attribute("Spam"),
                                Spyware = (long)ip.Attribute("Spyware"),
                                WinChance = (long)ip.Attribute("WinChance"),
                                Hostname = (string)ip.Attribute("Hostname"),
                                Attacks = ip.Element("Attacks").Descendants()
                                .Select(att =>
                                new Attacks()
                                {
                                    Dt = (DateTime)att.Attribute("Dt"),
                                    IP = (string)att.Attribute("IP"),
                                    MoneyOwned = (long)att.Attribute("MoneyOwned"),
                                    MoneyWon = (long)att.Attribute("MoneyWon"),
                                    RepWon = (long)att.Attribute("RepWon"),
                                }).ToList()
                            };
                        }
                        ).ToList();
                }
                catch (Exception e)
                {
                    return false;
                } 
            }



            return true;
        }

        public bool AddAttack(string iP, Attacks att)
        {
            try
            {
                var found = IPs.FirstOrDefault(it => it.IP == iP);
                if (found == null)
                    return false;

                if (found.Attacks == null)
                    found.Attacks = new HashSet<Attacks>();

                att.IP = iP;
                att.IPs = found;

                found.Attacks.Add(att);
                Update();
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public IPs AddIp(IPs dbIp)
        {
            this.IPs.Add(dbIp);
            Update();
            return dbIp;
        }

        public bool RemoveIp(string ips)
        {
            var found = IPs.FirstOrDefault(ip => ip.IP == ips);
            if (found == null)
                return false;

            if (!IPs.Remove(found))
                return false;

            Save();
            return true;
        }

        public IPs GetIp(string iP)
        {
            return IPs.FirstOrDefault(ip => ip.IP == iP);
        }

        public IEnumerable<IPs> GetIps()
        {
            return IPs;
        }

        public bool IpExist(string iP)
        {
            return IPs.Any(ip => ip.IP == iP);
        }

        public IEnumerable<IPs> ScannableIps()
        {
            var now = DateTime.Now;
            return IPs.Where(ip =>
            {
                return (now - ip.LastAttack) > TimeSpan.FromHours(1);
            });
        }

        DateTime lastUpdate = DateTime.MinValue;
        public void Update()
        {
            if (DateTime.Now - lastUpdate < TimeSpan.FromMinutes(5)) // saves every 5 minutes
                return;
            Save();
            lastUpdate = DateTime.Now;
        }

        public bool UpdateIp(IPs ip)
        {
            var idx = IPs.FindIndex(it => it.IP == ip.IP);
            var oldip = IPs[idx];
            var res = ip.CopyProperties(ref oldip);
            if (res)
                Update();
            return res;
        }
    }
}
