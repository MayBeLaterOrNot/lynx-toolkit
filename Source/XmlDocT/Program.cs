// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
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
            Console.WriteLine(Application.Header);

            var output = ".";
            var format = "html";
            string outputExtension = null;
            string stylesheet = null;
            string template = null;
            var singlePage = false;
            var createMemberPages = false;
            int topLevel = 1;
            var input = new List<string>();

            var model = new NamespaceCollection();

            foreach (var arg in args)
            {
                var kv = arg.Split('=');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "/singlepage":
                            singlePage = true;
                            break;
                        case "/creatememberpages":
                            createMemberPages = true;
                            break;
                        case "/input":
                            input.Add(kv[1]);
                            continue;
                        case "/output":
                            output = kv[1];
                            continue;
                        case "/stylesheet":
                            stylesheet = kv[1];
                            continue;
                        case "/toplevel":
                            topLevel = int.Parse(kv[1]);
                            continue;
                        case "/template":
                            template = kv[1];
                            continue;
                        case "/format":
                            format = kv[1].ToLower();
                            continue;
                        case "/extension":
                            outputExtension = kv[1].ToLower();
                            continue;
                        case "/ignore":
                            model.IgnoreAttributes.Add(kv[1]);
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
                        model.Add(fileName);
                    }

                    continue;
                }

                if (File.Exists(path))
                {
                    Console.WriteLine("Processing " + path);
                    model.Add(path);
                    continue;
                }

                if (Directory.Exists(path))
                {
                    Console.WriteLine("Processing " + path + @"\\*.dll");
                    foreach (var fileName in Directory.GetFiles(path, "*.dll"))
                    {
                        model.Add(fileName);
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("Output");

            var outputDirectory = singlePage ? Path.GetDirectoryName(output) : output;

            if (!Directory.Exists(outputDirectory))
            {
                Console.WriteLine("  Creating output directory: {0}", Path.GetFullPath(outputDirectory));
                Directory.CreateDirectory(outputDirectory);
            }
            else
            {
                Console.WriteLine("  Output directory: {0}", Path.GetFullPath(outputDirectory));
            }

            if (singlePage)
            {
                Console.WriteLine("  Output file: {0}", Path.GetFullPath(output));
            }

            //if (stylesheet != null)
            //{
            //    Console.WriteLine("  Copying stylesheet: {0}", Path.GetFullPath(stylesheet));
            //    var destStylesheet = Path.Combine(output, Path.GetFileName(stylesheet));
            //    File.Copy(stylesheet, destStylesheet, true);
            //}

            Console.WriteLine("  Output format: {0}", format);
            if (singlePage)
            {
                Console.WriteLine("  Output to a single page.");
            }

            Console.WriteLine("  Writing documentation output files...");

            // Write the documentation pages
            DocFormatter.CreatePages(model, output, format, outputExtension, stylesheet, template, singlePage, createMemberPages, topLevel);

            Console.WriteLine();
            Console.WriteLine(
                string.Format(
                    CultureInfo.InvariantCulture, "Completed in {0:0.0} seconds.", w.ElapsedMilliseconds * 1e-3));
        }
    }
}