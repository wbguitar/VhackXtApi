using System;
using System.Threading.Tasks;
using vHackApi.Api;

namespace vHackApi.Console
{
    public class Connection
    {
        IConfig config;
        private String IP;
        private bool success = true;
        private String username,
                firewallLevel,
                antiVirusLevel,
                scanLevel,
                sdkLevel,
                spamLevel,
                money,
                anonymous,
                successRep,
                failRep,
                successRate;

        public Connection(IConfig cfg, String[] result, String ip)
        {
            config = cfg;

            if (result.Length == 1)
            {
                success = false;
                return;
            }
            if (result[1] == null)
            {
                success = false;
            }
            else
            {
                username = result[1].Substring(26);
                firewallLevel = result[2].Substring(26);
                antiVirusLevel = result[3].Substring(27);
                scanLevel = result[4].Substring(22);
                sdkLevel = result[5].Substring(21);
                spamLevel = result[6].Substring(22);
                money = result[7].Substring(23);
                anonymous = result[9].Substring(27);
                successRep = result[11].Substring(32);
                failRep = result[12].Substring(29);
                successRate = result[13].Substring(39);
            }

        }

        //public async Task<SpywareUploadResult> spywareUpload(){

        //    String returnString = await Utils.StringRequest("user::::pass::::uhash::::target", Console.user + "::::" + Console.pass + "::::" + Console.uHash + "::::" + IP, "vh_spywareUpload.php");
        //    return new SpywareUploadResult(returnString);

        //}

        public async Task<TransferResult> trojanTransfer()
        {

            var json = await vhUtils.JSONRequest("user::::pass::::uhash::::target", config.username + "::::" + config.password + "::::" + vhConsole.uHash + "::::" + IP, "vh_trTransfer.php");
            return new TransferResult(json, IP);
        }

        public async void clearLogs()
        {

            await vhUtils.JSONRequest("user::::pass::::uhash::::target", config.username + "::::" + config.password + "::::" + vhConsole.uHash + "::::" + IP, "vh_clearAccessLog.php");
        }

        public String getUsername()
        {
            if (!success) return null;
            return username;
        }


        public int? getFirewallLevel()
        {
            if (!success) return null;
            try
            {
                return int.Parse(firewallLevel);
            }
            catch (FormatException e)
            {
                return null;
            }
        }


        public int? getAntiVirusLevel()
        {
            if (!success) return null;
            try
            {
                return int.Parse(antiVirusLevel);
            }
            catch (FormatException e)
            {
                return null;
            }
        }


        public int? getScanLevel()
        {
            if (!success) return null;
            try
            {
                return int.Parse(scanLevel);
            }
            catch (FormatException e)
            {
                return null;
            }
        }


        public int? getSdkLevel()
        {
            if (!success) return null;
            try
            {
                return int.Parse(sdkLevel);
            }
            catch (FormatException e)
            {
                return null;
            }
        }


        public int? getSpamLevel()
        {
            if (!success) return null;
            try
            {
                return int.Parse(spamLevel);
            }
            catch (FormatException e)
            {
                return null;
            }
        }


        public int? getMoney()
        {
            if (!success) return null;
            try
            {
                return int.Parse(money);
            }
            catch (FormatException e)
            {
                return null;
            }
        }


        public bool? isAnonymous()
        {
            if (!success) return null;
            return "YES".Equals(anonymous);
        }


        public int? getSuccessRep()
        {
            if (!success) return null;
            try
            {
                return int.Parse(successRep);
            }
            catch (FormatException er)
            {
                return null;
            }
        }


        public int? getFailRep()
        {
            if (!success) return null;
            try
            {
                return int.Parse(failRep);
            }
            catch (FormatException er)
            {
                return null;
            }
        }


        public int? getSuccessRate()
        {
            if (!success) return 0;
            try
            {
                return int.Parse(successRate.Replace("%", ""));
            }
            catch (FormatException er)
            {
                return null;
            }
        }

        public String getIP()
        {
            return IP;
        }

        public bool getSuccess()
        {

            return success;

        }
        /*

        private val success = result[1] != null
        val username = result[1]?.Substring(26)
        val firewallLevel = result[2]!!.Substring(26).toInt()
        val antiVirusLevel = result[3]!!.Substring(27).toInt()
        val scanLevel = result[4]!!.Substring(22).toInt()
        val sdkLevel = result[5]!!.Substring(21).toInt();
        val spamLevel = result[6]!!.Substring(22).toInt();
        val money = result[7]!!.Substring(23).toInt();
        val anonymous = result[9]!!.Substring(27) == "YES";
        val successRep = result[11]!!.Substring(32).toInt();
        val failRep = result[12]!!.Substring(29).toInt();
        val successRate = result[13]!!.Substring(39).replace("%","").toInt();
         */
    }
}
