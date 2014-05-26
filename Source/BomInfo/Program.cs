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
// <summary>
//   The 'BomInfo' program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace BomInfo
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;

    using LynxToolkit;

    /// <summary>
    /// The 'BomInfo' program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// List of BOMs that should not be reported.
        /// </summary>
        private static readonly List<string> DoNotReport = new List<string>();

        /// <summary>
        /// Patterns of file/directory names that should be ignored.
        /// </summary>
        private static readonly List<string> IgnorePatterns = new List<string>();

        /// <summary>
        /// Force to specified encoding.
        /// </summary>
        private static string force;

        static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);
            var directory = @".";
            var searchPattern = "*.cs";

            DoNotReport.Add("UTF-8");
            IgnorePatterns.Add("bin");
            IgnorePatterns.Add("obj");

            foreach (var arg in args)
            {
                var argx = arg.Split('=');
                switch (argx[0].ToLower())
                {
                    case "/donotreport":
                        DoNotReport.Add(argx[1]);
                        continue;
                    case "/ignore":
                        IgnorePatterns.Add(argx[1]);
                        continue;
                    case "/directory":
                        directory = argx[1];
                        continue;
                    case "/searchPattern":
                        searchPattern = argx[1];
                        continue;
                    case "/force":
                        force = argx[1];
                        continue;
                    default:
                        Console.WriteLine("Unknown argument: " + argx[0]);
                        break;
                }
            }

            Search(directory, searchPattern);
        }

        private static void Search(string directory, string searchPattern)
        {
            foreach (var file in Directory.GetFiles(directory, searchPattern))
            {
                var name = Path.GetFileName(file);
                if (Ignore(name))
                {
                    continue;
                }

                var info = GetBomInfo(file);
                if (DoNotReport.Contains(info))
                {
                    continue;
                }

                if (force != null && info != force)
                {
                    Fix(file);
                }

                Console.WriteLine(info + ": " + file);
            }

            foreach (var dir in Directory.GetDirectories(directory))
            {
                var name = Path.GetFileName(dir);
                if (Ignore(name))
                {
                    continue;
                }

                Search(dir, searchPattern);
            }
        }

        private static void Fix(string file)
        {
            var contents = File.ReadAllText(file);
            File.WriteAllText(file, contents, GetEncoding(force));
        }

        private static Encoding GetEncoding(string signatureName)
        {
            switch (signatureName)
            {
                case "UTF-8":
                    return Encoding.UTF8;
                default:
                    throw new NotImplementedException();
            }
        }

        private static bool Ignore(string name)
        {
            return IgnorePatterns.Contains(name);
        }

        private static string GetBomInfo(string file)
        {
            using (var stream = File.OpenRead(file))
            using (var reader = new BinaryReader(stream))
            {
                var bytes = reader.ReadBytes(3);
                if (bytes.Length > 2 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
                {
                    return "UTF-8";
                }

                if (bytes.Length > 1 && bytes[0] == 0xFE && bytes[1] == 0xFF)
                {
                    return "UTF-16 (BE)"; // big endian
                }

                if (bytes.Length > 1 && bytes[0] == 0xFF && bytes[1] == 0xFE)
                {
                    return "UTF-16 (LE)"; // little endian
                }
            }

            return "N/A";
        }
    }
}
