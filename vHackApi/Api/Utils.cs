using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using vHackApi.Console;
using vHackApi.Interfaces;

namespace vHackApi.Api
{
    public static class vhUtils
    {
        public static IConfig config { get; set; }

        public static string jsonTextC;
        /**
         * The url of the current api.<br>
         * As of now it is {@value url}.
         */
        private static readonly string rootUrl = "https://api.vhack.cc/v/9/";
        /**
         * The hashing algorithm that is used to hash data in requests.<br>
         * It now is {@value md5s}.
         */
        private static readonly string md5s = "MD5";
        /**
         * A secret salt that is used with hashing<br>
         * It now is {@value secret}.
         */
        private static readonly string secret = "aeffI";
        /**
         * Unknown
         */
        static readonly bool assertionstatus;
        /**
         * Unknown - maybe the charset?
         */
        private static readonly byte[] byt;

        public static bool debug = false;

        static vhUtils()
        {
            assertionstatus = true; //!Utils.class.desiredAssertionStatus();
            byt = new byte[]{(byte) 65, (byte) 66, (byte) 67,
                         (byte) 68, (byte) 69, (byte) 70,
                         (byte) 71, (byte) 72, (byte) 73,
                         (byte) 74, (byte) 75, (byte) 76,
                         (byte) 77, (byte) 78, (byte) 79,
                         (byte) 80, (byte) 81, (byte) 82,
                         (byte) 83, (byte) 84, (byte) 85,
                         (byte) 86, (byte) 87, (byte) 88,
                         (byte) 89, (byte) 90, (byte) 97,
                         (byte) 98, (byte) 99, (byte) 100,
                         (byte) 101, (byte) 102, (byte) 103,
                         (byte) 104, (byte) 105, (byte) 106,
                         (byte) 107, (byte) 108, (byte) 109,
                         (byte) 110, (byte) 111, (byte) 112,
                         (byte) 113, (byte) 114, (byte) 115,
                         (byte) 116, (byte) 117, (byte) 118, 
                         (byte) 119, (byte) 120, (byte) 121,
                         (byte) 122, (byte) 48, (byte) 49,
                         (byte) 50, (byte) 51, (byte) 52,
                         (byte) 53, (byte) 54, (byte) 55,
                         (byte) 56, (byte) 57, (byte) 45,
                         (byte) 95};
        }

        /**
         * Reads all data from a buffered reader and returns it as a string.
         * @param rd The buffered Reader which holds the data.
         * @return The string representation of data the buffered reader contains.
         * @throws IOException  If an I/O error occurs
         */
        public static string readJson(System.IO.StreamReader rd)
        {
            return rd.ReadToEnd();
        }

        static DateTime lastReq = DateTime.Now;

        public class RequestException: Exception
        {
            public RequestException(string msg) : base(msg) { }
        }
        /**
         * Makes a request to the api and returns the result as a JObject Object.
         * Makes a requests to the vHack Api, with the params format, data data and to the file php and returns the result, which is json, as a JObject Object.<br>
         * Errors are thrown if user/password is wrong and (possibly) if the api url changed.<br>
         * It is similar to {@link Utils#StringRequest(string, string, string)} but differs from it in that does processing with the obtained data.<br>
         * it returns the result as json Object and performs checks for any (known) errors.
         * @param format Lists the params that will be passed to the api endpoint. The names are separated with "::::".<br>
         *               Every request, except the very first one, should include "user::::pass::::uhash".<br>
         *               Example: "user::::pass::::uhash::::global" (taken from Console.getIP)
         * @param data The data for the params that you passed in. They are also separated by "::::". You can just concatanate the parts of this.<br>
         *             Example: "vHackAPI::::123456::::aaaabbbbccccddddeeeeffffgggghhhhiiiijjjjkkkkllllmmmmnnnnoooopppp::::1"
         * @param php This is the api endpoint that the request will be sent to. In the case of the vHackAPI it are php documents.<br>
         *            Example "vh_network.php"
         * @return The resulte Json as a JObject. Errors are thrown if user/password is wrong and (possibly) if the api url changed. null is returned if there are other errors.
         * @throws ExecutionException 
         * @throws InterruptedException 
         */
        public static async Task<JObject> JSONRequest(string format, string data, string php, int attempts = -1)
        {
            string jsonText = "";
            try
            {

                jsonText = await Request(format, data, php, attempts);

                if (string.IsNullOrEmpty(jsonText))
                {
                    throw new Exception("Old API URL");
                }
                else if (jsonText == "8")
                {
                    throw new RequestException("Wrong Password/User");
                }
                else if (jsonText.Length == 1)
                    return null;

                return JObject.Parse(jsonText);
            }
            catch (RequestException e)
            {
                Debug.Print(e.StackTrace);

            }

            //var jreqTask = JSONRequest(format, data, php);
            var jres = await JSONRequest(format, data, php);
            return jres;
        }

        //it'll just do the request without any checks
        /**
         * Makes a request to the api and returns the result as a string.
         * Makes a requests to the vHack Api, with the params format, data data and to the file php and returns the result, which is json, as a string Object.<br>
         * It is similar to {@link Utils#JSONRequest(string, string, string)} but differs from it in the form that it returns and string and doesn't perform checks.
         * @param format Lists the params that will be passed to the api endpoint. The names are separated with "::::".<br>
         *               Every request, except the very first one, should include "user::::pass::::uhash".<br>
         *               Example: "user::::pass::::uhash::::global" (taken from Console.getIP)
         * @param data The data for the params that you passed in. They are also separated by "::::". You can just concatanate the parts of this.<br>
         *             Example: "vHackAPI::::123456::::aaaabbbbccccddddeeeeffffgggghhhhiiiijjjjkkkkllllmmmmnnnnoooopppp::::1"
         * @param php This is the api endpoint that the request will be sent to. In the case of the vHackAPI it are php documents.<br>
         *            Example "vh_network.php"
         * @return The resulte Json as a Future<string>.
         */
        //JDOC needs rewriting
        public static async Task<string> Request(string format, string data, string php, int attempts)
        {
            try
            {
                var elapsed = DateTime.Now - lastReq;
                if (elapsed.TotalMilliseconds < vhConsole.WaitStep)
                    Thread.Sleep(vhConsole.WaitStep - (int)elapsed.TotalMilliseconds);

                var url = vhUtils.generateURL(format, data, php);
                var req = WebRequest.CreateHttp(url);

                var proxy = config?.proxy;
                if (proxy != null)
                    req.Proxy = proxy;

                var res = req.GetResponse();
                var resTxt = new StreamReader(res.GetResponseStream()).ReadToEnd();

                lastReq = DateTime.Now;

                return resTxt;
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                //Thread.Sleep(vhConsole.WaitStep);
            }

            if (attempts == 0)
                return "0";

            if (attempts != -1)
                attempts--;

            var ret = await Request(format, data, php, attempts);
            return ret;
        }

        public static async Task<string> StringRequest(string format, string data, string php, int attempts = -1)
        {
            return await Request(format, data, php, attempts);
        }

        private static byte[] m9179a(byte[] arrby, int n2, int n3, byte[] arrby2, int n4, byte[] arrby3)
        {
            int n5 = n3 > 0 ? arrby[n2] << 24 >> 8 : 0;
            int n6 = n3 > 1 ? arrby[n2 + 1] << 24 >> 16 : 0;
            int n7 = n6 | n5;
            int n8 = 0;
            if (n3 > 2)
            {
                n8 = arrby[n2 + 2] << 24 >> 24;
            }
            int n9 = n8 | n7;
            switch (n3)
            {
                default:
                    {
                        return arrby2;
                    }
                case 3:
                    {
                        arrby2[n4] = arrby3[n9 >> 18];
                        arrby2[n4 + 1] = arrby3[63 & n9 >> 12];
                        arrby2[n4 + 2] = arrby3[63 & n9 >> 6];
                        arrby2[n4 + 3] = arrby3[n9 & 63];
                        return arrby2;
                    }
                case 2:
                    {
                        arrby2[n4] = arrby3[n9 >> 18];
                        arrby2[n4 + 1] = arrby3[63 & n9 >> 12];
                        arrby2[n4 + 2] = arrby3[63 & n9 >> 6];
                        arrby2[n4 + 3] = 61;
                        return arrby2;
                    }
                case 1:
                    break;
            }
            arrby2[n4] = arrby3[n9 >> 18];
            arrby2[n4 + 1] = arrby3[63 & n9 >> 12];
            arrby2[n4 + 2] = 61;
            arrby2[n4 + 3] = 61;
            return arrby2;
        }

        private static string generateUser_old(byte[] bArr, int i, int i2, byte[] bArr2, bool z)
        {
            return Convert.ToBase64String(bArr).Replace("=", "");

            //byte[] a = assertion(bArr, i, i2, bArr2, int.MaxValue);
            //int length = a.Length;
            //while (!z && length > 0 && a[length - 1] == 61)
            //{
            //    length--;
            //}
            ////return new string(a, 0, length);
            //return Encoding.UTF8.GetString(a.Take(length).ToArray());
        }

        private static string generateUser(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes).Replace("=", "");
        }

        /**
         * Hashes the given string with {@value md5s}.
         * The hashing alorithm is determined by {@link Utils#md5s}
         * @param str The string that should be hashed with {@value md5s}.
         * @return The parameter str hashed using {@value md5s}.
         */
        private static string hashString(string str)
        {
            try
            {
                var md5 = MD5CryptoServiceProvider.Create();
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(str));
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    //string toHexString = (b & 255).ToString("x2");
                    string toHexString = b.ToString("x2");
                    while (toHexString.Length < 2)
                    {
                        toHexString = "0" + toHexString;
                    }
                    sb.Append(toHexString);
                }

                return sb.ToString();


                //MessageDigest instance = MessageDigest.getInstance(md5s);
                //instance.update();
                //byte[] digest = instance.digest();
                //for (byte b in digest) {
                //    string toHexString = Integer.toHexString(b & 255);
                //    while (toHexString.length() < 2) {
                //        toHexString = "0" + toHexString;
                //    }
                //    stringBuilder.append(toHexString);
                //}
                //return stringBuilder.toString();
            }
            catch (Exception e)
            {
                Debug.Print(e.StackTrace);
                return "";
            }
        }

        private static byte[] assertion(byte[] bArr, int i, int i2, byte[] bArr2, int i3)
        {
            int i4 = ((i2 + 2) / 3) * 4;
            byte[] bArr3 = new byte[(i4 + (i4 / i3))];
            int i5 = i2 - 2;
            int i6 = 0;
            int i7 = 0;
            int i8 = 0;
            while (i8 < i5)
            {
                i4 = (((bArr[i8 + i] << 24) >> 8) | ((bArr[(i8 + 1) + i] << 24) >> 16)) | ((bArr[(i8 + 2) + i] << 24) >> 24);
                bArr3[i7] = bArr2[i4 >> 18];
                bArr3[i7 + 1] = bArr2[(i4 >> 12) & 63];
                bArr3[i7 + 2] = bArr2[(i4 >> 6) & 63];
                bArr3[i7 + 3] = bArr2[i4 & 63];
                i4 = i6 + 4;
                if (i4 == i3)
                {
                    bArr3[i7 + 4] = (byte)10;
                    i7++;
                    i4 = 0;
                }
                i8 += 3;
                i7 += 4;
                i6 = i4;
            }
            if (i8 < i2)
            {
                m9179a(bArr, i8 + i, i2 - i8, bArr3, i7, bArr2);
                if (i6 + 4 == i3)
                {
                    bArr3[i7 + 4] = (byte)10;
                    i7++;
                }
                i7 += 4;
            }
            if (assertionstatus || i4 == bArr3.Length)
            {
                return bArr3;
            }
            throw new Exception();
        }

        /**
         * Generates a url to where a request has to be made.
         * Generates the complete url a request has to be done to, to achieve a certain action (E.g. upgrade a Botnet Computer).<br>
         * Needed for this are the username, the password, the uHash and any additional parameters. The time is also neede but you dont need to supply it because the programm get the time by it itself.<br>
         * It is used by {@link Utils#JSONRequest(string, string, string)} and {@link Utils#StringRequest(string, string, string)}.
         * @param format Lists the params that will be passed to the api endpoint. The names are separated with "::::".<br>
         *               Every request, except the very first one, should include "user::::pass::::uhash".<br>
         *               Example: "user::::pass::::uhash::::global" (taken from Console.getIP)
         * @param data The data for the params that you passed in. They are also separated by "::::". You can just concatanate the parts of this.<br>
         *             Example: "vHackAPI::::123456::::aaaabbbbccccddddeeeeffffgggghhhhiiiijjjjkkkkllllmmmmnnnnoooopppp::::1"
         * @param php This is the api endpoint that the request will be sent to. In the case of the vHackAPI it are php documents.<br>
         *            Example "vh_network.php"
         * @return The url Url a request has to be directed to.
         */
        public static string generateURL_old(string format, string data, string php)
        {
            var split = format.Split(new[] { "::::" }, StringSplitOptions.None);
            var split2 = data.Split(new[] { "::::" }, StringSplitOptions.None);
            long currentTimeMillis = (long)(DateTime.Now - new DateTime(1970, 01, 01)).TotalMilliseconds;// (long)(((double)DateTime.Now.Ticks / (double)TimeSpan.TicksPerMillisecond) / 1000.0);
            JObject jSONObject = new JObject();
            jSONObject.Add("", "");
            for (int i = 0; i < split.Length; i++)
            {
                try
                {
                    //if (split2[i] == "")
                    //    split2[i] = "null";
                    jSONObject.Add(split[i].ToString(), split2[i].ToString());
                }
                catch (JsonException e)
                {
                    Debug.Print(e.StackTrace);
                }
            }
            try
            {
                jSONObject.Add("time", currentTimeMillis + "");
            }
            catch (JsonException e2)
            {
                Debug.Print(e2.StackTrace);
            }
            string jsonString = jSONObject.ToString();
            //jsonString = jsonString
            //    .Replace(System.Environment.NewLine, "")
            //    .Replace(" ", "");

            byte[] jsonStringBytes = Encoding.UTF8.GetBytes(jsonString);
            string a = generateUser_old(jsonStringBytes, 0, jsonStringBytes.Length, byt, false);

            string a2 = hashString(jsonString.Length + hashString(currentTimeMillis + ""));
            string str5 = split2[0] + "" + hashString(hashString(split2[1]));
            string str6 = hashString(currentTimeMillis + "" + jsonString);
            byte[] bytes2 = Encoding.UTF8.GetBytes(a2);
            byte[] bytes3 = Encoding.UTF8.GetBytes(str5);
            byte[] bytes4 = Encoding.UTF8.GetBytes(str6);
            string a3 = hashString(secret + hashString(hashString(generateUser_old(bytes2, 0, bytes2.Length, byt, false))));
            string str9 = generateUser_old(bytes3, 0, bytes3.Length, byt, false);
            string str7 = generateUser_old(bytes4, 0, bytes4.Length, byt, false);
            string str8 = hashString(hashString(a3 + hashString(hashString(str9) + str7)));
            string retStr = rootUrl + php + "?user=" + a + "&pass=" + str8;

            Debug.Print(retStr);

            return retStr;
        }

        public static string generateURL(string format, string data, string php)
        {
            var split = format.Split(new[] { "::::" }, StringSplitOptions.None);
            var split2 = data.Split(new[] { "::::" }, StringSplitOptions.None);
            long currentTimeMillis = (long)(DateTime.Now - new DateTime(1970, 01, 01)).TotalMilliseconds;
            JObject jSONObject = new JObject();
            //jSONObject.Add("", "");
            for (int i = 0; i < split.Length; i++)
            {
                try
                {
                    //if (split2[i] == "")
                    //    split2[i] = "null";
                    jSONObject.Add(split[i].ToString(), split2[i].ToString());
                }
                catch (JsonException e)
                {
                    Debug.Print(e.StackTrace);
                }
            }
            try
            {
                jSONObject.Add("time", currentTimeMillis + "");
            }
            catch (JsonException e2)
            {
                Debug.Print(e2.StackTrace);
            }
            string jsonString = jSONObject.ToString();
            //jsonString = jsonString
            //    .Replace(System.Environment.NewLine, "")
            //    .Replace(" ", "");

            byte[] jsonStringBytes = Encoding.UTF8.GetBytes(jsonString);
            string a = generateUser(jsonString);

            string a2 = hashString(jsonString.Length + hashString(currentTimeMillis.ToString()));
            string str5 = split2[0] + hashString(hashString(split2[1]));
            string str6 = hashString(currentTimeMillis + jsonString);
            string a3 = hashString(secret + hashString(hashString(generateUser(a2))));
            string str9 = generateUser(str5);
            string str7 = generateUser(str6);
            string str8 = hashString(hashString(a3 + hashString(hashString(str9) + str7)));
            string retStr = rootUrl + php + "?user=" + a + "&pass=" + str8;

            Debug.Print(retStr);

            return retStr;
        }

        public static bool IsContestRunning()
        {
            var tod = DateTime.UtcNow;
            var res = (DateTime.Parse("7:00") <= tod && tod <= DateTime.Parse("9:00")) ||
                (DateTime.Parse("19:00") <= tod && tod <= DateTime.Parse("21:00"));
            return res;
        }

    }
}