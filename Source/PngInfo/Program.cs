// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace PngInfo
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The Program
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Reads a big endian <see cref="Int32"/>.
        /// </summary>
        /// <param name="r">The reader.</param>
        /// <returns>An integer number.</returns>
        public static int ReadBigEndianInt32(this BinaryReader r)
        {
            var bytes = r.ReadBytes(4).Reverse().ToArray();
            return BitConverter.ToInt32(bytes, 0);
        }

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void Main(string[] args)
        {
            var file = args[0];
            Console.WriteLine(file);
            using (var s = File.OpenRead(args[0]))
            {
                using (var r = new BinaryReader(s))
                {
                    var signature = r.ReadBytes(8);
                    Console.Write("Signature: ");
                    for (int i = 0; i < 8; i++)
                    {
                        Console.Write("{0:X2} ", signature[i]);
                    }

                    Console.WriteLine();
                    while (true)
                    {
                        var length = r.ReadBigEndianInt32();
                        var chunkType = Encoding.UTF8.GetString(r.ReadBytes(4));
                        Console.WriteLine(chunkType + ": " + length + " bytes");

                        // Read chunk http://www.w3.org/TR/PNG-Chunks.html
                        switch (chunkType)
                        {
                            case "IHDR":
                                int width = r.ReadBigEndianInt32();
                                int height = r.ReadBigEndianInt32();
                                byte bitDepth = r.ReadByte();
                                byte colorType = r.ReadByte();
                                byte compressionMethod = r.ReadByte();
                                byte filterMethod = r.ReadByte();
                                byte interlaceMethod = r.ReadByte();
                                Console.WriteLine("  Width: " + width);
                                Console.WriteLine("  Height: " + height);
                                Console.WriteLine("  BitDepth: " + bitDepth);
                                Console.WriteLine("  ColorType: " + colorType);
                                Console.WriteLine("  CompressionMethod: " + compressionMethod);
                                Console.WriteLine("  FilterMethod: " + filterMethod);
                                Console.WriteLine("  InterlaceMethod: " + interlaceMethod);
                                break;
                            default:
                                r.ReadBytes(length);
                                break;
                        }

                        // read the crc
                        r.ReadBigEndianInt32();

                        if (chunkType == "IEND")
                        {
                            break;
                        }
                    }
                }
            }
        }
    }
}