using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhackXtApi
{
    public class Passwords
    {
        public string Img1 { get; private set; }
        public Passwords(string[] arr)
        {
            if (arr.Length == 5)
                Img1 = arr[0].Split(':')[1];
            else
                Img1 = null;
        }
    }

    public class IPAddress
    {
        public string IP { get; private set; }
        public string Attacked { get; private set; }
        public string FirewallLevel { get; private set; }

        public IPAddress(string[] arr)
        {
            IP = arr[0].Split(':')[1];
            FirewallLevel = arr[1].Split(':')[1];
            Attacked = arr[2].Split(':')[1];
        }
    }

    public interface ILogger
    {
        void Log(string format, params object[] parms);
    }

    

    public enum Tasks
    {
        spam,
        antivirus,

    }

    public interface IConfig
    {
        string username { get; }
        string password { get; }
        string tessdata { get; }
        int waitstep { get; }
        int winchance { get; }

        Tasks[] updates { get; }

        ILogger logger { get; }
    }
}
