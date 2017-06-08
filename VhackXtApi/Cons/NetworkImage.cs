using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Drawing;

namespace VhackXtApi.Console
{

    public class NetworkImage : Img
    {

        private string hostName = "";
        private bool anonymous;
        private int firewallLevel;

        public NetworkImage(string base64String) : base(base64String) { }

        public NetworkImage(Image image) : base(image) { }

        override protected void parseImageData()
        {
            readInHostname();
            parseLastLine();
        }

        private void parseLastLine()
        {
            anonymous = checkRedPixel();
            var letters = Letters.Instance;

            if (anonymous)
            {
                int start = findStartOfFirewallText() + 77 + 13;

                for (int i = 0; i < 10; i++)
                {
                    //var subImage = new Bitmap(image).Clone(new Rectangle(start + i * 9, 38, 8, 12), image.PixelFormat);
                    var subImage = GetSubImage(start + i * 9, 38, 8, 12);

                    if (letters.getCharFor(generateHashFor(subImage)) == ' ')
                    {
                        //we probably reached the end but it may also be an unknown char
                        break;
                    }
                    else
                    {
                        firewallLevel *= 10;
                        firewallLevel += (int)letters.getCharFor(generateHashFor(subImage));
                    }
                }
            }
        }


        private bool checkRedPixel()
        {
            //int oneAnonymityPixel = image.getRGB(50, 38);
            //int red = (oneAnonymityPixel & 0x00ff0000) >> 16;
            //int green = (oneAnonymityPixel & 0x0000ff00) >> 8;
            //int blue = oneAnonymityPixel & 0x000000ff;
            //return !(red == 136 && green == 0 && blue == 0);

            var pixel = new Bitmap(image).GetPixel(50, 38);
            return !(pixel.R == 136 && pixel.G == 0 && pixel.B == 0);
        }

        private void readInHostname()
        {
            Letters letters = Letters.Instance;

            for (int i = 0; i < 7; i++)
            {
                var subImage = new Bitmap(image).Clone(new Rectangle(9 * i + 72, 23, 8, 12), image.PixelFormat);
                //BufferedImage subImage = image.getSubimage(9 * i + 72, 23, 8, 12);

                if (letters.getCharFor(generateHashFor(subImage)) == ' ')
                {
                    throw new Exception("One of the characters is unkown at the moment. Plese send the base64 string to us so that we cam add it");
                }
                else
                {
                    hostName += letters.getCharFor(generateHashFor(subImage));
                }
            }
        }

        public bool checkForAnonymity()
        {
            return anonymous;
        }

        public string getHostName()
        {
            return "XT-" + hostName + ".vhack.cc";
        }

        public int getFirewallLevel()
        {
            return firewallLevel;
        }

        private int findStartOfFirewallText()
        {
            for (int x = 0; x < 263; x++)
            {
                if (GetRGB(x, 38) != 16711680)
                {
                    return x;
                }
            }

            throw new Exception("The image seems to be malformed");
        }
    }
}