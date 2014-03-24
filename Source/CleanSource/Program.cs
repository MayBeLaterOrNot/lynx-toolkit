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
    using System.Collections.Generic;
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
        /// The number of files cleaned.
        /// </summary>
        private static int filesCleaned;

        /// <summary>
        /// The number of files scanned.
        /// </summary>
        private static int fileCount;

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
        public static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);

            exclude = "AssemblyInfo.cs Packages *.Designer.cs obj bin _*";

            foreach (var arg in args)
            {
                var argx = arg.Split('=');
                switch (argx[0].ToLower())
                {
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

            Console.WriteLine("{0} files modified.", filesCleaned);
            Console.WriteLine("{0} files scanned.", fileCount);
#if DEBUG
            Console.ReadKey();
#endif
        }

        /// <summary>
        /// Scans the specified directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        public static void Scan(string directory)
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
        public static void Clean(string file)
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
        /// Splits the specified code to lines.
        /// </summary>
        /// <param name="text">The code.</param>
        /// <returns>A sequence of strings.</returns>
        public static IEnumerable<string> ToLines(this string text)
        {
            var newLineSeparators = new[] { "\r\n", "\n", "\r" };
            return text.Split(newLineSeparators, StringSplitOptions.None);
        }

        /// <summary>
        /// Formats the specified lines to a <see cref="string" />.
        /// </summary>
        /// <param name="lines">The lines to format.</param>
        /// <returns>A <see cref="string" />.</returns>
        public static string ToText(this IEnumerable<string> lines)
        {
            var sb = new StringBuilder();
            foreach (var line in lines)
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }

                sb.Append(line);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Cleans the code.
        /// </summary>
        /// <param name="code">The code to clean.</param>
        /// <returns>The cleaned code.</returns>
        public static string CleanCode(string code)
        {
            return code
                .ToLines()
                .TrimLineEnds()
                .ReplaceTabs()
                .RemoveDoubleNewLines()
                .RemoveRegions()
                .CleanComments()
                .ToText()
                .Trim();
        }

        public static IEnumerable<string> TrimLineEnds(this IEnumerable<string> lines)
        {
            return lines.Select(l => l.StartsWith("// ") ? l : l.TrimEnd());
        }

        public static IEnumerable<string> ReplaceTabs(this IEnumerable<string> lines)
        {
            return lines.Select(l => l.Replace("\t", "    "));
        }

        public static IEnumerable<string> RemoveDoubleNewLines(this IEnumerable<string> lines)
        {
            // TODO: do not change inside strings...

            bool previousLineWasEmpty = false;
            foreach (var line in lines)
            {
                var thisLineIsEmpty = line.Length == 0;
                if (previousLineWasEmpty && thisLineIsEmpty)
                {
                    continue;
                }

                previousLineWasEmpty = thisLineIsEmpty;

                yield return line;
            }
        }

        public static IEnumerable<string> RemoveRegions(this IEnumerable<string> lines)
        {
            return lines.Where(l => !Regex.Match(l, @"\w*#(?:end)?region").Success);
        }

        public static IEnumerable<string> CleanComments(this IEnumerable<string> lines)
        {
            var commentExpression = new Regex(@"\s+///\s", RegexOptions.Compiled);
            var comments = new StringBuilder();
            string prefix = null;
            foreach (var line in lines)
            {
                var match = commentExpression.Match(line);
                if (match.Success)
                {
                    prefix = line.Substring(0, match.Length);
                    comments.AppendLine(line.Substring(match.Length).Trim());
                }
                else
                {
                    if (prefix != null)
                    {
                        var documentationComments = new DocumentationComments(comments.ToString(), line);
                        foreach (var c in documentationComments.ToString().ToLines())
                        {
                            yield return prefix + c;
                        }

                        comments.Clear();
                        prefix = null;
                    }

                    yield return line;
                }
            }
        }
    }
}