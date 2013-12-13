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
//   The program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace TodoFinder
{
    using System;
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.XPath;
    using System.Xml.Xsl;

    using LynxToolkit;

    /// <summary>
    /// Counts and lists all TODO comments in the code.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// The trim chars applied to the comment following the TODO.
        /// </summary>
        private static readonly char[] TrimChars = "/=: ".ToCharArray();

        /// <summary>
        /// The regular expression used to search for TODO comments.
        /// </summary>
        private static readonly Regex SearchExpression = new Regex(@"\bTODO\b", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The number of TODO comments.
        /// </summary>
        private static int todoCount;

        /// <summary>
        /// The source directory.
        /// </summary>
        private static string source;

        /// <summary>
        /// The output writer.
        /// </summary>
        private static TextWriter outputWriter;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        /// <returns>the number of TODO comments in the processed files.</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);

            source = Directory.GetCurrentDirectory();
            string output = null;
            string transform = null;

            if (args.Length > 0)
            {
                source = Path.GetFullPath(args[0]);
            }

            if (args.Length > 1)
            {
                output = args[1];
            }

            if (args.Length > 2)
            {
                transform = args[2];
            }

            if (output != null)
            {
                outputWriter = File.CreateText(output);
                outputWriter.WriteLine("<Report>");
            }

            ProcessFolder(source);

            if (outputWriter != null)
            {
                outputWriter.WriteLine("</Report>");
                outputWriter.Close();
                outputWriter.Dispose();
            }

            Console.WriteLine();
            Console.WriteLine("{0} TODO comments found.", todoCount);

            if (transform != null)
            {
                var transformedOutput = Transform(output, transform);
                System.Diagnostics.Process.Start(transformedOutput);
            }

            if (output != null)
            {
               // System.Diagnostics.Process.Start(output);
            }

            return todoCount;
        }

        private static string Transform(string input, string transform)
        {
            var output = Path.ChangeExtension(input, ".html");
            var myXPathDoc = new XPathDocument(input);
            var myXslTrans = new XslCompiledTransform();
            myXslTrans.Load(transform);
            var myWriter = new XmlTextWriter(output, null);
            myXslTrans.Transform(myXPathDoc, null, myWriter);
            return output;
        }

        /// <summary>
        /// Processes the specified directory and subdirectories.
        /// </summary>
        /// <param name="path">The directory path.</param>
        private static void ProcessFolder(string path)
        {
            foreach (var file in Directory.GetFiles(path, "*.cs"))
            {
                ProcessFile(file);
            }

            foreach (var subDirectory in Directory.GetDirectories(path))
            {
                ProcessFolder(subDirectory);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="file">The file path.</param>
        private static void ProcessFile(string file)
        {
            bool first = true;
            int lineNumber = 0;
            foreach (var line in File.ReadLines(file))
            {
                lineNumber++;
                var match = SearchExpression.Match(line);
                if (match.Success)
                {
                    int i = match.Index;
                    var description = line.Substring(i + 4).Trim(TrimChars);
                    if (first)
                    {
                        var relativePath = file.Replace(source, string.Empty).TrimStart('\\');
                        Console.WriteLine(relativePath);
                        if (outputWriter != null)
                        {
                            outputWriter.WriteLine("<File Path=\"{0}\">", relativePath);
                        }

                        first = false;
                    }

                    Console.WriteLine("  [{0}] {1}", lineNumber, description);
                    if (outputWriter != null)
                    {
                        outputWriter.WriteLine("<Comment Line=\"{0}\">{1}</Comment>", lineNumber, description);
                    }

                    todoCount++;
                }
            }

            if (!first)
            {
                if (outputWriter != null)
                {
                    outputWriter.WriteLine("</File>");
                }

                Console.WriteLine();
            }
        }
    }
}
