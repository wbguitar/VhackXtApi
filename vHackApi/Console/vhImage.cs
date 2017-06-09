using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;

namespace vHackApi.Console
{
    public abstract class vhImg
    {

        public Image image { get; protected set; }

        public vhImg(string base64String)
        {
            var bytes = Convert.FromBase64String(base64String);
            using (var ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
        }

        public void Save(string filePath)
        {
            image.Save(filePath);
        }

        public vhImg(System.Drawing.Image _image)
        {
            this.image = _image;
            parseImageData();
        }

        protected abstract void parseImageData();

        protected Int64 generateHashFor(System.Drawing.Image image)
        {
            return generateHashFor(image, 16711680);
        }

        protected Int64 generateHashFor(System.Drawing.Image image, int imageColorToSearchFor)
        {
            Int64 two = 2; // BigInteger.valueOf(2);

            Int64 hash = 1;// BigInteger.ONE;
            var bmp = new System.Drawing.Bitmap(image);
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var rgb = bmp.GetPixel(x, y).ToArgb();
                    hash = hash * two + (rgb == imageColorToSearchFor ? 0 : 1);
                    //hash = hash.multiply(two).add(BigInteger.valueOf((image.getRGB(x, y) == imageColorToSearchFor ? 0 : 1)));
                }
            }
            return hash;
        }

        protected String base64RepresentationOf(System.Drawing.Image image)
        {
            try
            {
                var ms = new MemoryStream();
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                return Encoding.UTF8.GetString(ms.GetBuffer());
                //ImageIO.write(image, "png", byteArrayOutputStream);
                //return DatatypeConverter.printBase64Binary(byteArrayOutputStream.toByteArray());
            }
            catch (IOException e)
            {
                Debug.Print(e.StackTrace);
                throw new Exception("Couldn't convert the BufferedImage to a base64 String");
            }
        }


        public int GetRGB(int x, int y)
        {
            var pix = new Bitmap(image).GetPixel(x, y);
            return pix.ToArgb();
        }

        public Bitmap GetSubImage(int x, int y, int w, int h)
        {
            return new Bitmap(image).Clone(new Rectangle(x, y, w, h), image.PixelFormat);
        }
    }
}
