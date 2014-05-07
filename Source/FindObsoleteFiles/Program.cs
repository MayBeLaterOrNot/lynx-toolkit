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
// --------------------------------------------------------------------------------------------------------------------

namespace FindObsoleteFiles
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;

    /// <summary>
    /// Finds all *.cs files that are not included in the project(s) of the solutions(s).
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Utilities.ApplicationHeader);

            foreach (var arg in args)
            {
                FindObsoleteFiles(arg);
            }
        }

        /// <summary>
        /// Gets the projects in the specified solution.
        /// </summary>
        /// <param name="solutionFileName">The file name of the solution.</param>
        /// <returns>A sequence of project file names.</returns>
        public static IEnumerable<string> GetProjects(string solutionFileName)
        {
            var content = File.ReadAllText(solutionFileName);
            var regex = new Regex("^Project\\(\".*\"\\) = \"(.*?)\", \"(.*?)\"", RegexOptions.Multiline);
            return regex.Matches(content).Cast<Match>().Select(m => m.Groups[2].Value);
        }

        /// <summary>
        /// Finds the obsolete files in the specified solution.
        /// </summary>
        /// <param name="solutionFileName">The file name of the solution.</param>
        public static void FindObsoleteFiles(string solutionFileName)
        {
            var files = new List<string>();
            var directory = Path.GetDirectoryName(solutionFileName) ?? string.Empty;
            var searchPattern = Path.GetFileName(solutionFileName);
            if (string.IsNullOrEmpty(searchPattern))
            {
                searchPattern = "*.sln";
            }

            foreach (var solution in Directory.GetFiles(directory, searchPattern))
            {
                foreach (var proj in GetProjects(solution))
                {
                    if (!proj.EndsWith(".csproj"))
                    {
                        continue;
                    }

                    var projPath = Path.Combine(directory, proj);
                    files.AddRange(GetFiles(projPath));
                }
            }

            foreach (var f in AllFiles(directory).Where(f => !files.Contains(f)))
            {
                Console.WriteLine(f);
            }
        }

        /// <summary>
        /// Gets all files under the specified directory.
        /// </summary>
        /// <param name="directory">
        /// The directory.
        /// </param>
        /// <returns>
        /// A sequence of file names.
        /// </returns>
        private static IEnumerable<string> AllFiles(string directory)
        {
            var q = new Queue<string>();
            q.Enqueue(directory);
            while (q.Count > 0)
            {
                directory = q.Dequeue();
                if (IsExcluded(directory))
                {
                    continue;
                }

                foreach (var f in Directory.GetFiles(directory, "*.cs"))
                {
                    yield return f;
                }

                foreach (var d in Directory.GetDirectories(directory))
                {
                    q.Enqueue(d);
                }
            }
        }

        /// <summary>
        /// Determines whether the specified file name is excluded.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns><c>true</c> if the file is excluded; otherwise <c>false</c>.</returns>
        private static bool IsExcluded(string fileName)
        {
            var name = Path.GetFileName(fileName) ?? string.Empty;
            if (name.StartsWith("bin"))
            {
                return true;
            }

            if (name.StartsWith("obj"))
            {
                return true;
            }

            if (name.StartsWith("packages"))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the files in the specified project.
        /// </summary>
        /// <param name="projectFileName">File name of the project.</param>
        /// <returns>A sequence of file names.</returns>
        private static IEnumerable<string> GetFiles(string projectFileName)
        {
            var directory = Path.GetDirectoryName(projectFileName) ?? string.Empty;
            var doc = new XmlDocument();
            doc.Load(projectFileName);
            var root = doc.DocumentElement;
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("b", "http://schemas.microsoft.com/developer/msbuild/2003");

            if (root == null)
            {
                yield break;
            }

            var compileNodes = root.SelectNodes("//b:Compile", nsmgr);
            var noneNodes = root.SelectNodes("//b:None", nsmgr);
            if (compileNodes == null || noneNodes == null)
            {
                yield break;
            }

            var nodes = compileNodes.Cast<XmlNode>().Concat(noneNodes.Cast<XmlNode>());

            foreach (var node in nodes)
            {
                if (node == null || node.Attributes == null)
                {
                    continue;
                }

                var includeAttribute = node.Attributes["Include"];
                if (includeAttribute != null)
                {
                    yield return Path.GetFullPath(Path.Combine(directory, includeAttribute.InnerText));
                }
            }
        }
    }
}