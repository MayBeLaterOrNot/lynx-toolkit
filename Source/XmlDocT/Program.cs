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
//   Utility to generate "MSDN-style" documentation. Output to html or wiki format.
//   - Reads XML comments from the XML documentation files
//   - Reflects the assemblies to find inheritance hierarchy and syntax
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using LynxToolkit;

    /// <summary>
    ///     Utility to generate "MSDN-style" documentation. Output to html or wiki format.
    ///     - Reads XML comments from the XML documentation files
    ///     - Reflects the assemblies to find inheritance hierarchy and syntax
    /// </summary>
    public class Program
    {
        /// <summary>
        ///     The main program.
        /// </summary>
        /// <param name="args">
        ///     The command line arguments.
        /// </param>
        public static void Main(string[] args)
        {
            var w = Stopwatch.StartNew();
            Console.WriteLine(Utilities.ApplicationHeader);

            var doc = new DocFormatter();
            var input = new List<string>();

            foreach (var arg in args)
            {
                var kv = arg.Split('=');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "/singlepage":
                            doc.SinglePage = true;
                            break;
                        case "/creatememberpages":
                            doc.CreateMemberPages = true;
                            break;
                        case "/indexpage":
                            doc.IndexPage = kv[1];
                            break;
                        case "/indextitle":
                            doc.IndexTitle = kv[1];
                            break;
                        case "/helpcontents":
                            doc.HtmlHelpContents = kv[1];
                            break;
                        case "/input":
                            input.Add(kv[1]);
                            continue;
                        case "/output":
                            doc.Output = kv[1];
                            continue;
                        case "/stylesheet":
                            doc.StyleSheet = kv[1];
                            continue;
                        case "/toplevel":
                            doc.TopLevel = int.Parse(kv[1]);
                            continue;
                        case "/template":
                            doc.Template = kv[1];
                            continue;
                        case "/format":
                            doc.Format = kv[1].ToLower();
                            continue;
                        case "/extension":
                            doc.OutputExtension = kv[1].ToLower();
                            continue;
                        case "/ignore":
                            doc.Model.IgnoreAttributes.Add(kv[1]);
                            continue;
                        default:
                            doc.Replacements.Add(kv[0].Trim('/'), kv[1]);
                            continue;
                    }
                }

                input.Add(arg);
            }

            // Process input data
            foreach (var path in input)
            {
                if (path.Contains('*'))
                {
                    var dir = Path.GetDirectoryName(path);
                    if (string.IsNullOrWhiteSpace(dir))
                    {
                        dir = ".";
                    }

                    var pattern = Path.GetFileName(path);
                    Console.WriteLine("Processing " + path);
                    foreach (var fileName in Directory.GetFiles(dir, pattern))
                    {
                        doc.Model.Add(fileName);
                    }

                    continue;
                }

                if (File.Exists(path))
                {
                    Console.WriteLine("Processing " + path);
                    doc.Model.Add(path);
                    continue;
                }

                if (Directory.Exists(path))
                {
                    Console.WriteLine("Processing " + path + @"\\*.dll");
                    foreach (var fileName in Directory.GetFiles(path, "*.dll"))
                    {
                        doc.Model.Add(fileName);
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Output");

            var outputDirectory = doc.SinglePage ? Path.GetDirectoryName(doc.Output) : doc.Output;
            if (outputDirectory == null)
            {
                outputDirectory = ".";
            }

            if (!Directory.Exists(outputDirectory))
            {
                Console.WriteLine("  Creating output directory: {0}", Path.GetFullPath(outputDirectory));
                Directory.CreateDirectory(outputDirectory);
            }
            else
            {
                Console.WriteLine("  Output directory: {0}", Path.GetFullPath(outputDirectory));
            }

            if (doc.SinglePage)
            {
                Console.WriteLine("  Output file: {0}", Path.GetFullPath(doc.Output));
            }

            Console.WriteLine("  Output format: {0}", doc.Format);
            if (doc.SinglePage)
            {
                Console.WriteLine("  Output to a single page.");
            }

            Console.WriteLine("  Writing documentation output files...");

            // Write the documentation pages
            doc.WritePages();

            Console.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture, "\nExecution time: {0:0.000} s.", w.ElapsedMilliseconds * 1e-3));
        }
    }
}