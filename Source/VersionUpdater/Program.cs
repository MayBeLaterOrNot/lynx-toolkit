//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VersionUpdater
{
    using System.Text;

    // Version information for an assembly consists of the following four values:
    //
    //      Major Version
    //      Minor Version 
    //      Build Number
    //      Revision
    //
    // You can specify all the values or you can default the Build and Revision Numbers 
    // by using the '*' as shown below:

    // See also
    // http://www.codeproject.com/KB/XML/vrt.aspx

    internal class Program
    {
        private static void Main(string[] args)
        {
            // default parameters
            string version = "yyyy.MM.*";
            string company = null;
            string copyright = null;
            string directory = Directory.GetCurrentDirectory();

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] kv = arg.Split('=');
                switch (kv[0])
                {
                    case "/Version":
                        version = kv[1];
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

            if (copyright == null && company != null) copyright = string.Format("Copyright © {0} {1}", company, DateTime.Now.Year);

            var updater = new Updater(version, copyright, company);

            updater.ScanFolder(directory);
        }
    }

    public class Updater
    {
        public Updater(string version, string copyright, string company)
        {
            Copyright = copyright;
            Company = company;

            version = version.Replace("yyyy", DateTime.Now.Year.ToString());
            version = version.Replace("MM", DateTime.Now.Month.ToString());
            version = version.Replace("dd", DateTime.Now.Day.ToString());

            version = To16bitVersionNumbers(version);

            Version = version;

            string nuSpecVersion = version.Replace(".*", ".0");

            FileVersion = nuSpecVersion;

            NuSpecReplacements = new Dictionary<Regex, string>
                                     {
                                         {
                                             new Regex(@"<version>.*</version>"),
                                             string.Format(@"<version>{0}</version>", nuSpecVersion)
                                             }
                                     };
            AssemblyInfoReplacements = new Dictionary<Regex, string>
                                           {
                                               {
                                                   new Regex(@"AssemblyVersion\(.*\)"),
                                                   string.Format("AssemblyVersion(\"{0}\")", Version)
                                                   },
                                               {
                                                   new Regex(@"AssemblyFileVersion\(.*\)"),
                                                   string.Format("AssemblyFileVersion(\"{0}\")", FileVersion)
                                                   }
                                           };
            if (Company != null)
            {
                AssemblyInfoReplacements.Add(new Regex(@"AssemblyCompany\(.*\)"), string.Format("AssemblyCompany(\"{0}\")", Company));
            }

            if (Copyright != null)
            {
                AssemblyInfoReplacements.Add(new Regex(@"AssemblyCopyright\(.*\)"),
                                             string.Format("AssemblyCopyright(\"{0}\")", Copyright));
            }
        }

        /// <summary>
        /// Modifies the version numbers to be within 16-bit boundaries.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <returns>The modified version number.</returns>
        private static string To16bitVersionNumbers(string version)
        {
            var vs = version.Split('.');
            var sb = new StringBuilder();
            foreach (var s in vs)
            {
                if (sb.Length > 0)
                {
                    sb.Append(".");
                }
                int i;
                if (int.TryParse(s, out i))
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
        public string FileVersion { get; set; }

        public Dictionary<Regex, string> NuSpecReplacements { get; set; }
        public Dictionary<Regex, string> AssemblyInfoReplacements { get; set; }

        public void ScanFolder(string path)
        {
            foreach (string file in Directory.GetFiles(path, "AssemblyInfo.cs"))
            {
                UpdateFile(file, AssemblyInfoReplacements);
            }

            foreach (string file in Directory.GetFiles(path, "*.nuspec"))
            {
                UpdateFile(file, NuSpecReplacements);
            }

            foreach (string dir in Directory.GetDirectories(path))
                ScanFolder(dir);
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

            using (var sw = new StreamWriter(file))
            {
                sw.Write(text);
            }

            Log(file);
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
