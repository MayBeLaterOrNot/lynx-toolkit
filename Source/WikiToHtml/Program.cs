//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace WikiToHtml
{
    using System;
    using System.Linq;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    static class Program
    {
        private static string wikiSyntax;
        private static string templatePath;
        private static string searchPattern;
        private static string unlockProgram;
        private static string unlockArguments;
        private static CreoleWikiEngine.WikiToHtmlConverter.Options options;

        private static void Main(string[] args)
        {
            Console.WriteLine("Wiki to Html converter");
            Console.WriteLine("version " + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine();

            if (args.Length == 0)
            {
                Console.WriteLine(
                    "Arguments: [-Syntax=creole] [-LocalLinks=formatString] [-UnlockProgram=program] [-UnlockArguments=args] [-LocalLinks=formatstring] [Template] [searchPattern] [InputFolder]");
                Console.WriteLine(
                    "Example: wiki2html.exe -LocalLinks={0}/{1}.html -UnlockProgram=p4.exe \"-UnlockArguments=edit {0}\" Template.html *.wiki ..\\Help");
                Console.WriteLine("  The 'unlock' program will be executed for each output file. {0} is replaced by the output file name.");
                Console.WriteLine("  The template should contain \"$content\" where the wiki content should be placed.");
                Console.WriteLine("  Local links like [[space:link]] will be converted to \"href=space/link.html\".");
                return;
            }

            options = new CreoleWikiEngine.WikiToHtmlConverter.Options();

            wikiSyntax = "creole";
            templatePath = "Template.html";
            searchPattern = "*.txt";
            // unlockProgram = "p4.exe";
            // unlockArguments = "edit {0}";

            foreach (var arg in args)
            {
                if (arg.Contains('*'))
                {
                    searchPattern = arg;
                    continue;
                }

                if (arg.StartsWith("-Syntax"))
                {
                    var f = arg.Split('=');
                    wikiSyntax = f[1];
                    continue;
                }

                if (arg.StartsWith("-LocalLinks"))
                {
                    var f = arg.Split('=');
                    options.LocalLinkFunction = (space, link) =>
                        {
                            var href = string.Format(f[1], string.IsNullOrEmpty(space) ? "~" : space, link);
                            return href.Replace("~/", "");
                        };
                    continue;
                }

                if (arg.StartsWith("-UnlockProgram"))
                {
                    var f = arg.Split('=');
                    unlockProgram = f[1];
                    continue;
                }

                if (arg.StartsWith("-UnlockArguments"))
                {
                    var f = arg.Split('=');
                    unlockArguments = f[1];
                    continue;
                }

                if (File.Exists(arg))
                {
                    var ext = Path.GetExtension(arg);
                    if (string.Equals(ext, ".html", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // argument is a template
                        templatePath = arg;
                        continue;
                    }

                    Convert(arg);
                    continue;
                }

                if (Directory.Exists(arg))
                {
                    SearchDirectory(arg);
                }
            }

            if (args.Length == 0)
            {
                SearchDirectory(".");
            }
        }

        private static bool directoryPrinted;

        private static void SearchDirectory(string path)
        {
            directoryPrinted = false;
            foreach (var file in Directory.GetFiles(path, searchPattern))
            {
                Convert(file);
            }

            foreach (var dir in Directory.GetDirectories(path))
            {
                SearchDirectory(dir);
            }
        }

        private static void UnlockFile(string filename)
        {
            if (unlockProgram == null)
            {
                return;
            }
            var args = unlockArguments.Replace("{0}", filename);
            var psi = new ProcessStartInfo(unlockProgram, args) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        private static void Convert(string inputPath)
        {
            var ext = Path.GetExtension(inputPath);
            var file = Path.GetFileNameWithoutExtension(inputPath);
            string outputPath = Path.ChangeExtension(inputPath, ".html");
            string input = File.ReadAllText(inputPath);

            string title = null;
            string keywords = string.Empty;
            string description = string.Empty;

            // the first header is the default title
            var headers = Regex.Match(input, @"^=+\s*(.*)$", RegexOptions.Multiline);
            if (headers.Success)
            {
                title = headers.Groups[1].Value.Trim();
            }

            input = Regex.Replace(
                input,
                "^@Title=(.*)$",
                titleMatch =>
                {
                    title = titleMatch.Groups[1].Value.Trim();
                    return string.Empty;
                },
                RegexOptions.Multiline);

            input = Regex.Replace(
                input,
                "^@Keywords=(.*)$",
                titleMatch =>
                {
                    keywords = titleMatch.Groups[1].Value.Trim();
                    return string.Empty;
                },
                RegexOptions.Multiline);

            input = Regex.Replace(
                input,
                "^@Description=(.*)$",
                titleMatch =>
                {
                    description = titleMatch.Groups[1].Value.Trim();
                    return string.Empty;
                },
                RegexOptions.Multiline);

            string body;

            switch (wikiSyntax.ToLower())
            {
                case "markdown":
                    throw new NotImplementedException();
                case "creole":
                case "creole10":
                    var engine = new CreoleWikiEngine.WikiToHtmlConverter(options);
                    body = engine.Transform(input);
                    break;
                default:
                    throw new InvalidOperationException("Invalid wiki syntax.");
            }

            string templateContent = File.ReadAllText(templatePath);
            var html = Regex.Replace(templateContent, @"\$title", title ?? string.Empty);
            html = Regex.Replace(html, @"\$keywords", keywords ?? string.Empty);
            html = Regex.Replace(html, @"\$description", description ?? string.Empty);
            html = Regex.Replace(html, @"\$content", body);

            if (File.Exists(outputPath))
            {
                var existingHtml = File.ReadAllText(outputPath);
                if (existingHtml == html)
                {
                    // Process.Start(outputPath);
                    // no change
                    return;
                }
            }

            if (!directoryPrinted)
            {
                Console.WriteLine(Path.GetDirectoryName(inputPath));
                directoryPrinted = true;
            }
            Console.WriteLine("  " + file);
            UnlockFile(outputPath);
            using (var w = new StreamWriter(outputPath, false, Encoding.UTF8))
            {
                w.Write(html);
            }

            // Process.Start(outputPath);
        }
    }
}
