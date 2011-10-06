//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Text;

namespace FileHeaderUpdater
{
    using System.IO;

    class Program
    {
        static void Main(string[] args)
        {
            string company = null;
            string copyright = null;
            string exclude = "AssemblyInfo.cs";
            string directory = Directory.GetCurrentDirectory();

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] kv = arg.Split('=');
                switch (kv[0])
                {
                    case "/Directory":
                        directory = kv[1];
                        break;
                    case "/Exclude":
                        exclude = kv[1];
                        break;
                    case "/Company":
                        company = kv[1];
                        break;
                    case "/Copyright":
                        copyright = kv[1];
                        break;
                }
            }
            if (copyright == null && company != null) copyright = string.Format("Copyright © {0}. All rights reserved.", company);

            var updater = new Updater(copyright, company, exclude);

            updater.ScanFolder(directory);
        }
    }

    public class Updater
    {
        public Updater(string copyright, string company, string exclude)
        {
            Copyright = copyright;
            Company = company;
            Exclude = exclude;
        }

        public string Copyright { get; set; }
        public string Company { get; set; }
        public string Exclude { get; set; }

        public void ScanFolder(string path)
        {
            foreach (string file in Directory.GetFiles(path, "*.cs"))
            {
                if (!IsExcluded(file))
                    UpdateFile(file);
            }

            foreach (string dir in Directory.GetDirectories(path))
                if (!IsExcluded(dir))
                    ScanFolder(dir);
        }

        private bool IsExcluded(string path)
        {
            var name = Path.GetFileName(path);
            if (Exclude != null && Exclude.Contains(name))
                return true;
            return false;
        }

        private void UpdateFile(string file)
        {
            var fileName = Path.GetFileName(file);
            var sb = new StringBuilder();
            using (var r = new StreamReader(file))
            {
                var header =
                    String.Format(
                        @"//-----------------------------------------------------------------------
// <copyright file=""{0}"" company=""{1}"">
//     {2}
// </copyright>
//-----------------------------------------------------------------------
",
                        fileName,
                        Company,
                        Copyright);
                sb.AppendLine(header);
                bool isHeader = true;
                while (!r.EndOfStream)
                {
                    var line = r.ReadLine();
                    if (!string.IsNullOrWhiteSpace(line) && !line.StartsWith("//")) isHeader = false;
                    if (!isHeader) sb.AppendLine(line);
                }
            }
            using (var w = new StreamWriter(file))
            {
                w.Write(sb.ToString());
            }
            Log(file);
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
