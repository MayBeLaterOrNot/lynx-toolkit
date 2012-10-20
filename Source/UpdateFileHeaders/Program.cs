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

namespace UpdateFileHeaders
{
    using System;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    using LynxToolkit;

    /// <summary>
    /// The program.
    /// </summary>
    internal class Program
    {
        /// <summary>
        ///     The number of files scanned.
        /// </summary>
        private static int fileCount;

        /// <summary>
        ///     The number of files cleaned.
        /// </summary>
        private static int filesCleaned;

        /// <summary>
        ///     The arguments to check out files.
        /// </summary>
        private static string openForEditArguments;

        /// <summary>
        ///     The executable to check out files.
        /// </summary>
        private static string openForEditExecutable;

        /// <summary>
        ///     Determines whether to replace copyright symbol (C) or not.
        /// </summary>
        private static bool replaceSymbol;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">
        /// The args. 
        /// </param>
        private static void Main(string[] args)
        {
            Console.WriteLine(Application.Header);

            string company = null;
            string copyright = null;
            string exclude = "Packages *.Designer.cs obj bin _*";
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

        /// <summary>
        /// The updater.
        /// </summary>
        public class Updater
        {
            /// <summary>
            /// The ruler comment.
            /// </summary>
            private const string rulerComment =
                "// --------------------------------------------------------------------------------------------------------------------";

            /// <summary>
            /// Initializes a new instance of the <see cref="Updater"/> class.
            /// </summary>
            /// <param name="copyright">
            /// The copyright.
            /// </param>
            /// <param name="company">
            /// The company.
            /// </param>
            /// <param name="exclude">
            /// The exclude.
            /// </param>
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

            /// <summary>
            /// Gets or sets the company.
            /// </summary>
            public string Company { get; set; }

            /// <summary>
            /// Gets or sets the copyright.
            /// </summary>
            public string Copyright { get; set; }

            /// <summary>
            /// Gets or sets the exclude.
            /// </summary>
            public string Exclude { get; set; }

            /// <summary>
            /// The scan folder.
            /// </summary>
            /// <param name="path">
            /// The path.
            /// </param>
            public void ScanFolder(string path)
            {
                if (Utilities.IsExcluded(this.Exclude, path))
                {
                    return;
                }

                foreach (string file in Directory.GetFiles(path, "*.cs"))
                {
                    this.UpdateFile(file);
                }

                foreach (string dir in Directory.GetDirectories(path))
                {
                    this.ScanFolder(dir);
                }
            }

            /// <summary>
            /// The update file.
            /// </summary>
            /// <param name="file">
            /// The file.
            /// </param>
            private void UpdateFile(string file)
            {
                if (Utilities.IsExcluded(this.Exclude, file))
                {
                    return;
                }

                string fileName = Path.GetFileName(file);
                string input = File.ReadAllText(file);
                Match summaryMatch = Regex.Match(
                    input, 
                    @"///\s*<summary>\s*^(.*?)$\s*///\s*</summary>", 
                    RegexOptions.Multiline | RegexOptions.Singleline);
                string summary = string.Empty;
                if (summaryMatch.Success)
                {
                    summary =
                        Regex.Replace(summaryMatch.Groups[1].Value, @"^\s*///\s*", "//   ", RegexOptions.Multiline)
                            .Trim();
                }

                var sb = new StringBuilder();
                sb.AppendLine(rulerComment);
                sb.AppendFormat("// <copyright file=\"{0}\" company=\"{1}\">", fileName, this.Company);
                sb.AppendLine();
                foreach (string line in this.Copyright.Split('\n'))
                {
                    sb.AppendLine(string.Format("//   {0}", line.Trim()).Trim());
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
                        string line = r.ReadLine();
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
                string output = sb.ToString().Trim();
                string original = File.ReadAllText(file);
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