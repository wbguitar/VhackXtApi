using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using DiffLib;
using System.IO;
using Newtonsoft.Json.Linq;
using Tesseract;
using DiffLib;

namespace VhackXtApi
{
    public class OCR
    {
        private int[][] nrlist = new int[][]
        {
            new int [] {13, 14, 15, 8, 6, 7, 15, 15, 12},
            new int [] {4, 4, 15, 15, 15},
            new int [] {9, 12, 13, 11, 14, 14, 13, 9},
            new int [] {7, 9, 9, 10, 10, 15, 15, 14},
            new int [] {5, 8, 8, 11, 10, 12, 15, 15},
            new int [] {12, 12, 11, 9, 9, 13, 13, 12},
            new int [] {13, 15, 10, 9, 9, 12, 14, 11},
            new int [] {4, 11, 13, 15, 9, 6, 4},
            new int [] {14, 15, 12, 9, 10, 15, 15, 14},
            new int [] {10, 13, 14, 10, 11, 15, 15, 14}
        };
        private TesseractEngine engine;

        public OCR(TesseractEngine engine)
        {
            this.engine = engine;
        }

        int[] analize(Image im)
        {
            var res = new List<int>();
            var bmp = new Bitmap(im);
            for (int i = 0; i < bmp.Width; i++)
            {
                var count = 0;
                for (int j = 0; j < bmp.Height; j++)
                {
                    var pix = bmp.GetPixel(i, j);
                    var rgb = string.Format("0x{0}{0}{0}", pix.B.ToString("X2"), pix.G.ToString("X2"), pix.R.ToString("X2"));
                    if (Convert.ToInt32(rgb, 16) != 0)
                        count++;
                }

                res.Add(count);
            }

            return res.ToArray();
        }

        int[][] splitnumbers(int[] list)
        {
            int i = 0;
            var l = list.ToList();
            var nrs = new List<int[]>();
            while (i < l.Count)
            {
                if (l[i] > 3)
                {
                    var temp = new List<int>();
                    var add = 0;

                    for (int j = 0; j < 9; j++)
                    {
                        if (l[i + j] >= 4)
                            temp.Add(l[i + j]);
                        else
                        {
                            add = j;
                            break;
                        }
                    }

                    if (add == 0)
                        add = 8;

                    i += add;
                    nrs.Add(temp.ToArray());
                }

                i++;
            }

            return nrs.ToArray();
        }

        int[] readit(int[][] list)
        {
            var nrs = new List<int>();
            foreach (var it in list)
            {
                var ratio = double.MaxValue;
                var cur = 0;
                for (int j = 0; j < nrlist.Length; j++)
                {
                    //var matcher = new DiffLib.PatienceSequenceMatcher<int>();
                    //var diff = matcher.CreateDifferencer();
                    //var diffs = diff.FindDifferences(it, nrlist[j]);

                    //var finder = matcher.FindMatchingBlocks(it, nrlist[j]);
                    //var trat = finder.Count();
                    //if (trat > ratio)
                    //{
                    //    ratio = trat;
                    //    cur = j;
                    //}



                    //var temp = similitudeRatio(it, nrlist[j]);
                    //if (double.IsInfinity(temp))
                    //{
                    //    cur = j;
                    //    break;
                    //}
                    //else if (temp > ratio)
                    //{
                    //    ratio = temp;
                    //    cur = j;
                    //}

                    var temp = calc(it, nrlist[j]);
                    if (temp == 0)
                    {
                        cur = j;
                        break;
                    }
                    else if (temp < ratio)
                    {
                        ratio = temp;
                        cur = j;
                    }
                }

                nrs.Add(cur);
            }

            return nrs.ToArray();
        }


        double calc(int[] arr1, int []arr2)
        {
            //var arr1 = i1.ToString().ToCharArray().Select(it => (int)it).ToList();
            //var arr2 = i2.ToString().ToCharArray().Select(it => (int)it).ToList();
            var sections = Diff.CalculateSections<int>(arr1, arr2);

            var matches = sections.Where(s => s.IsMatch);
            var unmatches = sections.Where(s => !s.IsMatch);
            var a = (double)matches.Select(s => s.LengthInCollection1).Sum() * matches.Count();
            var b = (double)unmatches.Select(s => s.LengthInCollection1).Sum() * unmatches.Count();
            //	Console.WriteLine(a);
            //	Console.WriteLine(b);
            //	Console.WriteLine(b/a);
            return b / a;
        }

        double similitudeRatio(int[] arr1, int[] arr2)
        {

            var sum = 0.0;
            for (int i = 0; i < Math.Min(arr1.Length, arr2.Length); i++)
                sum += Math.Pow(arr1[i] - arr2[i], 2);

            // divide by a factor proportional to lenghts difference
            var div = Math.Abs((double)(arr1.Length - arr2.Length) / 10.0) + 1;

            return div / Math.Sqrt(sum);
        }

        string rightSolution(int[] nr, int[] poss)
        {
            var number = Convert.ToInt32(nr.Aggregate("", (s, i) => s += i.ToString()));

            return "p" + (poss.ToList().IndexOf(number) + 1);

            //var ratio = 0.0;
            //var cur = 0;

            //for (int i = 0; i < poss.Length; i++)
            //{
            //    var matcher = new DiffLib.PatienceSequenceMatcher<int>();
            //    var finder = matcher.FindMatchingBlocks(nr, poss[i]);
            //    var trat = finder.Count();
            //    if (trat > ratio)
            //    {
            //        ratio = trat;
            //        cur = i;
            //    }
            //}


//            return "p" + (cur + 1);

        }


        public string getSolution(string response)
        {
            return getSolution(JObject.Parse(response));
        }

        public string getSolution(JObject jo)
        {
            
            string str = "";
            try
            {
                //str = response.Split(',')[0]
                //.Split(new string[] { @"{""img"":" }, StringSplitOptions.None)[1]
                //.Split('"')[0];

                str = (string)jo.GetValue("img");

            }
            catch (Exception)
            {
                return "";
            }

            Image image = null;
            var bytes = Convert.FromBase64String(str);
            using (var ms = new MemoryStream(bytes))
            {
                image = System.Drawing.Image.FromStream(ms);
            }

            var filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), Path.GetRandomFileName() + ".png");
            //image.Save(filename, System.Drawing.Imaging.ImageFormat.Png);

            var possies = new int[7];
            for (int i = 1; i < 7; i++)
            {
                //possies[i -1] = Convert.ToInt32(response.Split(',')[i].Split(':')[1]);
                possies[i - 1] = (int)jo.GetValue("p" + i);
            }

            //int sol = 0;
            //using (var page = engine.Process(new Bitmap(image)))
            //{
            //    sol = Convert.ToInt32(page.GetText().Trim());
            //}

            //var retval = "p" + (possies.ToList().IndexOf(sol) + 1);
            //return retval;

            var anal = analize(image);
            var l = splitnumbers(anal);
            var l1 = readit(l);
            return rightSolution(l1, possies);
        }
    }
}