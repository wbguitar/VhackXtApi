using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Text.RegularExpressions;
using VhackXtApi.Api;

namespace VhackXtApi.Console
{
    public class Target
    {

        private String ip;
        private NetworkImage img;
        private int firewallLevel;
        private bool isWatched;
        private String hostname;

        IConfig config;

        public Target(IConfig cfg, String hostname, NetworkImage img)
        {
            config = cfg;
            this.img = img;
            this.hostname = hostname;
            resolveHostname();
            firewallLevel = img.getFirewallLevel();
            isWatched = img.checkForAnonymity();
        }

        public NetworkImage getNetworkImage()
        {
            return img;
        }

        public bool isWatchedByFBI()
        {

            return isWatched;

        }

        public int getFirewallLevel()
        {

            //Coming soon
            if (!isWatched)
            {

                return firewallLevel;

            }
            else
            {

                return 0;

            }

        }

        public String getIP()
        {

            return ip;

        }

        public async void resolveHostname()
        {

            ip = await Utils.StringRequest("user::::pass::::uhash::::hostname", config.username + "::::" + config.password + "::::" + Cons.uHash + "::::" + this.hostname, "vh_getIpByHostname.php");

        }

        public async Task<int> bruteforcePassword()
        {

            JObject json = await Utils.JSONRequest("user::::pass::::uhash::::target", config.username + "::::" + config.password + "::::" + Cons.uHash + "::::" + this.ip, "vh_vulnScan.php");
            try
            {
                PasswordImageHelper helper = new PasswordImageHelper(json);
                int result = helper.getIDOfRightImage();
                return result;
            }
            catch (IOException e)
            {
            }
            return 0;

        }

        public async Task<Connection> connect()
        {

            int decision = await bruteforcePassword();
            String s = await Utils.StringRequest("user::::pass::::uhash::::decision", config.username + "::::" + config.password + "::::" + Cons.uHash + "::::" + decision, "vh_createConnection.php");

            return new Connection(config, Regex.Split(s, "\r\n|\r|\n"), ip);

        }



    }

}