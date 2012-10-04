//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//     Copyright © Lynx Toolkit.
// </copyright>
//-----------------------------------------------------------------------

namespace FileHeaderUpdater
{
    using System;
    using System.IO;
    using System.Text;

    class Program
    {
        static void Main(string[] args)
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
            if (copyright == null && company != null) copyright = string.Format("Copyright © {0}.", company);

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
                    Log("  "+ Path.GetFileName(file) + " excluded.");
                }
            }

            foreach (string dir in Directory.GetDirectories(path))
                if (!IsExcluded(dir))
                    ScanFolder(dir);
        }

        private bool IsExcluded(string path)
        {
            var name = Path.GetFileName(path);
            foreach (var item in Exclude.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(item)) continue;
                if (name.ToLower().Contains(item.ToLower()))
                    return true;
            }
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
                        @"// --------------------------------------------------------------------------------------------------------------------
// <copyright file=""{0}"" company=""{1}"">
//   {2}
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
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
                w.Write(sb.ToString().Trim());
            }
        }

        private void Log(string msg)
        {
            Console.WriteLine(msg);
        }
    }
}
