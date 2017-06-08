using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VhackXtApi.Console
{
    public class PasswordImage : Img
    {

        private String text;

        public PasswordImage(String base64String) : base(base64String) { }

        public PasswordImage(Image image) : base(image) { }


        override protected void parseImageData()
        {
            readInPassword();
        }

        private void readInPassword()
        {
            text = "";

            Letters letters = Letters.Instance;

            bool pixelsInTenCharSpace = arePixelsInTenCharSpace();

            int offset = (pixelsInTenCharSpace ? 0 : 4);
            int numberOfCharsDelimiter = (pixelsInTenCharSpace ? 11 : 10);

            for (int i = 1; i < numberOfCharsDelimiter; i++)
            {
                var subImage = GetSubImage((9 * i) + offset, 15, 8, 12);

                if (letters.getCharFor(generateHashFor(subImage)) == ' ')
                {
                    Debug.Print(base64RepresentationOf(subImage));
                    throw new Exception("One of the characters is unkown at the moment. You may send us the preceding string so that we can add it to the lookup table.");
                }
                else
                {
                    text += letters.getCharFor(generateHashFor(subImage));
                }
            }
        }

        public String getOCRString()
        {
            return text;
        }

        private bool arePixelsInTenCharSpace()
        {
            for (int y = 15; y < 27; y++)
            {
                for (int x = 9; x < 12; x++)
                {
                    if (this.GetRGB(x, y) != 16711680)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

}
