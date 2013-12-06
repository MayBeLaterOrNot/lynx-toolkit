// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx Toolkit">
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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Threading;
    using System.Threading.Tasks;

    using LynxToolkit;

    internal class Program
    {
        private static string Host { get; set; }

        private static string Password { get; set; }

        private static bool RecursiveSearch { get; set; }

        private static int UploadThreads { get; set; }

        private static string UserName { get; set; }

        private static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);
            if (args.Length < 5)
            {
                Console.WriteLine("Missing arguments.");
                return;
            }

            UploadThreads = 3;
            RecursiveSearch = true;
            Host = args[0];
            UserName = args[1];
            Password = args[2];
            var localFile = args[3];
            var remoteFile = args[4];

            for (int i = 5; i < args.Length; i++)
            {
                var kv = args[i].Split('=');
                switch (kv[0].ToLower())
                {
                    case "/r":
                        RecursiveSearch = true;
                        break;
                    case "/t":
                        UploadThreads = int.Parse(kv[1]);
                        break;
                }
            }

            Console.WriteLine("{0} => {1}/{2}", localFile, Host, remoteFile);
            Console.WriteLine();

            var w = Stopwatch.StartNew();

            if (localFile.Contains("*"))
            {
                UploadFiles(remoteFile, localFile);
            }
            else
            {
                var length = UploadFile(
                    remoteFile,
                    localFile,
                    1024,
                    (x, y) =>
                    {
                        Console.Write("#");
                        return true;
                    });
                Console.WriteLine("\n{0} bytes uploaded.", length);
            }

            Console.WriteLine();
            Console.WriteLine("Upload time: {0}:{1:00}", w.Elapsed.Minutes, w.Elapsed.Seconds);
        }

        private static long UploadFile(string remoteFile, string localFile, int bufferSize = 2048, Func<long, long, bool> callback = null)
        {
            try
            {
                var request = (FtpWebRequest)WebRequest.Create("ftp://" + Host + "/" + remoteFile);
                request.Credentials = new NetworkCredential(UserName, Password);
                request.UseBinary = true;
                request.UsePassive = true;
                request.KeepAlive = true;
                var fi = new FileInfo(localFile);
                request.ContentLength = fi.Length;
                request.Method = WebRequestMethods.Ftp.UploadFile;

                long total = 0;
                var requestStream = request.GetRequestStream();
                using (var localFileStream = new FileStream(localFile, FileMode.Open))
                {
                    var byteBuffer = new byte[bufferSize];
                    while (true)
                    {
                        // Read from local stream
                        var bytes = localFileStream.Read(byteBuffer, 0, bufferSize);
                        if (bytes == 0)
                        {
                            break;
                        }

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
                }

                requestStream.Flush();
                
                // don't know why, but this seems to help...
                Thread.Sleep(150); 
                requestStream.Close();
                
                var response = (FtpWebResponse)request.GetResponse();
                response.Close();
                
                return total;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return -1;
            }
        }

        private static void UploadFiles(string remoteDirectory, string localPath)
        {
            var localDir = Path.GetDirectoryName(localPath) ?? ".";
            var searchPattern = Path.GetFileName(localPath) ?? "*.*";

            var files = RecursiveSearch
                            ? Utilities.FindFiles(localDir, searchPattern)
                            : Directory.GetFiles(localDir, searchPattern);
            var q = new ConcurrentQueue<string>(files);
            var tasks = new List<Task>();
            if (UploadThreads == 1)
            {
                ProcessQueue(remoteDirectory, q, localDir);
            }
            else
            {
                for (int i = 0; i < UploadThreads; i++)
                {
                    tasks.Add(Task.Factory.StartNew(() => ProcessQueue(remoteDirectory, q, localDir)));
                }

                while (!tasks.All(t => t.IsCompleted))
                {
                    Thread.Sleep(1000);
                }
            }
        }

        private static void ProcessQueue(string remoteDirectory, ConcurrentQueue<string> q, string localDir)
        {
            while (q.Count > 0)
            {
                string f;
                if (!q.TryDequeue(out f))
                {
                    continue;
                }

                var remoteFile = f.Replace(localDir, remoteDirectory).Replace('\\', '/');
                Console.WriteLine(f);
                UploadFile(remoteFile, f);
            }
        }
    }
}