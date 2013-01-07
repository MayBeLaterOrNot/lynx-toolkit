// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Lynx">
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
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    public static class Utilities
    {
        /// <summary>
        ///     Creates the directory if missing.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void CreateDirectoryIfMissing(string path)
        {
            var outputdir = Path.GetFullPath(path);
            if (!Directory.Exists(outputdir))
            {
                Directory.CreateDirectory(outputdir);
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

        public static void Match(
            this Regex regex, string input, Action<string> notMatchAction, Action<Match> matchAction)
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
        ///     Opens the specified file for edit.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="scc">The source code control provider.</param>
        public static void OpenForEdit(string filename, string scc)
        {
            switch (scc)
            {
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
        ///     Returns the substring from index 0 to the first occurence of the specified string.
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
    }

    /// <summary>
    ///     Provides utility methods for the Lynx toolkit console applications.
    /// </summary>
    /// <remarks></remarks>
    public static class Application
    {
        /// <summary>
        ///     Gets the application comments.
        /// </summary>
        /// <remarks></remarks>
        public static string Comments
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).Comments;
            }
        }

        /// <summary>
        ///     Gets the application description.
        /// </summary>
        /// <remarks></remarks>
        public static string Description
        {
            get
            {
                return FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location).FileDescription;
            }
        }

        /// <summary>
        ///     Gets the application header (product name, version number and copyright notice).
        /// </summary>
        public static string Header
        {
            get
            {
                var sb = new StringBuilder();
                var fvi = FileVersionInfo.GetVersionInfo(typeof(Application).Assembly.Location);
                sb.AppendLine(fvi.ProductName);
                sb.AppendFormat(
                    "Version {0}.{1} (build {2})", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);
                sb.AppendLine();
                sb.AppendLine(fvi.LegalCopyright);
                return sb.ToString();
            }
        }
    }
}