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
//   The program.
// </summary>
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
        private static bool copyrightSymbol;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        private static void Main(string[] args)
        {
            //// StyleCop rule SA1633 states that a C# code file should have a standard file header.
            //// http://stylecop.soyuz5.com/SA1633.html File must have header
            //// http://stylecop.soyuz5.com/SA1634.html Must show copyright
            //// http://stylecop.soyuz5.com/SA1635.html Must have copyright text
            //// http://stylecop.soyuz5.com/SA1636.html Copyright text must match
            //// http://stylecop.soyuz5.com/SA1637.html Must contain file name
            //// http://stylecop.soyuz5.com/SA1638.html File name documentation must match file name
            //// http://stylecop.soyuz5.com/SA1639.html Must have summary
            //// http://stylecop.soyuz5.com/SA1640.html Must have valid company text
            //// http://stylecop.soyuz5.com/SA1641.html Company name must match

            //// if the file is auto-generated, the file header should not be modified.

            Console.WriteLine(Utilities.ApplicationHeader);

            if (args.Length == 0)
            {
                Console.WriteLine(Utilities.ApplicationDescription);
                Console.WriteLine();
                Console.WriteLine("UpdateFileHeaders /company=companyName [/copyright=copyrightNotice] [/copyright-file=path] [/exclude=filestoExclude]");
                Console.WriteLine("                 [/directory=directory] [/scc=p4] [/replacesymbol]");
                Console.WriteLine();
                Console.WriteLine("  /company          the company name (required)");
                Console.WriteLine("  /copyright        the copyright text");
                Console.WriteLine("  /copyright-file   path to a file containing the copyright text");
                Console.WriteLine("  /exclude          directory/file exclude pattern");
                Console.WriteLine("  /directory        directory to search");
                Console.WriteLine("  /scc              version control system (e.g. 'p4')");
                Console.WriteLine("  /copyrightsymbol  replace (C) and (c) by ©");
                return;
            }

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
                    case "/copyrightsymbol":
                        copyrightSymbol = true;
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

                var updater = new HeaderUpdater(copyright, company, exclude);
                updater.ScanFolder(directory);

                Console.WriteLine("{0} files updated (of {1})", filesCleaned, fileCount);
            }
        }

        /// <summary>
        /// Implements the header updater tool.
        /// </summary>
        public class HeaderUpdater
        {
            /// <summary>
            /// The ruler comment.
            /// </summary>
            private const string RulerComment =
                "// --------------------------------------------------------------------------------------------------------------------";

            /// <summary>
            /// Initializes a new instance of the <see cref="HeaderUpdater"/> class.
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
            public HeaderUpdater(string copyright, string company, string exclude)
            {
                if (copyright == null && company != null)
                {
                    copyright = string.Format("Copyright (C) {0}. All rights reserved.", company);
                }

                if (copyrightSymbol && copyright != null)
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
            /// Scans the specified folder.
            /// </summary>
            /// <param name="path">
            /// The path to the folder.
            /// </param>
            public void ScanFolder(string path)
            {
                if (Utilities.IsExcluded(this.Exclude, path))
                {
                    return;
                }

                foreach (var file in Directory.GetFiles(path, "*.cs"))
                {
                    this.UpdateFile(file);
                }

                foreach (var dir in Directory.GetDirectories(path))
                {
                    this.ScanFolder(dir);
                }
            }

            /// <summary>
            /// Updates the specified file.
            /// </summary>
            /// <param name="path">
            /// The path to the file.
            /// </param>
            private void UpdateFile(string path)
            {
                if (Utilities.IsExcluded(this.Exclude, path))
                {
                    return;
                }

                string fileName = Path.GetFileName(path);
                string input = File.ReadAllText(path);

                // check if the file is auto-generated
                if (Regex.Match(input, "<auto-generated\\s*/>").Success)
                {
                    return;
                }

                var summaryMatch = Regex.Match(
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
                sb.AppendLine(RulerComment);
                sb.AppendFormat("// <copyright file=\"{0}\" company=\"{1}\">", fileName, this.Company);
                sb.AppendLine();
                foreach (string line in this.Copyright.Split('\n'))
                {
                    sb.AppendLine(string.Format("//   {0}", line.Trim()));
                }

                sb.AppendLine("// </copyright>");
                if (!string.IsNullOrWhiteSpace(summary))
                {
                    sb.AppendLine("// <summary>");
                    sb.AppendLine(RemoveTags(summary));
                    sb.AppendLine("// </summary>");
                }

                sb.AppendLine(RulerComment);
                sb.AppendLine();

                using (var r = new StreamReader(path))
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
                string original = File.ReadAllText(path);
                if (string.Equals(original, output))
                {
                    return;
                }

                Console.WriteLine("  " + path);
                Utilities.OpenForEdit(path, openForEditExecutable, openForEditArguments);
                File.WriteAllText(path, output, Encoding.UTF8);

                filesCleaned++;
            }

            /// <summary>
            /// Removes XML comment tags from a summary comment.
            /// </summary>
            /// <param name="summary"></param>
            /// <returns></returns>
            private static string RemoveTags(string summary)
            {
                // remove <c> and </c> tags
                summary = Regex.Replace(summary, "</?c>", string.Empty);

                // remove <see cref="..." />
                summary = Regex.Replace(summary, "<see cref=\"(.+?)\"\\s*/>", string.Empty);

                // remove <paramref name="..." />
                summary = Regex.Replace(summary, "<paramref name=\"(.+?)\"\\s*/>", string.Empty);

                return summary;
            }
        }
    }
}