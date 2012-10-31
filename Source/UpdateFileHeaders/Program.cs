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
// <summary>
//   The executable to check out files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace UpdateFileHeaders
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using LynxToolkit;

    internal class Program
    {
        /// <summary>
        /// The executable to check out files.
        /// </summary>
        private static string openForEditExecutable;

        /// <summary>
        /// The arguments to check out files.
        /// </summary>
        private static string openForEditArguments;

        /// <summary>
        /// Determines whether to replace copyright symbol (C) or not.
        /// </summary>
        private static bool replaceSymbol;

        /// <summary>
        /// The number of files cleaned.
        /// </summary>
        private static int filesCleaned;

        /// <summary>
        /// The number of files scanned.
        /// </summary>
        private static int fileCount;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The args.</param>
        private static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);

            string company = null;
            string copyright = null;
            var exclude = "AssemblyInfo.cs Packages *.Designer.cs obj bin _*";
            string directory = Directory.GetCurrentDirectory();

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] kv = arg.Split('=');
                switch (kv[0].ToLower())
                {
                    case "/directory":
                        directory = kv[1];
                        break;
                    case "/exclude":
                        exclude = kv[1];
                        continue;
                    case "/replacesymbol":
                        replaceSymbol = true;
                        continue;
                    case "/company":
                        company = kv[1];
                        continue;
                    case "/copyright":
                        copyright = kv[1];
                        continue;
                    case "/copyright-file":
                        copyright = File.ReadAllText(kv[1]);
                        continue;
                    case "/scc":
                        if (string.Equals(kv[1], "p4", StringComparison.InvariantCultureIgnoreCase))
                        {
                            openForEditExecutable = "p4.exe";
                            openForEditArguments = "edit {0}";
                        }

                        continue;
                    default:
                        directory = arg;
                        break;
                }

                var updater = new Updater(copyright, company, exclude);
                updater.ScanFolder(directory);

                Console.WriteLine("{0} files updated (of {1})", filesCleaned, fileCount);
            }
        }

        public class Updater
        {
            public Updater(string copyright, string company, string exclude)
            {
                if (copyright == null && company != null)
                {
                    copyright = string.Format("Copyright © {0}. All rights reserved.", company);
                }

                if (replaceSymbol && copyright != null)
                {
                    copyright = copyright.Replace("(c)", "©");
                    copyright = copyright.Replace("(C)", "©");
                }

                this.Copyright = copyright;
                this.Company = company;
                this.Exclude = exclude;
            }

            public string Copyright { get; set; }

            public string Company { get; set; }

            public string Exclude { get; set; }

            public void ScanFolder(string path)
            {
                if (Utilities.IsExcluded(Exclude, path))
                {
                    return;
                }

                foreach (string file in Directory.GetFiles(path, "*.cs"))
                {
                    this.UpdateFile(file);
                }

                foreach (var dir in Directory.GetDirectories(path))
                {
                    this.ScanFolder(dir);
                }
            }


            private const string rulerComment =
                "// --------------------------------------------------------------------------------------------------------------------";
            private void UpdateFile(string file)
            {
                if (Utilities.IsExcluded(Exclude, file))
                {
                    return;
                }

                var fileName = Path.GetFileName(file);
                var input = File.ReadAllText(file);
                var summaryMatch = Regex.Match(
                    input,
                    @"///\s*<summary>\s*^(.*?)$\s*///\s*</summary>",
                    RegexOptions.Multiline | RegexOptions.Singleline);
                string summary = string.Empty;
                if (summaryMatch.Success)
                {
                    summary = Regex.Replace(summaryMatch.Groups[1].Value, @"^\s*///\s*", "//   ", RegexOptions.Multiline).Trim();
                }

                var sb = new StringBuilder();
                sb.AppendLine(rulerComment);
                sb.AppendFormat("// <copyright file=\"{0}\" company=\"{1}\">", fileName, this.Company);
                sb.AppendLine();
                foreach (var line in this.Copyright.Split('\n'))
                {
                    sb.AppendLine(string.Format("//   {0}", line.Trim()));
                }

                sb.AppendLine("// </copyright>");
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    sb.AppendLine("// <summary>");
                    sb.AppendLine(summary);
                    sb.AppendLine("// </summary>");
                }

                sb.AppendLine(rulerComment);

                using (var r = new StreamReader(file))
                {
                    bool isHeader = true;
                    while (!r.EndOfStream)
                    {
                        var line = r.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//"))
                        {
                            isHeader = false;
                        }

                        if (!isHeader)
                        {
                            sb.AppendLine(line);
                        }
                    }
                }

                fileCount++;
                var output = sb.ToString().Trim();
                var original = File.ReadAllText(file);
                if (string.Equals(original, output))
                {
                    return;
                }

                Console.WriteLine("  " + file);
                Utilities.OpenForEdit(file, openForEditExecutable, openForEditArguments);
                File.WriteAllText(file, output, Encoding.UTF8);

                filesCleaned++;
            }
        }
    }
}