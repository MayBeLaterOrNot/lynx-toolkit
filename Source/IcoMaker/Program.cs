using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace LynxToolkit
{
    using System.Drawing;
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Application.Header);

            var ico = new Icon();
            string iconFilename = null;
            foreach (var arg in args)
            {
                if (arg.Contains(".ico")) iconFilename = arg;
                if (arg.Contains(".png")) ico.Add(arg);
            }

            if (ico.Images.Count > 0)
            {
                if (iconFilename == null) iconFilename = "default.ico";
                Console.WriteLine("Exporting to " + iconFilename);
                ico.Save(iconFilename);
            }
            else if (iconFilename != null)
            {
                Console.WriteLine("Loading " + iconFilename);
                ico.Load(iconFilename);
                Console.WriteLine();
                Console.WriteLine("Image type: {0}", ico.ImageType);
                Console.WriteLine("Images: {0}", ico.Entries.Count);
                Console.WriteLine();
                foreach (var entry in ico.Entries)
                {
                    Console.WriteLine("{0}x{1} {2}c {4}cp {5}bpp {6}b {8}", entry.Width, entry.Height, entry.Colors, entry.Reserved, entry.ColorPlanes, entry.BitsPerPixel, entry.Size, entry.Offset, entry.Format);
                }
                Console.ReadKey();
            }
        }
    }

    public enum ImageType { Icon = 1, Cursor = 2 }

    public class Icon
    {
        public IList<Entry> Entries { get; private set; }
        public IList<Image> Images { get; private set; }

        public ImageType ImageType { get; set; }

        public Icon()
        {
            Entries = new List<Entry>();
            Images = new List<Image>();
        }

        public class Entry
        {
            public byte Width { get; set; }
            public byte Height { get; set; }
            public byte Colors { get; set; }
            public byte Reserved { get; set; }
            public ushort ColorPlanes { get; set; }
            public ushort BitsPerPixel { get; set; }
            public uint Size { get; set; }
            public uint Offset { get; set; }
            public byte[] ImageData { get; set; }

            public string Format
            {
                get
                {
                    if (ImageData == null || ImageData.Length < 4)
                        return null;
                    if ((ImageData[0] == 137) && (ImageData[1] == 80) && (ImageData[2] == 78) && (ImageData[3] == 71))
                        return "PNG";
                    return "BMP";
                }
            }
        }

        public void Load(string path)
        {
            using (var s = File.OpenRead(path))
            {
                using (var r = new BinaryReader(s))
                {
                    var reserved = r.ReadUInt16();
                    if (reserved != 0)
                    {
                        throw new InvalidOperationException();
                    }

                    this.ImageType = (ImageType)r.ReadUInt16();
                    var n = r.ReadUInt16();
                    for (int i = 0; i < n; i++)
                    {
                        var entry = new Entry();
                        entry.Width = r.ReadByte();
                        entry.Height = r.ReadByte();
                        entry.Colors = r.ReadByte();
                        entry.Reserved = r.ReadByte();
                        entry.ColorPlanes = r.ReadUInt16();
                        entry.BitsPerPixel = r.ReadUInt16();
                        entry.Size = r.ReadUInt32();
                        entry.Offset = r.ReadUInt32();
                        Entries.Add(entry);
                    }
                    for (int i = 0; i < n; i++)
                    {
                        var entry = Entries[i];
                        r.BaseStream.Seek(entry.Offset, SeekOrigin.Begin);
                        entry.ImageData = r.ReadBytes((int)entry.Size);

                        // load the image
                        using (var ms = new MemoryStream(entry.ImageData))
                        {
                            Images.Add(Image.FromStream(s));
                        }
                    }
                }
            }

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
                        w.Write((byte)i.Width); // Specifies image width in pixels. Can be any number between 0 and 255. Value 0 means image width is 256 pixels.
                        w.Write((byte)i.Height); // Specifies image height in pixels. Can be any number between 0 and 255. Value 0 means image height is 256 pixels.
                        w.Write((byte)0); // Specifies number of colors in the color palette. Should be 0 if the image does not use a color palette.
                        w.Write((byte)0); // Reserved
                        w.Write((short)0); // Specifies color planes. Should be 0 or 1
                        w.Write((short)0); // Specifies bits per pixel. 
                        var bytes = this.BitmapToByteArray(i);
                        buffers.Add(bytes);
                        var size = bytes.Length;
                        w.Write((uint)size); // Specifies the size of the image's data in bytes
                        w.Write((uint)offset); // Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file
                        offset += size;
                    }

                    // All image data referenced by entries in the image directory proceed directly after the image directory. 
                    // It is customary practice to store them in the same order as defined in the image directory.
                    foreach (var b in buffers)
                    {
                        // Recall that if an image is stored in BMP format, it must exclude the opening BITMAPFILEHEADER structure, 
                        // whereas if it is stored in PNG format, it must be stored in its entirety.
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
