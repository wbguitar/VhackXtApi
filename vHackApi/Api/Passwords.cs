using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vHackApi.Api
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
}
