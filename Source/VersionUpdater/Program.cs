//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//     Copyright © Lynx Toolkit.
// </copyright>
//-----------------------------------------------------------------------

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
    //   VersionUpdater.exe [/Directory=xxx] [/Version=x.x.x.x] [/Company=xxx] [/Copyright=xxx]
    // Arguments:
    //   /Directory specifies the root directory (all subdirectories will be scanned)
    //   /Version specifies the version number (Major.Minor.Build.Revision)
    //      * = automatic build/revision numbers
    //      yyyy = year
    //      MM = month
    //      dd = day
    //   /Company
    //   /Copyright

    // See also
    // http://www.codeproject.com/KB/XML/vrt.aspx

    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);

            // default parameters
            string version = "yyyy.MM.*";
            string informationalVersion = null;
            string company = null;
            string copyright = null;
            string directory = Directory.GetCurrentDirectory();

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] kv = arg.Split('=');
                if (kv.Length > 1)
                {
                    switch (kv[0])
                    {
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

            var updater = new Updater(version, informationalVersion, copyright, company);

            updater.ScanFolder(directory);
        }

        public class Updater
        {
            public Updater(string version, string informationalVersion, string copyright, string company)
            {
                Copyright = copyright;
                Company = company;

                version = version.Replace("yyyy", DateTime.Now.Year.ToString(CultureInfo.InvariantCulture));
                version = version.Replace("MM", DateTime.Now.Month.ToString(CultureInfo.InvariantCulture));
                version = version.Replace("dd", DateTime.Now.Day.ToString(CultureInfo.InvariantCulture));

                version = To16BitVersionNumbers(version);
                Version = version;
                InformationalVersion = informationalVersion ?? version;

                string nuSpecVersion = version.Replace(".*", ".0");

                FileVersion = nuSpecVersion;

                NuSpecReplacements = new Dictionary<Regex, string> { { new Regex(@"<version>.*</version>"), string.Format(@"<version>{0}</version>", nuSpecVersion) } };
                AssemblyInfoReplacements = new Dictionary<Regex, string>
                    {
                        { new Regex(@"AssemblyVersion\(.*\)"), string.Format("AssemblyVersion(\"{0}\")", Version) },
                        {
                            new Regex(@"AssemblyInformationalVersion\(.*\)"),
                            string.Format("AssemblyInformationalVersion(\"{0}\")", InformationalVersion)
                        },
                        {
                            new Regex(@"AssemblyFileVersion\(.*\)"),
                            string.Format("AssemblyFileVersion(\"{0}\")", FileVersion)
                        }
                    };
                if (Company != null)
                {
                    AssemblyInfoReplacements.Add(
                        new Regex(@"AssemblyCompany\(.*\)"), string.Format("AssemblyCompany(\"{0}\")", Company));
                }

                if (Copyright != null)
                {
                    AssemblyInfoReplacements.Add(
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
                            // only use the last 4 digts
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
                    UpdateFile(file, AssemblyInfoReplacements);
                }

                foreach (string file in Directory.GetFiles(path, "*.nuspec"))
                {
                    UpdateFile(file, NuSpecReplacements);
                }

                foreach (string dir in Directory.GetDirectories(path)) ScanFolder(dir);
            }

            private void UpdateFile(string file, Dictionary<Regex, string> replacements)
            {
                string text;
                using (var sr = new StreamReader(file))
                {
                    text = sr.ReadToEnd();

                    foreach (var kvp in replacements)
                    {
                        Regex regex = kvp.Key;
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
