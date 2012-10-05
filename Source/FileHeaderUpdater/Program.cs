//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//     Copyright © Lynx Toolkit.
// </copyright>
//-----------------------------------------------------------------------

namespace FileHeaderUpdater
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;

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

        private static bool replaceSymbol;

        private static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);

            string company = null;
            string copyright = null;
            string exclude = "AssemblyInfo.cs Packages .Designer.cs obj bin";
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

                Copyright = copyright;
                Company = company;
                Exclude = exclude;
            }

            public string Copyright { get; set; }

            public string Company { get; set; }

            public string Exclude { get; set; }

            public void ScanFolder(string path)
            {
                Log(path);
                foreach (string file in Directory.GetFiles(path, "*.cs"))
                {
                    if (!IsExcluded(file))
                    {
                        Log("  " + Path.GetFileName(file));
                        UpdateFile(file);
                    }
                    else
                    {
                        Log("  " + Path.GetFileName(file) + " excluded.");
                    }
                }

                foreach (string dir in Directory.GetDirectories(path)) if (!IsExcluded(dir)) ScanFolder(dir);
            }

            private bool IsExcluded(string path)
            {
                var name = Path.GetFileName(path);
                foreach (var item in Exclude.Split(' '))
                {
                    if (string.IsNullOrWhiteSpace(item)) continue;
                    if (name.ToLower().Contains(item.ToLower())) return true;
                }
                return false;
            }

            private void UpdateFile(string file)
            {
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
                using (var r = new StreamReader(file))
                {
                    sb.AppendLine(
                        "// --------------------------------------------------------------------------------------------------------------------");
                    sb.AppendFormat("// <copyright file=\"{0}\" company=\"{1}\">", fileName, this.Company);
                    sb.AppendLine();
                    sb.AppendFormat("//   {0}", this.Copyright);
                    sb.AppendLine();
                    sb.AppendLine("// </copyright>");
                    if (!string.IsNullOrWhiteSpace(summary))
                    {
                        sb.AppendLine("// <summary>");
                        sb.AppendLine(summary);
                        sb.AppendLine("// </summary>");
                    }
                    sb.AppendLine(
                        "// --------------------------------------------------------------------------------------------------------------------");
                    sb.AppendLine();
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

                var output = sb.ToString().Trim();
                var original = File.ReadAllText(file);
                if (string.Equals(original, output))
                {
                    return;
                }

                OpenForEdit(file, openForEditExecutable, openForEditArguments);
                File.WriteAllText(file, output, Encoding.UTF8);
            }

            /// <summary>
            /// Opens the specified file for edit.
            /// </summary>
            /// <param name="filename">The filename.</param>
            /// <param name="exe">The executable.</param>
            /// <param name="argumentFormatString">The argument format string.</param>
            private static void OpenForEdit(string filename, string exe, string argumentFormatString)
            {
                if (exe == null)
                {
                    return;
                }

                var psi = new ProcessStartInfo(exe, string.Format(argumentFormatString, filename)) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
                var p = Process.Start(psi);
                p.WaitForExit();
            }

            private void Log(string msg)
            {
                Console.WriteLine(msg);
            }
        }
    }
}
