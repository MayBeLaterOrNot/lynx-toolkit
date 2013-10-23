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
//   Modifies the version numbers to be within 16-bit boundaries.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace VersionUpdater
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

    // Description:
    //   Does a recursive scan on all AssemblyInfo.cs and *.nuspec files from the specified root folder.
    //   The program updates version and company/copyright information.
    //
    // Syntax:
    //   VersionUpdater.exe [/Directory=..\src] [/VersionFile=..\version.txt] [/Version=x.x.x.x] [/Build=x] [/Revision=x] [/Company=xxx] [/Copyright=xxx]
    // Arguments:
    //   /Directory specifies the root directory (all subdirectories will be scanned)
    //   /Version specifies the version number (Major.Minor.Build.Revision)
    //      * = automatic build/revision numbers
    //      yyyy = year
    //      MM = month
    //      dd = day
    //   /VersionFile gets the version number from a file
    //   /Build - overwrites the build number
    //   /Revision - overwrites the revision number
    //   /Company
    //   /Copyright

    // See also
    // http://www.codeproject.com/KB/XML/vrt.aspx

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Utilities.ApplicationHeader);

            // default parameters
            string version = "yyyy.MM.*";
            string informationalVersion = null;
            string company = null;
            string copyright = null;
            string directory = Directory.GetCurrentDirectory();
            string build = null;
            string revision = null;

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] kv = arg.Split('=');
                if (kv.Length > 1)
                {
                    switch (kv[0])
                    {
                        case "/VersionFile":
                            version = GetVersionFromFile(kv[1]);
                            break;
                        case "/Version":
                            version = kv[1];
                            break;
                        case "/InformationalVersion":
                            informationalVersion = kv[1];
                            break;
                        case "/Directory":
                            directory = kv[1];
                            break;
                        case "/Company":
                            company = kv[1];
                            break;
                        case "/Copyright":
                            copyright = kv[1];
                            break;
                        case "/Build":
                            build = kv[1];
                            break;
                        case "/Revision":
                            revision = kv[1];
                            break;
                    }
                }
                else
                {
                    directory = arg;
                }
            }

            if (copyright == null && company != null)
            {
                copyright = string.Format("Copyright © {0} {1}", company, DateTime.Now.Year);
            }

            var updater = new Updater(version, informationalVersion, copyright, company, build, revision);

            updater.ScanFolder(directory);
        }

        /// <summary>
        /// Gets the version number from a text file.
        /// </summary>
        /// <param name="fileName">Path to a text file.</param>
        /// <returns>The version number, or nothing if the file was empty.</returns>
        private static string GetVersionFromFile(string fileName)
        {
            var lines = File.ReadAllLines(fileName);

            // search for a line containing 'version' (case-insensitive)
            foreach (var line in lines)
            {
                var i = line.IndexOf("version", StringComparison.InvariantCultureIgnoreCase);
                if (i >= 0)
                {
                    // return the remaining part of this line
                    return line.Substring(i + 7).Trim();
                }
            }

            if (lines.Length == 0)
            {
                return string.Empty;
            }

            return lines[0];
        }

        public class Updater
        {
            public Updater(string version, string informationalVersion, string copyright, string company, string build, string revision)
            {
                this.Copyright = copyright;
                this.Company = company;

                version = version.Replace("yyyy", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture));
                version = version.Replace("MM", DateTime.Now.Month.ToString(CultureInfo.InvariantCulture));
                version = version.Replace("dd", DateTime.Now.Day.ToString(CultureInfo.InvariantCulture));

                // split up the version number string
                var versionNumbers = version.Split('.');
                if (versionNumbers.Length < 4)
                {
                    var tmp = new string[4];
                    for (int i = 0; i < 4; i++)
                    {
                        tmp[i] = i < versionNumbers.Length ? versionNumbers[i] : "0";
                    }

                    versionNumbers = tmp;
                }

                // replace build and revision number if they are specified
                if (build != null)
                {
                    versionNumbers[2] = build;
                }

                if (revision != null)
                {
                    versionNumbers[3] = revision;
                }

                // rebuild the version number string
                version = string.Format("{0}.{1}.{2}.{3}", versionNumbers[0], versionNumbers[1], versionNumbers[2], versionNumbers[3]);

                // truncate to 16 bit values
                version = To16BitVersionNumbers(version);

                this.Version = version;
                this.InformationalVersion = informationalVersion ?? version;

                string nuspecVersion = version.Replace(".*", ".0");

                this.FileVersion = nuspecVersion;

                this.NuSpecReplacements = new Dictionary<Regex, string> { { new Regex(@"<version>.*</version>"), string.Format(@"<version>{0}</version>", nuspecVersion) } };
                this.AssemblyInfoReplacements = new Dictionary<Regex, string>
                    {
                        { new Regex(@"AssemblyVersion\(.*\)"), string.Format("AssemblyVersion(\"{0}\")", this.Version) },
                        {
                            new Regex(@"AssemblyInformationalVersion\(.*\)"),
                            string.Format("AssemblyInformationalVersion(\"{0}\")", this.InformationalVersion)
                        },
                        {
                            new Regex(@"AssemblyFileVersion\(.*\)"),
                            string.Format("AssemblyFileVersion(\"{0}\")", this.FileVersion)
                        }
                    };

                if (this.Company != null)
                {
                    this.AssemblyInfoReplacements.Add(
                        new Regex(@"AssemblyCompany\(.*\)"), string.Format("AssemblyCompany(\"{0}\")", this.Company));
                }

                if (this.Copyright != null)
                {
                    this.AssemblyInfoReplacements.Add(
                        new Regex(@"AssemblyCopyright\(.*\)"), string.Format("AssemblyCopyright(\"{0}\")", Copyright));
                }
            }

            /// <summary>
            /// Modifies the version numbers to be within 16-bit boundaries.
            /// </summary>
            /// <param name="version">The version.</param>
            /// <returns>The modified version number.</returns>
            private static string To16BitVersionNumbers(string version)
            {
                var vs = version.Split('.');
                var sb = new StringBuilder();
                foreach (var s in vs)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(".");
                    }

                    long i;
                    if (long.TryParse(s, out i)
                        || long.TryParse(s, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out i))
                    {
                        if (i > 65534)
                        {
                            // only use the last 4 digits
                            i = i % 10000;
                        }

                        sb.Append(i);
                    }
                    else
                    {
                        sb.Append(s);
                    }
                }

                return sb.ToString();
            }

            public string Copyright { get; set; }

            public string Company { get; set; }

            public string Version { get; set; }

            public string InformationalVersion { get; set; }

            public string FileVersion { get; set; }

            public Dictionary<Regex, string> NuSpecReplacements { get; set; }

            public Dictionary<Regex, string> AssemblyInfoReplacements { get; set; }

            public void ScanFolder(string path)
            {
                foreach (string file in Directory.GetFiles(path, "*AssemblyInfo.cs"))
                {
                    this.UpdateFile(file, this.AssemblyInfoReplacements);
                }

                foreach (string file in Directory.GetFiles(path, "*.nuspec"))
                {
                    this.UpdateFile(file, this.NuSpecReplacements);
                }

                foreach (string dir in Directory.GetDirectories(path))
                {
                    this.ScanFolder(dir);
                }
            }

            private void UpdateFile(string file, Dictionary<Regex, string> replacements)
            {
                string text;
                using (var sr = new StreamReader(file))
                {
                    text = sr.ReadToEnd();

                    foreach (var kvp in replacements)
                    {
                        var regex = kvp.Key;
                        string replacement = kvp.Value;
                        text = regex.Replace(text, replacement);
                    }
                }

                File.WriteAllText(file, text, Encoding.UTF8);

                this.Log(file);
            }

            private void Log(string msg)
            {
                Console.WriteLine(msg);
            }
        }
    }
}