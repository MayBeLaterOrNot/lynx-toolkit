// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Lynx Toolkit">
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
//   Provides static utility methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides static utility methods.
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        ///     Gets the application comments.
        /// </summary>
        public static string ApplicationComments
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).Comments;
            }
        }

        /// <summary>
        ///     Gets the application description.
        /// </summary>
        public static string ApplicationDescription
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileDescription;
            }
        }

        /// <summary>
        ///     Gets the application header (product name, version number and copyright notice).
        /// </summary>
        public static string ApplicationHeader
        {
            get
            {
                var sb = new StringBuilder();
                var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                sb.AppendLine(fvi.ProductName);
                sb.AppendFormat(
                    "Version {0}.{1} (build {2})", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);
                sb.AppendLine();
                sb.AppendLine(fvi.LegalCopyright);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Writes the application header to the <see cref="Console"/>.
        /// </summary>
        /// <param name="windowWidth">The width of the console window.</param>
        public static void WriteHeader(int windowWidth = 120)
        {
            var fvi = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
            Console.Title = fvi.ProductName;
            Console.WindowWidth = windowWidth;
            Console.WriteLine(fvi.ProductName);
            Console.WriteLine("Version {0}.{1} (build {2})", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);
            Console.WriteLine();
            Console.WriteLine(fvi.LegalCopyright);
            Console.WriteLine();
        }

        /// <summary>
        /// Finds the files in the specified directory and its subdirectories.
        /// </summary>
        /// <param name="directory">The directory.</param>
        /// <param name="searchPattern">The search pattern.</param>
        /// <returns>A sequence of file paths.</returns>
        public static IEnumerable<string> FindFiles(string directory, string searchPattern)
        {
            foreach (var file in Directory.GetFiles(directory, searchPattern))
            {
                yield return file;
            }

            foreach (var file in Directory.GetDirectories(directory).SelectMany(d => FindFiles(d, searchPattern)))
            {
                yield return file;
            }
        }

        /// <summary>
        ///     Creates the directory if missing.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void CreateDirectoryIfMissing(string path)
        {
            path = Path.GetFullPath(path);
            var current = string.Empty;
            foreach (var d in path.Split('\\'))
            {
                current = Path.Combine(current, d);
                if (current.EndsWith(":"))
                {
                    current += "\\";
                }

                if (!Directory.Exists(current))
                {
                    Directory.CreateDirectory(current);
                }
            }
        }

        /// <summary>
        ///     Formats the list of items to a string with the specified separator.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The System.String.</returns>
        public static string FormatList(this IEnumerable<object> items, string separator)
        {
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }

                sb.Append(item);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the specified file is modified.
        /// </summary>
        /// <param name="filePath">The file path.</param>
        /// <param name="newContent">The new content.</param>
        /// <returns>
        ///   <c>true</c> if the file is modified; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsFileModified(string filePath, string newContent)
        {
            if (!File.Exists(filePath))
            {
                return true;
            }

            var content = File.ReadAllText(filePath);
            return !string.Equals(content, newContent);
        }

        /// <summary>
        ///     Determines whether the specified file should be excluded.
        /// </summary>
        /// <param name="excludedItems">The excluded items.</param>
        /// <param name="path">The path.</param>
        /// <returns>
        ///     <c>true</c> if the specified excluded items is excluded; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsExcluded(string excludedItems, string path)
        {
            var name = Path.GetFileName(path);
            if (name == null)
            {
                return true;
            }

            foreach (var item in excludedItems.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                if (item.StartsWith("*"))
                {
                    var pattern = item.TrimStart('*');
                    if (name.EndsWith(pattern))
                    {
                        return true;
                    }
                }

                if (item.EndsWith("*"))
                {
                    var pattern = item.TrimEnd('*');
                    if (name.StartsWith(pattern))
                    {
                        return true;
                    }
                }

                if (string.Equals(name, item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Matches the specified regular expression.
        /// </summary>
        /// <param name="regex">The regex.</param>
        /// <param name="input">The input.</param>
        /// <param name="notMatchAction">The not match action.</param>
        /// <param name="matchAction">The match action.</param>
        public static void Match(this Regex regex, string input, Action<string> notMatchAction, Action<Match> matchAction)
        {
            int index = 0;

            foreach (Match match in regex.Matches(input))
            {
                if (match.Index > index)
                {
                    var s = input.Substring(index, match.Index - index);
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        notMatchAction(s);
                    }
                }

                index = match.Index + match.Length;
                matchAction(match);
            }

            if (index < input.Length)
            {
                var s = input.Substring(index);
                if (!string.IsNullOrWhiteSpace(s))
                {
                    notMatchAction(s);
                }
            }
        }

        /// <summary>
        /// Set the output string if the match group with the specified key is a success.
        /// </summary>
        /// <param name="m">The match.</param>
        /// <param name="key">The key.</param>
        /// <param name="output">The output.</param>
        /// <returns><c>true</c> if the value was set, <c>false</c> otherwise</returns>
        public static bool SetIfSuccess(this Match m, string key, ref string output)
        {
            if (m.Groups[key].Success)
            {
                output = m.Groups[key].Value;
                return true;
            }

            return false;
        }

        /// <summary>
        ///     Opens the specified file for edit.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="scc">The source code control provider.</param>
        public static void OpenForEdit(string filename, string scc)
        {
            switch (scc)
            {
                case "P4":
                case "p4":
                    OpenForEdit(filename, "p4.exe", "edit {0}");
                    break;
                default:
                    return;
            }
        }

        /// <summary>
        ///     Opens the specified file for edit.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="exe">The executable.</param>
        /// <param name="argumentFormatString">The argument format string.</param>
        public static void OpenForEdit(string filename, string exe, string argumentFormatString)
        {
            if (exe == null)
            {
                return;
            }

            if (!File.Exists(filename))
            {
                return;
            }

            var psi = new ProcessStartInfo(exe, string.Format(argumentFormatString, filename))
                          {
                              CreateNoWindow = true,
                              WindowStyle =
                                  ProcessWindowStyle
                                  .Hidden
                          };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        /// <summary>
        ///     Returns the substring from index 0 to the first occurrence of the specified string.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="value">The string to search for.</param>
        /// <param name="comparisonType">Type of the comparison.</param>
        /// <returns>The sub string.</returns>
        public static string SubstringTo(
            this string input, string value, StringComparison comparisonType = StringComparison.Ordinal)
        {
            int i = input.IndexOf(value, comparisonType);
            if (i == 0)
            {
                return string.Empty;
            }

            if (i > 0)
            {
                return input.Substring(0, i);
            }

            return input;
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>See <a href="http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path">Stack overflow</a></remarks>
        public static String MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}