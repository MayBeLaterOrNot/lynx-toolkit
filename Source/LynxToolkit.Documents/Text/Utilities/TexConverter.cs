namespace LynxToolkit.Documents
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Text;

    public class TexConverter
    {
        public TexConverter()
        {
            this.LatexInstallationDirectory = @"C:\Program Files (x86)\MiKTeX 2.9\miktex\bin";
            this.TemporaryDirectory = "~temp";
            this.MagnificationRatio = 1440;
            this.Background = "transparent";
            this.Tight = true;
            this.Page = 1;
        }

        public string LatexInstallationDirectory { get; set; }

        public bool CleanTemporaryFiles { get; set; }

        public string TemporaryDirectory { get; set; }

        /// <summary>
        /// Gets or sets the magnification ratio.
        /// </summary>
        /// <value>The magnification ratio.</value>
        /// <remarks>
        /// Set the x magnification ratio to num/1000. Overrides the magnification specified in the DVI file. 
        /// Must be between 10 and 100000. It is recommended that you use standard magstep values (1095, 1200, 
        /// 1440, 1728, 2074, 2488, 2986, and so on) to help reduce the total number of PK files generated. 
        /// num may be a real number, not an integer, for increased precision.
        /// </remarks>
        public int MagnificationRatio { get; set; }

        public int Page { get; set; }

        public string Background { get; set; }

        public bool Tight { get; set; }

        public bool Convert(string tex, string fileName)
        {
            var tempTexFileName = Path.Combine(this.TemporaryDirectory, Path.ChangeExtension("~" + Path.GetFileName(fileName), ".tex"));
            if (!Directory.Exists(this.TemporaryDirectory))
            {
                Directory.CreateDirectory(this.TemporaryDirectory);
            }

            File.WriteAllText(tempTexFileName, CreateTexDocument(tex));

            var latexStartInfo = new ProcessStartInfo
                                     {
                                         CreateNoWindow = true,
                                         UseShellExecute = false,
                                         FileName = Path.Combine(this.LatexInstallationDirectory, "latex.exe"),
                                         Arguments =
                                             string.Format(
                                                 "-halt-on-error -output-directory=\"{0}\" \"{1}\"",
                                                 this.TemporaryDirectory,
                                                 tempTexFileName)
                                     };
            var latexProcess = Process.Start(latexStartInfo);
            latexProcess.WaitForExit();
            if (latexProcess.ExitCode != 0)
            {
                return false;
            }

            var tempDviFileName = Path.ChangeExtension(tempTexFileName, ".dvi");
            var options = this.Tight ? "-T tight" : string.Empty;

            var ext = (Path.GetExtension(fileName) ?? string.Empty).ToLower();
            if (ext == ".png")
            {
                var pngStartInfo = new ProcessStartInfo
                                       {
                                           CreateNoWindow = true,
                                           UseShellExecute = false,
                                           FileName =
                                               Path.Combine(
                                                   this.LatexInstallationDirectory, "dvipng.exe"),
                                           Arguments =
                                               string.Format(
                                                   "-q -x {0} -p {1} --height --depth {2} -bg {3} --png -z 9 -o {4} \"{5}\"",
                                                   this.MagnificationRatio,
                                                   this.Page,
                                                   options,
                                                   this.Background,
                                                   fileName,
                                                   tempDviFileName)
                                       };
                var pngProcess = Process.Start(pngStartInfo);
                pngProcess.WaitForExit();
                if (pngProcess.ExitCode != 0)
                {
                    return false;
                }
            }

            if (ext == ".pdf")
            {
                var pdfStartInfo = new ProcessStartInfo
                                       {
                                           CreateNoWindow = true,
                                           UseShellExecute = false,
                                           FileName =
                                               Path.Combine(
                                                   this.LatexInstallationDirectory, "dvipdfm.exe"),
                                           Arguments =
                                               string.Format(
                                                   "-ecz 9 -o {0} \"{1}\"",
                                                   Path.ChangeExtension(fileName, ".pdf"),
                                                   tempDviFileName)
                                       };
                var pdfProcess = Process.Start(pdfStartInfo);
                pdfProcess.WaitForExit();
                if (pdfProcess.ExitCode != 0)
                {
                    return false;
                }
            }

            if (this.CleanTemporaryFiles)
            {
                try
                {
                    File.Delete(tempTexFileName);
                    File.Delete(Path.ChangeExtension(tempTexFileName, ".dvi"));
                    File.Delete(Path.ChangeExtension(tempTexFileName, ".aux"));
                    File.Delete(Path.ChangeExtension(tempTexFileName, ".log"));
                    Directory.Delete(this.TemporaryDirectory);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return true;
        }

        private static string CreateTexDocument(string content)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"\documentclass{article}");
            sb.AppendLine(@"\usepackage[paperwidth=\maxdimen,paperheight=\maxdimen]{geometry}");
            sb.AppendLine(@"\usepackage[active,displaymath,textmath,sections,graphics,floats]{preview}");
            sb.AppendLine(@"\usepackage{amssymb}");
            sb.AppendLine(@"\usepackage[utf8]{inputenc}");
            sb.AppendLine(@"\usepackage{lmodern}");
            sb.AppendLine(@"\pagestyle{empty}");
            sb.AppendLine(@"\begin{document}");
            sb.AppendLine(@"\begin{samepage}");
            sb.AppendLine(content);
            sb.AppendLine(@"\end{samepage}");
            sb.AppendLine(@"\end{document}");
            return sb.ToString();
        }
    }
}