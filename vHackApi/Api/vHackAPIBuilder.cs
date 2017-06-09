using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Threading.Tasks;

namespace vHackApi.Api
{
    public class vhAPIBuilder
    {

        IConfig cfg;
        protected String mail;

        WebProxy proxy;

        /**
             * Sets a proxy for the system
             * @param proxyUrl  The proxy's IP/URL
             * @param proxyPort The proxy's port
             */
        public vhAPIBuilder useProxy(String proxyUrl, int proxyPort)
        {

            //System.setProperty("http.proxyHost", proxyUrl);
            //System.setProperty("http.proxyPort", String.valueOf(proxyPort));

            proxy = new WebProxy(proxyUrl, proxyPort);
            return this;

        }

        /**
         * Sets a proxy that requires auth for the system
         * @param proxyUrl  The proxy's IP/URL
         * @param proxyPort The proxy's port
         * @param username  The proxy's username
         * @param password  The proxy's password
         */
        public vhAPIBuilder useProxy(String proxyUrl, int proxyPort, String username, String password)
        {

            //System.setProperty("http.proxyHost", proxyUrl);
            //System.setProperty("http.proxyPort", String.valueOf(proxyPort));
            //System.setProperty("http.proxyUser", username);
            //System.setProperty("http.proxyPassword", password);

            proxy = new WebProxy(proxyUrl, proxyPort);
            proxy.Credentials = new NetworkCredential(username, password);

            return this;
        }

        public vhAPIBuilder email(String em)
        {
            this.mail = em;
            return this;
        }

        public vhAPIBuilder config(IConfig cfg)
        {
            this.cfg = cfg;
            return this;
        }

        public async Task<vhAPIBuilder> register()
        {

            JObject json = await vhUtils.JSONRequest("user::::pass::::email", cfg.username + "::::" + cfg.password + "::::" + mail, "vh_register.php");

            if ((string)json.GetValue("result") != "0")
            {
                return null;
            }
            else
            {
                return this;
            }
        }



        public vhAPI getAPI()
        {
            //try {
            //       SSLContext sc = SSLContext.getInstance("SSL");
            //       sc.init(null, trustAllCerts, new java.security.SecureRandom());
            //       HttpsURLConnection.setDefaultSSLSocketFactory(sc.getSocketFactory());
            //   } catch (GeneralSecurityException e) {
            //   }

            vhAPI api = new vhAPI(cfg);
            return api;
        }

        //public static TrustManager[] trustAllCerts = new TrustManager[] {
        //          new X509TrustManager() {
        //              public java.security.cert.X509Certificate[] getAcceptedIssuers() {
        //                  return new X509Certificate[0];
        //              }
        //              public void checkClientTrusted(
        //                  java.security.cert.X509Certificate[] certs, String authType) {
        //                  }
        //              public void checkServerTrusted(
        //                  java.security.cert.X509Certificate[] certs, String authType) {
        //              }
        //          }
        //      };


    }
}
