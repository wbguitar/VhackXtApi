using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhackXtApi.Console
{
    public class TransferResult
    {

        protected bool success;
        protected int moneyamount;
        protected int repgained;
        protected int replost;
        protected String ip;

        public TransferResult(JObject result, String ip)
        {
            this.ip = ip;
            success = result.GetValue("result").Contains("0");
            if (!success) return;

            try
            {
                moneyamount = (int)result.GetValue("amount");
            }
            catch (JsonException e)
            {
                Debug.Print(result.ToString());
                Debug.Print(e.StackTrace);
            }
            var resVal = (int)result.GetValue("eloch");
            if (resVal < 0)
            {
                replost = resVal;
                repgained = 0;
            }
            else
            {
                replost = 0;
                repgained = resVal;
            }
        }

        public bool getSuccess()
        {
            return success;
        }

        public String getTarget()
        {
            return ip;
        }

        public int getMoneyAmount()
        {
            return moneyamount;
        }

        public int getRepGained()
        {
            return repgained;
        }

        public int getRepLost()
        {
            return replost;
        }
    }

}