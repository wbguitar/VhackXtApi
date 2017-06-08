using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VhackXtApi.Api;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using Tesseract;
using System.Threading;

namespace VhackXtApi.Console
{
    public class Cons
    {
        public static int WaitStep { get; private set; }

        public static IConfig config { get; private set; }
        public static String uHash { get; private set; }

        static Cons()
        {
            WaitStep = 500;
        }

        public Cons(IConfig cfg, String uHash)
        {
            Cons.uHash = uHash;
            config = cfg;

            if (!Directory.Exists(cfg.tessdata))
            {
                cfg.logger.Log("Cannot find tessdata path: {0}", Path.GetFullPath(cfg.tessdata));
                throw new Exception();

            }

            engine = new Tesseract.TesseractEngine(cfg.tessdata, "eng");

            WaitStep = cfg.waitstep;
        }

        public async Task<JObject> myinfo()
        {
            return await Utils.JSONRequest("user::::pass::::gcm::::uhash",
                                     config.username + "::::" + config.password + "::::" + "eW7lxzLY9bE:APA91bEO2sZd6aibQerL3Uy-wSp3gM7zLs93Xwoj4zIhnyNO8FLyfcODkIRC1dc7kkDymiWxy_dTQ-bXxUUPIhN6jCUBVvGqoNXkeHhRvEtqAtFuYJbknovB_0gItoXiTev7Lc5LJgP2" + "::::" + "userHash_not_needed",
                                     "vh_update.php");
        }

        public async Task<JObject> requestPassword(string ip)
        {
            return await Utils.JSONRequest("user::::pass::::target",
                                   config.username + "::::" + config.password + "::::" + ip,
                                   "vh_vulnScan.php");
            
        }

        public async Task<JObject> enterPassword(string passwd, string target, string uhash)
        {
            var pass = passwd.Split('p');
            var temp = await Utils.JSONRequest("user::::pass::::port::::target::::uhash",
                                     config.username + "::::" + config.password + "::::" +
                                         pass[1] + "::::" + target + "::::" + uhash,
                                     "vh_trTransfer.php");

            return temp;
        }

        public async Task<JObject> check_Cluster(string uhash = null)
        {
            uhash = string.IsNullOrEmpty(uhash) ? "userHash_not_needed" : uhash;
            return await Utils.JSONRequest("user::::pass::::uhash",
                                     config.username + "::::" + config.password + "::::" + uhash,
                                     "vh_ClusterData.php");
        }

        public async Task<JObject> scanUser()
        {
            //var req = await Utils.StringRequest("user::::pass::::",
            //                       config.username + "::::" + config.password + "::::", "vh_scanHost.php");
            return await Utils.JSONRequest("user::::pass::::",
                                   config.username + "::::" + config.password + "::::", "vh_scanHost.php");
        }

        public async Task<string> GetTournamentPosition()
        {
            return await Utils.StringRequest("user::::pass::::uhash",
                                     config.username + "::::" + config.password + "::::" + "userHash_not_needed",
                                     "vh_tournamentData.php");
        }

        public async Task<string> AttackCluster(string tag)
        {
            return await Utils.StringRequest("user::::pass::::uhash::::ctag",
                                     config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + tag, 
                                     "vh_startDDoS.php");
        }

        public async Task<string> ScanCluster(string tag)
        {
            return await Utils.StringRequest("user::::pass::::uhash::::ctag",
                                     config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::"  + tag, 
                                     "vh_scanTag.php");
        }

        public async Task<JObject> transferMoney(string ip)
        {
            return await Utils.JSONRequest("user::::pass::::target",
                                   config.username + "::::" + config.password + "::::" + ip,
                                   "vh_trTransfer.php");
        }

        public void clearLog() { throw new NotImplementedException(); }

        public async Task<bool> uploadSpyware(string ip)
        {
            var res = await Utils.StringRequest("user::::pass::::target",
                                  config.username + "::::" + config.password + "::::" + ip,
                                  "vh_spywareUpload.php");
            return res == "0" ? true : false;
        }

        public async Task<bool> getTournament()
        {
            var temp = await Utils.JSONRequest("user::::pass::::uhash",
                                     config.username + "::::" + config.password + "::::" + "UserHash_not_needed",
                                     "vh_update.php");
            var val = (int)temp["tournamentActive"];
            return val == 2;
        }

        static Random r = new Random((int)DateTime.Now.Ticks);


        public enum ScanMode { Secure, Potator };
        public TesseractEngine engine;
        

        public async Task<int> calc_img(string imgstring, string uhash, string hostname, int max, ScanMode mode)
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
                var subimg = image.GetSubImage(0, image.image.Height / 3, image.image.Width, image.image.Height / 3);
                //var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName() + ".png");
                //subimg.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
                var hackedColor = Color.FromArgb(255, 250, 152, 25);
                for (int i = 0; i < subimg.Width; i++)
                {
                    int j = 0;
                    for (; j < subimg.Height; j++)
                    {
                        var pix = subimg.GetPixel(i, j);
                        if (pix == hackedColor)
                        {
                            config.logger.Log("Host {0} already hacked, skip", hostname);
                            return 1;
                        }
                        //if (pix.R != 0)
                        //    break;
                    }
                    if (j < subimg.Height)
                        break;
                }
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
                config.logger.Log("Host {0} is watched by FBI!! skipping", hostname);
                return 1;
            }
            else
            {
                var firewall = text.Split(':');
                if (firewall.Length > 2)
                {
                    var val = Convert.ToInt32(firewall[2].Trim());
                    if (val > max) // TODO: insert max firewall from config
                    {
                        config.logger.Log("Firewall is too high: {0}", val);
                    }
                    else {
                        var scan = await scanHost(uhash, hostname);
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
                                Thread.Sleep(r.Next(1, 3) * Cons.WaitStep);
                            else if (mode == ScanMode.Potator)
                                Thread.Sleep(r.Next(0, 1) * Cons.WaitStep);

                            try
                            {
                                var res = await attackIp(ip, 8000);

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
                                    config.logger.Log("\nAttack to IP {0} failed\n", ip);

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

        public async Task<JObject> scanHost(string uhash, string hostname)
        {
            //return await Utils.JSONRequest("user::::pass::::uhash::::hostname",
            //    config.username + "::::" + config.password + "::::" + "userHash_not_needed" + "::::" + hostname,
            //    "vh_scanHost.php");

            return await Utils.JSONRequest("user::::pass::::hostname",
                config.username + "::::" + config.password + "::::" + hostname,
                "vh_scanHost.php");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="max"></param>
        /// <param name="mode"></param>
        /// <returns>0 on success, -1 on failure, 1 on skip</returns>
        public async Task<int> attackIp(string ip, int max, ScanMode mode = ScanMode.Potator)
        {
            var info = await myinfo();
            var uhash = (string)info["uhash"];

            if (mode == ScanMode.Secure)
                Thread.Sleep(Cons.WaitStep);

            config.logger.Log("attacking IP {0}", ip);

            var jsons = await Utils.JSONRequest("user::::pass::::uhash::::target",
                                     config.username + "::::" + config.password + "::::" + uhash + "::::" + ip,
                                     "vh_loadRemoteData.php");

            var ocr = new OCR(engine);
            try
            {
                var sol = ocr.getSolution(jsons);
                if (sol == "p0")
                {
                    config.logger.Log("unable to find the password :(");
                    return -1;
                }
                else
                {

                }
                var user = (string)jsons["username"];
                var winchance = ((string)jsons["winchance"]).Contains("?") ? 0 : (int)jsons["winchance"];

                if (winchance <= config.winchance)
                {
                    config.logger.Log("Winchance too poor: {0}, skipping", winchance);
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

                if (sdklevel.Contains("?"))
                {
                    config.logger.Log("Cannot scan SDK, skipping");
                }
                else if (Convert.ToInt32(sdklevel) > (int)info["sdk"])
                {
                    config.logger.Log("SDK is too hign, skipping");
                }
                else if (avlevel.Contains("?") )
                {
                    //config.logger.Log("Cannot scan antivirus, skipping");
                    //return 1;
                }
                else if (max < Convert.ToInt32(avlevel))
                {
                    //config.logger.Log("Antivirus is too hign, skipping");
                    //return 1;
                }
                else if (anonymous.ToLower() != "yes")
                {
                    //config.logger.Log("Not anonymous, skipping");
                }
                else
                {
                    JObject pass = null;
                    do
                    {
                        // loops while get result
                        pass = await enterPassword(sol, ip, uhash);
                    } while (pass == null);

                    if ((int)pass["result"] == 0)
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
, (string)pass["amount"], (string)pass["eloch"], avlevel
, fwlevel, sdklevel, money
, winchance, anonymous, user, saving);
                        return 0;
                    }
                    else
                    {
                        config.logger.Log("enter password failed");
                        return -1;
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }

            return -1;
        }

        public async Task<int> attackIp2(string ip, string uhash, int max, ScanMode mode)
        {
            var password = await Utils.JSONRequest("user::::pass::::uhash::::target",
                                     config.username + "::::" + config.password + "::::" + uhash + "::::" + ip,
                                     "vh_loadRemoteData.php");
            //var password = await requestPassword(ip);
            var pwd = new PasswordImage((string)password["img"]);
            return 0;
        }

        public async Task<int> getIP(int max, ScanMode mode = ScanMode.Potator, bool active_protecte_cluster_ddos = false)
        {
            string uhash = "";
            try
            {
                var info = await this.myinfo();
                uhash = (string)info["uhash"];
            }
            catch (Exception e)
            {
                config.logger.Log("Error: {0}", e.Message);
            }

            var stats = await check_Cluster(uhash);
            var clusterBlocked = (string)stats["blocked"];
            if (clusterBlocked.Contains("Your Cluster is blocked") && active_protecte_cluster_ddos)
            {
                config.logger.Log("Cluster blocked, skipping"); // TODO
            }
            else
            {
                var temp = await getImg(uhash);
                var data = (JArray)temp["data"];

                List<dynamic> objs = new List<dynamic>();
                List<dynamic> whitelist = new List<dynamic>();
                foreach (var d in data)
                {
                    try
                    {
                        var hostname = (string)d["hostname"];
                        //var imgString = "data: image/png;base64," + (string)d["img"];
                        var imgString = (string)d["img"];
                        var res = await calc_img(imgString, uhash, hostname, max, mode);
                    }
                    catch (Exception e)
                    {
                        config.logger.Log(e.Message);
                    }
                }
            }

            return 0;
        }

        public async Task<JObject> getImg(string uhash)
        {
            return await Utils.JSONRequest("user::::pass::::uhash::::by",
                                         config.username + "::::" + config.password + "::::" + uhash + "::::" + r.Next(0, 1000), 
                                         "vh_getImg.php");
        }


        //Random r = new Random();
        //public async Task<Target[]> getTargets()
        //{

        //    JObject json = await Utils.JSONRequest("user::::pass::::uhash::::by", config.username + "::::" + config.password 
        //        + "::::" + uHash + "::::" + r.Next(1).ToString(), "vh_getImg.php");

        //    JArray jarray = (JArray)json.GetValue("data");
        //    Target[] targets = new Target[jarray.Count];

        //    for (int i = 0; i < jarray.Count; i++)
        //    {
        //        try
        //        {
        //            var tempjo = (JObject)jarray[i];
        //            var host = (string)tempjo.GetValue("hostname");
        //            var img = (string)tempjo.GetValue("img");
        //            var nimg = new NetworkImage(img);
        //            nimg.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName()) + ".png");
        //            targets[i] = new Target(host,nimg);

        //        }
        //        catch (Exception e)
        //        {
        //            Debug.Print(e.ToString());
        //        }
        //    }

        //    //for (int i = 0; i < jarray.Count; i++)
        //    //{

        //    //    JObject tempjo = jarray.getJSONObject(i);
        //    //    try
        //    //    {

        //    //        targets[i] = new Target(tempjo.getString("hostname"), new NetworkImage(tempjo.getString("img")));

        //    //    }
        //    //    catch (Exception e)
        //    //    {

        //    //    }

        //    //}

        //    return targets;

        //}
    }
}