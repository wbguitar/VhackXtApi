using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace VhackXtApi.Console
{
    public class PasswordImageHelper
    {

        string secret;
        string patternSecret;
        PasswordImage[] passwordImages = new PasswordImage[4];

        public PasswordImageHelper(string vulnScanResultString)
            : this(new JObject(vulnScanResultString))
        {

        }

        public PasswordImageHelper(JObject result)
        {
            secret = (string)result.GetValue("secret");
            patternSecret = secret.Replace("*", ".{1}");

            passwordImages[0] = new PasswordImage((string)result.GetValue("img_0"));
            passwordImages[1] = new PasswordImage((string)result.GetValue("img_1"));
            passwordImages[2] = new PasswordImage((string)result.GetValue("img_2"));
            passwordImages[3] = new PasswordImage((string)result.GetValue("img_3"));
        }

        public PasswordImage[] getPasswordImages()
        {
            return passwordImages;
        }

        public string getSecret()
        {
            return secret;
        }

        public int getIDOfRightImage()
        {
            foreach (var passwordImage in passwordImages)
            {
                var m = Regex.Match(passwordImage.getOCRString(), patternSecret);
                if (m.Success)
                {
                    return passwordImages.ToList().IndexOf(passwordImage);
                }
            }
            //for (PasswordImage passwordImage : passwordImages)
            //{
            //    Matcher matcher = patternSecret.matcher(passwordImage.getOCRString());
            //    if(matcher.find())
            //    {
            //        return Arrays.asList(passwordImages).indexOf(passwordImage);
            //    }
            //}
            throw new Exception("no matches");
        }
    }

}
