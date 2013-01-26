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
//   The 'CleanSource' program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace CleanSource
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using LynxToolkit;

    /// <summary>
    /// The 'CleanSource' program.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The constructor summary substring to search for.
        /// </summary>
        private const string ConstructorSummarySubString = "/// Initializes a new instance of the";

        /// <summary>
        /// Expression to search for region/end region lines.
        /// </summary>
        private static readonly Regex RegionExpression = new Regex(@"^(\s*#(?:end)?region.*?)$", RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.Singleline);

        /// <summary>
        /// The number of files cleaned.
        /// </summary>
        private static int filesCleaned;

        /// <summary>
        /// The number of files scanned.
        /// </summary>
        private static int fileCount;

        /// <summary>
        /// Clean summaries.
        /// </summary>
        private static bool cleanSummary;

        /// <summary>
        /// Indents summaries.
        /// </summary>
        private static bool indentSummary;

        /// <summary>
        /// Remove regions.
        /// </summary>
        private static bool cleanRegions;

        /// <summary>
        /// The executable to check out files.
        /// </summary>
        private static string openForEditExecutable;

        /// <summary>
        /// The arguments to check out files.
        /// </summary>
        private static string openForEditArguments;

        /// <summary>
        /// The exclude list.
        /// </summary>
        private static string exclude;

        /// <summary>
        /// The main entry point of the program.
        /// </summary>
        /// <param name="args">The args.</param>
        private static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);

            exclude = "AssemblyInfo.cs Packages *.Designer.cs obj bin _*";

            foreach (var arg in args)
            {
                var argx = arg.Split('=');
                switch (argx[0].ToLower())
                {
                    case "/cleansummary":
                        cleanSummary = true;
                        continue;
                    case "/indentsummary":
                        indentSummary = true;
                        continue;
                    case "/cleanregions":
                        cleanRegions = true;
                        continue;
                    case "/scc":
                        if (string.Equals(argx[1], "p4", StringComparison.InvariantCultureIgnoreCase))
                        {
                            openForEditExecutable = "p4.exe";
                            openForEditArguments = "edit {0}";
                        }

                        continue;
                }

                Scan(arg);
            }

            Console.WriteLine("{0} files cleaned (of {1})", filesCleaned, fileCount);
        }

        /// <summary>
        /// Scans the specified directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        private static void Scan(string directory)
        {
            if (Utilities.IsExcluded(exclude, directory))
            {
                return;
            }

            Directory.GetDirectories(directory).ToList().ForEach(Scan);
            Directory.GetFiles(directory, "*.cs").ToList().ForEach(Clean);
        }

        /// <summary>
        /// Cleans the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        private static void Clean(string file)
        {
            if (Utilities.IsExcluded(exclude, file))
            {
                return;
            }

            fileCount++;

            var input = File.ReadAllText(file);
            var output = CleanCode(input);

            if (!string.Equals(input, output))
            {
                Console.WriteLine(file);
                if (openForEditExecutable != null)
                {
                    Utilities.OpenForEdit(file, openForEditExecutable, openForEditArguments);
                }

                try
                {
                    File.WriteAllText(file, output, Encoding.UTF8);
                    filesCleaned++;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
        }

        /// <summary>
        /// Cleans the code.
        /// </summary>
        /// <param name="text">The code.</param>
        /// <returns>The cleaned code.</returns>
        private static string CleanCode(string text)
        {
            // dependency properties
            text = Regex.Replace(text, @"(?<=/// <summary>\s*/// )(?<summary>.*?)(?=\s*/// </summary>\s*public static readonly DependencyProperty (?<name>.*?)Property)",
                "Identifies the <see cref=\"${name}\"/> dependency property.");

            // routed events
            text = Regex.Replace(text, @"(?<=/// <summary>\s*/// )(?<summary>.*?)(?=\s*/// </summary>\s*public static readonly RoutedEvent (?<name>.*?)Event)",
                "Identifies the <see cref=\"${name}\"/> routed event.");

            // regions
            if (cleanRegions)
            {
                text = Regex.Replace(text, @"([^\S\r\n]*#(?:end)?region[^\r\n]*)", string.Empty);
            }

            // empty remarks
            text = Regex.Replace(text, @"/// <remarks>\s*(/// )?</remarks>", string.Empty);

            // Whitespace at end of line
            text = Regex.Replace(text, @"[^\S\r\n]+(?=[\r\n$])", string.Empty);

            // Replace double new lines
            text = Regex.Replace(text, @"(\r?\n\r?){3,}", "\r\n\r\n");

            return text;
        }

        private static string CleanCodeOld(string text)
        {
            var input = text.Split('\n');
            var output = new StringBuilder();

            int end;

            // Remove blank lines at end of file
            // http://stylecop.soyuz5.com/SA1518.html
            for (end = input.Length - 1; end >= 0; end--)
            {
                if (!string.IsNullOrWhiteSpace(input[end]))
                {
                    break;
                }
            }

            int start;

            // remove blank lines at start of file
            // http://stylecop.soyuz5.com/SA1517.html
            for (start = 0; start < end; start++)
            {
                if (!string.IsNullOrWhiteSpace(input[start]))
                {
                    break;
                }

            }

            string previousLine = null;

            for (int i = start; i <= end; i++)
            {
                var thisline = input[i];
                var nextline = i + 1 < end ? input[i + 1] : null;

                if (cleanRegions && RegionExpression.Match(thisline).Success)
                {

                    // skip the following blank line
                    if (string.IsNullOrWhiteSpace(nextline))
                    {
                        i++;
                    }

                    continue;
                }

                // Remove duplicate lines containing "/// Initializes a new instance of the"
                if (cleanSummary && previousLine != null && previousLine.Contains(ConstructorSummarySubString)
                    && thisline.Contains(ConstructorSummarySubString))
                {
                    continue;
                }

                // remove double blank lines
                // http://stylecop.soyuz5.com/SA1507.html
                if (string.IsNullOrWhiteSpace(thisline) && string.IsNullOrWhiteSpace(previousLine))
                {
                    continue;
                }

                // remove empty remarks comments
                if (nextline != null && Regex.IsMatch(thisline, @"^\s*///\s<remarks>\s*$")
                    && Regex.IsMatch(nextline, @"^\s*///\s</remarks>\s*$"))
                {
                    i++;
                    continue;
                }

                if (Regex.IsMatch(thisline, @"^\s*///\s*<remarks>\s*</remarks>\s*$"))
                {
                    continue;
                }

                // trim whitespace
                var trimmed1 = Regex.Replace(thisline, @"///\s+(?=[^<])", indentSummary ? "///     " : "/// ");
                if (!string.Equals(trimmed1, thisline))
                {
                }

                // trim the end
                var trimmed = trimmed1.TrimEnd();
                if (!string.Equals(trimmed, thisline))
                {
                }

                if (output.Length > 0)
                {
                    output.AppendLine();
                }

                output.Append(trimmed);
                previousLine = trimmed;
            }

            return output.ToString();
        }
    }
}