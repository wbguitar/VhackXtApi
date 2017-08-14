using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Tesseract;
using vHackApi.Api;
using vHackApi.Interfaces;
using System.Linq;

namespace vHackApi.Console
{
    public class vhConsole
    {
        static int waitStepDef = 500;
        public static int WaitStep => config == null ? waitStepDef : config.waitstep;

        public static IConfig config { get; private set; }
        public static String uHash { get; private set; }

        public vhConsole(IConfig cfg, String uHash)
        {
            vhConsole.uHash = uHash;
            config = cfg;

            //var info = MyInfo().Result;
            //vhConsole.uHash = (string)info["uhash"];

            if (!Directory.Exists(cfg.tessdata))
            {
                cfg.logger.Log("Cannot find tessdata path: {0}", Path.GetFullPath(cfg.tessdata));
                //throw new Exception();
            }

            engine = new Tesseract.TesseractEngine(cfg.tessdata, "eng");

        }

        public async Task<JObject> MyInfo()
        {
            return await vhUtils.JSONRequest("user::::pass::::gcm::::uhash",
                                     config.username + "::::" + config.password + "::::" + "eW7lxzLY9bE:APA91bEO2sZd6aibQerL3Uy-wSp3gM7zLs93Xwoj4zIhnyNO8FLyfcODkIRC1dc7kkDymiWxy_dTQ-bXxUUPIhN6jCUBVvGqoNXkeHhRvEtqAtFuYJbknovB_0gItoXiTev7Lc5LJgP2" + "::::" + "userHash_not_needed",
                                     "vh_update.php");
        }

        public async Task<JObject> RequestPassword(string ip)
        {
            return await vhUtils.JSONRequest("user::::pass::::target",
                                   config.username + "::::" + config.password + "::::" + ip,
                                   "vh_vulnScan.php");
        }

        public async Task<JObject> EnterPassword(string target, string uhash = null, string sol = null)
        {
            if (string.IsNullOrEmpty(uhash))
                uhash = vhConsole.uHash;

            if (!string.IsNullOrEmpty(sol))
            {
                var pass = sol.Split('p');
                return await vhUtils.JSONRequest("user::::pass::::port::::target::::uhash",
                                         config.username + "::::" + config.password + "::::" +
                                             pass[1] + "::::" + target + "::::" + uhash,
                                         "vh_trTransfer.php", 3);
                
            }

            return await vhUtils.JSONRequest("user::::pass::::target::::uhash",
                                   config.username + "::::" + config.password + "::::" +
                                       target + "::::" + uhash,
                                   "vh_trTransfer.php", 3);
        }

        public async Task<JObject> CheckCluster(string uhash = null)
        {
            uhash = string.IsNullOrEmpty(uhash) ? "userHash_not_needed" : uhash;
            return await vhUtils.JSONRequest("user::::pass::::uhash",
                                     config.username + "::::" + config.password + "::::" + uhash,
                                     "vh_ClusterData.php");
        }

        //public async Task<JObject> scanUser()
        //{
        //    //var req = await Utils.StringRequest("user::::pass::::",
        //    //                       config.username + "::::" + config.password + "::::", "vh_scanHost.php");
        //    return await vhUtils.JSONRequest("user::::pass::::",
        //                           config.username + "::::" + config.password + "::::", "vh_scanHost.php");
        //}

        public async Task<string> GetTournamentPosition()
        {
            return await vhUtils.StringRequest("user::::pass::::uhash",
                                     config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                                     "vh_tournamentData.php");
        }

        public async Task<string> AttackCluster(string tag)
        {
            return await vhUtils.StringRequest("user::::pass::::uhash::::ctag",
                                     config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + tag,
                                     "vh_startDDoS.php");
        }

        public async Task<string> ScanCluster(string tag)
        {
            return await vhUtils.StringRequest("user::::pass::::uhash::::ctag",
                                     config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + tag,
                                     "vh_scanTag.php");
        }

        public async Task<JObject> transferMoney(string ip)
        {
            return await vhUtils.JSONRequest("user::::pass::::target",
                                   config.username + "::::" + config.password + "::::" + ip,
                                   "vh_trTransfer.php");
        }

        public void clearLog()
        {
            throw new NotImplementedException();
        }

        public async Task<bool> uploadSpyware(string ip)
        {
            var res = await vhUtils.StringRequest("user::::pass::::target",
                                  config.username + "::::" + config.password + "::::" + ip,
                                  "vh_spywareUpload.php");
            return res == "0" ? true : false;
        }

        public async Task<bool> getTournament()
        {
            var temp = await vhUtils.JSONRequest("user::::pass::::uhash",
                                     config.username + "::::" + config.password + "::::" + "UserHash_not_needed",
                                     "vh_update.php");
            var val = (int)temp["tournamentActive"];
            return val == 2;
        }

        private static Random r = new Random((int)DateTime.Now.Ticks);

        public enum ScanMode { Secure, Potator };

        public TesseractEngine engine;

        /// <summary>
        /// Process an image extracting host info and attacks it if possible
        /// </summary>
        /// <param name="imgstring">host image represented by its bytes string</param>
        /// <param name="hostname"></param>
        /// <param name="mode"></param>
        /// <returns></returns>
        public async Task<int> ProcessImgAndAttack(string imgstring, string hostname, ScanMode mode)
        {
            var image = new NetworkImage(imgstring);
            string text = "";
            Page page = null;
            try
            {
                // the image is made of three lines:
                // line 1: "Hostname:"
                // line 2: hostname
                // line 3: firewall level
                page = engine.Process(new Bitmap(image.image));
                text = page.GetText();

                // get subimage for the second line: if orange the host is already been hacked
                var subimgHostname = image.GetSubImage(0, image.image.Height / 3, image.image.Width, image.image.Height / 3);
                // get subimage for the third line: if red the host is watched by FBI
                var subimgFwall = image.GetSubImage(0, image.image.Height / 3 * 2, image.image.Width, image.image.Height / 3);

                
               

                var hackedColor = Color.FromArgb(255, 250, 152, 25);
                var watchedbyFBIColor = Color.FromArgb(255, 136, 0, 0);
                if (hasColor(subimgHostname, hackedColor))
                {
                    //var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName() + ".png");
                    //subimgHostname.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

                    

                    config.logger.Log("Host {0} already hacked, skip", hostname);
                    // only updates hostname
                    var scan = await ScanHost(hostname, 10);
                    if (scan != null)
                    {
                        var ip = (string)scan["ipaddress"];
                        var ips = config.persistanceMgr.GetIp(ip);
                        if (ips != null && ips.Hostname == "unknown")
                        {
                            ips.Hostname = hostname;
                            if (config.persistanceMgr.UpdateIp(ips))
                                config.logger.Log("Updated hostname {0} for ip {1}", ips.Hostname, ips.IP);
                        }
                    }
                    return 1;
                }
                if (hasColor(subimgFwall, watchedbyFBIColor))
                {
                    //if (vhUtils.IsContestRunning())
                    //{
                    //    var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    //        , "FBI_WATCHED_" + hostname + "_" + Path.GetRandomFileName() + ".png");
                    //    image.image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    //}

                    //var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName() + "_FBI.png");
                    //subimgFwall.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                    config.logger.Log("Host {0} is watched by FBI!! skipping", hostname);
                    return 1;
                }
                else
                {
                    
                }
                //for (int i = 0; i < subimgHostname.Width; i++)
                //{
                //    int j = 0;
                //    for (; j < subimgHostname.Height; j++)
                //    {
                //        var pix = subimgHostname.GetPixel(i, j);
                //        if (pix == hackedColor)
                //        {
                //            config.logger.Log("Host {0} already hacked, skip", hostname);
                //            // only updates hostname
                //            var scan = await ScanHost(hostname, 10);
                //            if (scan != null)
                //            {
                //                var ip = (string)scan["ipaddress"];
                //                var ips = config.persistanceMgr.GetIp(ip);
                //                if (ips != null && ips.Hostname == "unknown")
                //                {
                //                    ips.Hostname = hostname;
                //                    if (config.persistanceMgr.UpdateIp(ips))
                //                        config.logger.Log("Updated hostname {0} for ip {1}", ips.Hostname, ips.IP);
                //                }
                //            }
                //            return 1;
                //        }
                //        //if (pix.R != 0)
                //        //    break;
                //    }
                //    if (j < subimgHostname.Height)
                //        break;
                //}
            }
            catch (Exception e)
            {
                Debug.Print(e.ToString());
                return -1;
            }
            finally
            {
                if (page != null) page.Dispose();
            }

            if (text.Contains("Matched by the FBI") || text.Contains("Watched by the FBI"))
            {
                //if (vhUtils.IsContestRunning())
                //{
                //    var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                //        , "FBI_WATCHED_" + hostname + "_" + Path.GetRandomFileName() + ".png");
                //    image.image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                //}

                config.logger.Log("Host {0} is watched by FBI!! skipping", hostname);
                return 1;
            }
            else
            {
                var firewall = text.Split(':');
                if (firewall.Length > 2)
                {
                    var val = Convert.ToInt32(firewall[2].Trim());
                    if (val > config.maxFirewall)
                    {
                        config.logger.Log("Firewall is too high: {0}", val);
                    }
                    else
                    {
                        var scan = await ScanHost(hostname, 10);
                        if (scan == null)
                        {
                            config.logger.Log("Unable to scan host {0}", hostname);
                            return -1;
                        }

                        var ip = (string)scan["ipaddress"];
                        var vuln = (int)scan["vuln"];
                        if (!ip.Contains(".vHack.cc") && vuln == 1)
                        {
                            if (mode == ScanMode.Secure)
                                Thread.Sleep(r.Next(1, 3) * vhConsole.WaitStep);
                            else if (mode == ScanMode.Potator)
                                Thread.Sleep(r.Next(0, 1) * vhConsole.WaitStep);

                            try
                            {
                                var ips = config.persistanceMgr.GetIp(ip);
                                if (ips != null && ips.Hostname == "unknown")
                                {
                                    ips.Hostname = hostname;
                                    if (config.persistanceMgr.UpdateIp(ips))
                                        config.logger.Log("Updated hostname {0} for ip {1}", ips.Hostname, ips.IP);
                                }

                                //if (vhUtils.IsContestRunning())
                                //{
                                //    var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                                //        , "FBI_FREE_" + hostname + "_" + Path.GetRandomFileName() + ".png");
                                //    image.image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                                //}

                                var res = await AttackIp(ip);

                                // remove spyware
                                // TODO
                                //var upd = new Update(config);
                                //var spy = await upd.SpywareInfo();
                                //var splt = (string)(spy[0]);
                                //if (!string.IsNullOrEmpty(splt)) //TODO
                                //{
                                //    var removed = await upd.removeSpyware();
                                //}

                                if (res == -1)
                                    config.logger.Log("Attack to IP {0} failed\n", ip);

                                return res;
                            }
                            catch (Exception exc)
                            {
                                config.logger.Log("Error: {0}", exc.ToString());
                                return -1;
                            }
                        }
                    }
                }
                else
                {
                    config.logger.Log("Text not recognized:\n\n{0}\n\n", text);
                    return -1;
                }
            }

            return -1;
        }

        public async Task<JObject> ScanHost(string hostname, int attempts = 3)
        {
            return await vhUtils.JSONRequest("user::::pass::::hostname",
                config.username + "::::" + config.password + "::::" + hostname,
                "vh_scanHost.php", attempts);
        }

        public async Task<JObject> ScanIp(string ip, int attempts = 3)
        {
            //    return await vhUtils.JSONRequest("user::::pass::::uhash::::target",
            //                             config.username + "::::" + config.password + "::::" + uhash + "::::" + ip,
            //                             "vh_loadRemoteData.php");

            var uhash = vhConsole.uHash;

            return await vhUtils.JSONRequest("user::::pass::::uhash::::target",
                                     config.username + "::::" + config.password + "::::" + uhash + "::::" + ip,
                                     "vh_loadRemoteData.php", attempts);
        }

        private object semaphore = new object();

        /// <summary>
        /// Attacks a given ip
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="mode"></param>
        /// <returns>0 on success, -1 on failure, 1 on skip</returns>
        public async Task<int> AttackIp(string ip, ScanMode mode = ScanMode.Potator)
        {
            var info = await MyInfo();
            var uhash = (string)info["uhash"];

            if (mode == ScanMode.Secure)
                Thread.Sleep(vhConsole.WaitStep);

            config.logger.Log("attacking IP {0}", ip);

            // loads IP related data, if it can be scanned
            //var jsons = await vhUtils.JSONRequest("user::::pass::::uhash::::target",
            //                         config.username + "::::" + config.password + "::::" + uhash + "::::" + ip,
            //                         "vh_loadRemoteData.php");
            var jsons = await ScanIp(ip, 3);
            if (jsons == null)
            {
                config.persistanceMgr.RemoveIp(ip);
                config.logger.Log("Unable to scan ip {0}, removing", ip);
                return 1;
            }

            var dbIp = new IPs(jsons);

            // create DB ip
            if (config.persistanceMgr.IpExist(dbIp.IP))
                dbIp = config.persistanceMgr.GetIp(dbIp.IP);
            else
                dbIp = config.persistanceMgr.AddIp(dbIp);

            Debug.Assert(dbIp != null);

            try
            {
                //last update: port not requested anymore
                var port = "";
                //var ocr = new OCR(engine);

                //var port = ocr.getSolution(jsons);
                //if (port == "p0")
                //{
                //    config.logger.Log("unable to find the port :(");
                //    return -1;
                //}

                var user = (string)jsons["username"];
                var winchance = ((string)jsons["winchance"]).Contains("?") ? 0 : (int)jsons["winchance"];

                if (winchance <= config.winchance)
                {
                    config.logger.Log("Winchance too low: {0}, skipping", winchance);
                    return 1;
                }

                var fwlevel = (string)jsons["fw"];
                var avlevel = (string)jsons["av"];
                var spamlevel = (string)jsons["spam"];
                var sdklevel = (string)jsons["sdk"];
                var ipsplevel = (string)jsons["ipsp"];
                var money = (string)jsons["money"];
                var saving = (string)jsons["savings"];
                var anonymous = (string)jsons["anonymous"];
                var winlo = (string)jsons["winelo"];
                var spywarelevel = (string)jsons["spyware"];

                var res = jsons["result"];

                // if not dev ip update stats
                if (ip != "127.0.0.1")
                {
                    
                    config.persistanceMgr.UpdateIp(new IPs(jsons) { Attacks = dbIp.Attacks });
                }

                //if (mode == ScanMode.Potator)
                //{
                //    //var solution = (string)jsons.GetValue(sol);
                //    var pass = enterPassword(sol, ip, uhash).Result;
                //    jsons = JObject.Parse(pass);

                //    if (!money.Contains("?") && (int)jsons["result"] == 0)
                //    {
                //    }
                //}

                //if (money.Contains("?"))
                //{
                //    config.logger.Log("Cannot scan money, skipping");
                //    return 1;
                //}

                //if (sdklevel.Contains("?"))
                //{
                //    config.logger.Log("Cannot scan SDK, skipping");
                //}
                //else if (Convert.ToInt32(sdklevel) > (int)info["sdk"])
                //{
                //    config.logger.Log("SDK is too hign, skipping");
                //}
                //else if (avlevel.Contains("?") )
                //{
                //    config.logger.Log("Cannot scan antivirus, skipping");
                //    return 1;
                //}

                if (!config.hackIfNotAnonymous && anonymous != "YES")
                {
                    config.logger.Log("Not anonymous, skipping ip {0}", ip);
                    return 1;
                }

                if (config.maxAntivirus < Convert.ToInt32(avlevel))
                {
                    //config.logger.log("antivirus is too hign, skipping");
                    //return 1;
                }
                

                JObject pass = await EnterPassword(ip, uhash, port);
                if (pass == null)
                {
                    config.logger.Log("Unable to enter password for ip {0}", ip);
                    return -1;
                }

                int retval = 0;
                var result = (int)pass["result"];

                if (result == 0)
                {
                    // success!!
                    config.logger.Log(@"
Your Money: {0}
[TargetIP: {1}]
Made {2} and {3} Rep.
Antivirus: {4} Firewall: {5} Sdk: {6} TotalMoney: {7}
YourWinChance: {8} Anonymous:{9} username: {10} saving: {11}"
, (string)pass["newmoney"]
, ip
, (string)pass["amount"], (string)pass["eloch"]
, avlevel, fwlevel, sdklevel, money
, winchance, anonymous, user, saving);

                    var att = new Attacks();
                    att.MoneyWon = (long)pass["amount"];
                    att.RepWon = (long)pass["eloch"];
                    att.MoneyOwned = money.Contains("?") ? 0 : (long)jsons["money"];
                    
                    var addRes = config.persistanceMgr.AddAttack(dbIp.IP, att);
                    Debug.Assert(addRes);

                    retval = 0;
                }
                else
                {
                    var att = new Attacks();
                    //att.MoneyWon = (long)pass["amount"];
                    //att.RepWon = (long)pass["eloch"];
                    //att.MoneyOwned = money.Contains("?") ? null : (long?)jsons["money"];

                    if (dbIp.IP != "127.0.0.1")
                    {
                        var addRes = config.persistanceMgr.AddAttack(dbIp.IP, att);
                        Debug.Assert(addRes);
                    }

                    config.logger.Log("enter password failed");
                    retval = -1;
                }

                return retval;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        bool hasColor(Bitmap subimgHostname, Color findColor)
        {
            for (int i = 0; i < subimgHostname.Width; i++)
            {
                int j = 0;
                for (; j < subimgHostname.Height; j++)
                {
                    var pix = subimgHostname.GetPixel(i, j);
                    if (pix == findColor)
                        return true;
                }
            }

            return false;
        }

        public async Task<int> attackIp2(string ip, string uhash, ScanMode mode)
        {
            var password = await vhUtils.JSONRequest("user::::pass::::uhash::::target",
                                     config.username + "::::" + config.password + "::::" + uhash + "::::" + ip,
                                     "vh_loadRemoteData.php");
            //var password = await requestPassword(ip);
            var pwd = new PasswordImage((string)password["img"]);
            return 0;
        }

        public async Task<int> FindHostsAndAttack(ScanMode mode = ScanMode.Potator, bool active_protecte_cluster_ddos = false)
        {
            //string uhash = "";
            //try
            //{
            //    var info = await this.MyInfo();
            //    uhash = (string)info["uhash"];
            //}
            //catch (Exception e)
            //{
            //    config.logger.Log("Error: {0}", e.Message);
            //}

            //var stats = await CheckCluster(vhConsole.uHash);
            //var clusterBlocked = (string)stats["blocked"];
            //if (clusterBlocked.Contains("Your Cluster is blocked") && active_protecte_cluster_ddos)
            //{
            //    config.logger.Log("Cluster blocked, skipping"); // TODO
            //}
            //else
            {
                var temp = await GetImg(vhConsole.uHash);
                var data = (JArray)temp["data"];

                foreach (var d in data)
                {
                    try
                    {
                        var hostname = (string)d["hostname"];

                        // the purpose of this guy is to search for new ips: if this hostname
                        // is one of the ones already stored we pass away, someone else will
                        // hack him some time, sooner or later

                        var found = config.persistanceMgr
                            .GetIps()
                            .FirstOrDefault(ip => ip.Hostname == hostname);
                       
                        if (found != null)
                            continue; // skips

                        //var imgString = "data: image/png;base64," + (string)d["img"];
                        var imgString = (string)d["img"];
                        var res = await ProcessImgAndAttack(imgString, hostname, mode);
                    }
                    catch (Exception e)
                    {
                        config.logger.Log(e.Message);
                    }
                }
            }

            return 0;
        }

        public async Task<JObject> GetImg(string uhash)
        {
            return await vhUtils.JSONRequest("user::::pass::::uhash::::by",
                                         config.username + "::::" + config.password + "::::" + uhash + "::::" + r.Next(9000, 10000),
                                         "vh_getImg.php");
        }
    }
}