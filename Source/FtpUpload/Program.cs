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
namespace FtpUpload
{
    using System;
    using System.IO;
    using System.Net;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);
            if (args.Length < 5)
            {
                Console.WriteLine("Missing arguments.");
                return;
            }

            var host = args[0];
            var userName = args[1];
            var password = args[2];
            var localFile = args[3];
            var remoteFile = args[4];
            Console.WriteLine("{0} => {1}/{2}", localFile, host, remoteFile);
            var length = UploadFile(host, userName, password, remoteFile, localFile, 1024, (x, y) =>
            {
                Console.Write("#");
                return true;
            });
            Console.WriteLine("\n{0} bytes uploaded.", length);

        }



        public static long UploadFile(string host, string userName, string password, string remoteFile, string localFile, int bufferSize = 2048, Func<long, long, bool> callback = null)
        {
            try
            {

                var request = (FtpWebRequest)WebRequest.Create("ftp://" + host + "/" + remoteFile);
                request.Credentials = new NetworkCredential(userName, password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (var requestStream = request.GetRequestStream())
                {
                    using (var localFileStream = new FileStream(localFile, FileMode.Open))
                    {
                        var byteBuffer = new byte[bufferSize];
                        long total = 0;
                        while (true)
                        {
                            // Read from local stream
                            var bytes = localFileStream.Read(byteBuffer, 0, bufferSize);
                            if (bytes == 0) break;

                            // Write to request stream
                            requestStream.Write(byteBuffer, 0, bytes);
                            total += bytes;
                            if (callback != null)
                            {
                                var cont = callback(localFileStream.Length, total);
                                if (!cont)
                                {
                                    break;
                                }
                            }
                        }
                        return total;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

    }
}