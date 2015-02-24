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
namespace UpdateVersionNumbers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Utilities.ApplicationHeader);

            // default parameters
            var updater = new Updater { Version = "yyyy.MM.*" };
            string directory = Directory.GetCurrentDirectory();

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] kv = arg.Split('=');
                if (kv.Length > 1)
                {
                    switch (kv[0])
                    {
                        case "/VersionFromNuGet":
                            var version = GetVersionFromNuGetPackage(kv[1], true);
                            var versionParts = version.Split('-');
                            if (versionParts.Length > 1)
                            {
                                updater.PreRelease = versionParts[1];
                            }

                            updater.Version = versionParts[0];
                            break;
                        case "/VersionFile":
                            updater.Version = GetVersionFromFile(kv[1]);
                            break;
                        case "/Version":
                            updater.Version = kv[1];
                            break;
                        case "/PreRelease":
                            updater.PreRelease = kv[1];
                            break;
                        case "/InformationalVersion":
                            updater.InformationalVersion = kv[1];
                            break;
                        case "/ReleaseNotesFile":
                            updater.ReleaseNotes = GetFromFile(kv[1]);
                            break;
                        case "/ExtractReleaseNotes":
                            updater.ReleaseNotes = Extract(@"\n\#\#.*?\n(.*?)\n\#\#[^#]", GetFromFile(kv[1]));
                            break;
                        case "/Directory":
                            directory = kv[1];
                            break;
                        case "/Company":
                            updater.Company = kv[1];
                            break;
                        case "/Copyright":
                            updater.Copyright = kv[1];
                            break;
                        case "/Build":
                            updater.Build = kv[1];
                            break;
                        case "/Revision":
                            updater.Revision = kv[1];
                            break;
                        case "/Dependency":
                            updater.Dependencies.Add(kv[1]);
                            break;
                    }
                }
                else
                {
                    directory = arg;
                }
            }

            if (updater.Copyright == null && updater.Company != null)
            {
                updater.Copyright = string.Format("Copyright © {0} {1}", updater.Company, DateTime.Now.Year);
            }

            updater.Initialize();

            Console.WriteLine("Version               = '{0}'", updater.Version);
            Console.WriteLine("File Version          = '{0}'", updater.FileVersion);
            Console.WriteLine("Informational Version = '{0}'", updater.InformationalVersion);
            Console.WriteLine("NuGet Version         = '{0}'", updater.NuGetVersion);

            if (updater.Copyright != null)
            {
                Console.WriteLine("Copyright             = '{0}'", updater.Copyright);
            }

            if (updater.Company != null)
            {
                Console.WriteLine("Company               = '{0}'", updater.Company);
            }

            if (updater.ReleaseNotes != null)
            {
                Console.WriteLine("Release Notes         = '{0}'", updater.ReleaseNotes);
            }

            Console.WriteLine();

            updater.UpdateFolder(directory);
        }

        /// <summary>
        /// Escapes the specified string for XML.
        /// </summary>
        /// <param name="unescaped">The unescaped string.</param>
        /// <returns>An escaped string.</returns>
        private static string XmlEscape(string unescaped)
        {
            var doc = new XmlDocument();
            var node = doc.CreateElement("root");
            node.InnerText = unescaped;
            return node.InnerXml;
        }

        /// <summary>
        /// Gets the version from a NuGet package.
        /// </summary>
        /// <param name="packageId">The package identifier.</param>
        /// <param name="includePrerelease">Include pre-release versions if set to <c>true</c>.</param>
        /// <returns>A version number.</returns>
        private static string GetVersionFromNuGetPackage(string packageId, bool includePrerelease)
        {
            var client = new WebClient();
            var address = string.Format("http://www.nuget.org/api/v2/package-versions/{0}", packageId);
            if (includePrerelease)
            {
                address += "?includePrerelease=true";
            }

            var response = client.DownloadString(address);
            var versions = response.Trim("[]".ToCharArray()).Split(',');
            return versions.Last().Trim("\"".ToCharArray());
        }

        /// <summary>
        /// Gets the version number from a file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns>A version number.</returns>
        private static string GetFromFile(string fileName)
        {
            return File.ReadAllText(fileName).Trim();
        }

        /// <summary>
        /// Extracts the text matching a regular expression.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="input">The input.</param>
        /// <returns>The text of the first group in the pattern.</returns>
        private static string Extract(string pattern, string input)
        {
            var match = Regex.Match(input, pattern, RegexOptions.Singleline);
            if (match.Success)
            {
                return match.Groups[1].Value.Trim();
            }

            return input;
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
            public Updater()
            {
                this.Dependencies = new List<string>();
            }

            public string Copyright { get; set; }

            public string Company { get; set; }

            public string Version { get; set; }

            public string NuGetVersion { get; set; }

            public string PreRelease { get; set; }

            public string Build { get; set; }

            public string Revision { get; set; }

            public string ReleaseNotes { get; set; }

            public string InformationalVersion { get; set; }

            public string FileVersion { get; set; }

            public Dictionary<Regex, string> NuSpecReplacements { get; set; }

            public Dictionary<Regex, string> AssemblyInfoReplacements { get; set; }

            public List<string> Dependencies { get; private set; }

            public void Initialize()
            {
                this.Version = this.CreateVersionNumber(this.Version);

                // AssemblyInfo
                this.FileVersion = this.Version.Replace(".*", ".0");
                this.InformationalVersion = this.InformationalVersion ?? this.Version;
                if (this.PreRelease != null)
                {
                    this.InformationalVersion += "-" + this.PreRelease;
                }


                // NuGet package version
                this.NuGetVersion = this.Version.Replace(".*", ".0");
                if (this.PreRelease != null)
                {
                    this.NuGetVersion += "-" + this.PreRelease;
                }


                this.NuSpecReplacements = new Dictionary<Regex, string> 
                {
                    { new Regex(@"<version>.*</version>"), string.Format(@"<version>{0}</version>", this.NuGetVersion) },
                    { new Regex(@"\$version"), this.NuGetVersion }, 
                };

                if (this.Copyright != null)
                {
                    this.NuSpecReplacements.Add(new Regex(@"\$copyright"), this.Copyright);
                }

                foreach (var dependency in this.Dependencies)
                {
                    this.NuSpecReplacements.Add(new Regex("(<dependency id=\"" + dependency + "\" version=\"\\[?)(.*?)(\\]?\"\\s?/>)"), "${1}" + this.NuGetVersion + "${3}");
                }

                if (this.ReleaseNotes != null)
                {
                    this.NuSpecReplacements.Add(
                        new Regex(@"<releaseNotes>.*</releaseNotes>"),
                        string.Format(@"<releaseNotes>{0}</releaseNotes>", XmlEscape(this.ReleaseNotes)));
                }

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
                        new Regex(@"AssemblyCopyright\(.*\)"), string.Format("AssemblyCopyright(\"{0}\")", this.Copyright));
                }
            }

            /// <summary>
            /// Updates the specified folder.
            /// </summary>
            /// <param name="path">The folder path.</param>
            public void UpdateFolder(string path)
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
                    this.UpdateFolder(dir);
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

            /// <summary>
            /// Creates the version number.
            /// </summary>
            /// <param name="version">The version format string.</param>
            /// <returns>A version string.</returns>
            private string CreateVersionNumber(string version)
            {
                version = version.Replace("yyyy", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture));
                version = version.Replace("MM", DateTime.Now.Month.ToString(CultureInfo.InvariantCulture));
                version = version.Replace("dd", DateTime.Now.Day.ToString(CultureInfo.InvariantCulture));

                // split up the version number string
                var versionNumbers = version.Split('.').ToList();

                // replace build and revision number if they are specified
                if (this.Build != null)
                {
                    while (versionNumbers.Count < 3)
                    {
                        versionNumbers.Add("0");
                    }

                    versionNumbers[2] = this.Build;
                }

                if (this.Revision != null)
                {
                    while (versionNumbers.Count < 4)
                    {
                        versionNumbers.Add("0");
                    }

                    versionNumbers[3] = this.Revision;
                }

                // rebuild the version number string
                version = string.Join(".", versionNumbers);

                // truncate to 16 bit values
                version = To16BitVersionNumbers(version);

                return version;
            }

            /// <summary>
            /// Updates the specified file.
            /// </summary>
            /// <param name="file">The file path.</param>
            /// <param name="replacements">The update expressions and replacement strings.</param>
            private void UpdateFile(string file, Dictionary<Regex, string> replacements)
            {
                var originalContent = File.ReadAllText(file);
                var updatedContent = originalContent;

                foreach (var kvp in replacements)
                {
                    var expression = kvp.Key;
                    var replacement = kvp.Value;
                    updatedContent = expression.Replace(updatedContent, replacement);
                }

                if (!string.Equals(originalContent, updatedContent))
                {
                    File.WriteAllText(file, updatedContent, Encoding.UTF8);
                    Console.WriteLine(file);
                }
            }
        }
    }
}