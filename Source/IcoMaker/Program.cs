using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace IcoMaker
{
    using System.Drawing;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            var ico = new Icon();
            string output = "default.ico";
            foreach (var arg in args)
            {
                if (arg.Contains(".ico")) output = arg;
                if (arg.Contains(".png")) ico.Add(arg);
            }

            Console.WriteLine("Exporting to " + output);
            ico.Save(output);
        }
    }

    public class Icon
    {
        public IList<Image> Images { get; private set; }

        public Icon()
        {
            Images = new List<Image>();
        }

        public void Save(string path)
        {
            // http://en.wikipedia.org/wiki/ICO_(file_format)
            using (var s = File.OpenWrite(path))
            {
                using (var w = new BinaryWriter(s))
                {
                    w.Write((short)0);
                    w.Write((short)1);
                    w.Write((short)Images.Count);
                    int offset = 6 + Images.Count * 16;
                    var buffers = new List<byte[]>();
                    foreach (var i in Images)
                    {
                        w.Write((byte)i.Width);
                        w.Write((byte)i.Height);
                        w.Write((byte)0); // number of colors in palette
                        w.Write((byte)0); // reserved
                        w.Write((short)0); // color planes
                        w.Write((short)0); // bits per pixel
                        var bytes = this.BitmapToByteArray(i);
                        buffers.Add(bytes);
                        var size = bytes.Length;
                        w.Write((uint)size); // bits per pixel
                        w.Write((uint)offset); // bits per pixel
                        offset += size;
                    }
                    foreach (var b in buffers)
                    {
                        w.Write(b);
                    }
                }
            }

        }

        byte[] BitmapToByteArray(Image bmp)
        {
            using (var ms = new MemoryStream())
            {
                bmp.Save(ms, ImageFormat.Png);
                return ms.GetBuffer();
            }
        }

        public void Add(string filename)
        {
            var img = Image.FromFile(filename);
            Images.Add(img);
        }
    }
}
