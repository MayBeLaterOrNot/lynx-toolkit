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

namespace WikiT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using LynxToolkit;
    using LynxToolkit.Documents;
    using LynxToolkit.Documents.OpenXml;

    public class Program
    {
        /// <summary>
        /// Gets or sets the input folder and search pattern.
        /// </summary>
        /// <value>The input.</value>
        public static string Input { get; set; }

        /// <summary>
        /// Gets or sets the default syntax of the input files.
        /// </summary>
        /// <value>The following values are accepted: owiki, md, creole, xml.</value>
        public static string DefaultSyntax { get; set; }

        /// <summary>
        /// Gets or sets the output format.
        /// </summary>
        /// <value>The output format (html, docx, owiki, md, creole, xml).</value>
        public static string Format { get; set; }

        /// <summary>
        /// Gets or sets the output folder.
        /// </summary>
        /// <value>The output folder.</value>
        public static string Output { get; set; }

        /// <summary>
        /// Gets or sets the output extension.
        /// </summary>
        /// <value>The output extension.</value>
        public static string Extension { get; set; }

        /// <summary>
        /// Gets or sets the template (word or html file).
        /// </summary>
        /// <value>The template.</value>
        public static string Template { get; set; }

        /// <summary>
        /// Gets or sets the stylesheet (css file).
        /// </summary>
        public static string Stylesheet { get; set; }

        /// <summary>
        /// Gets or sets the format string for local links.
        /// </summary>
        public static string LocalLinks { get; set; }

        /// <summary>
        /// Gets or sets the format string for links containing a space identifier.
        /// </summary>
        public static string SpaceLinks { get; set; }

        /// <summary>
        /// Gets or sets the defines.
        /// </summary>
        /// <value>The defines.</value>
        public static HashSet<string> Defines { get; set; }

        public static Dictionary<string, string> Replacements { get; set; }

        public static int Main(string[] args)
        {
            Console.WriteLine(Application.Header);
            if (args.Length < 3)
            {
                Console.WriteLine("Arguments: [/input=folder/search-pattern] [/defaultSyntax=owiki] [/format=html] [/extension=.html] [/output=output-folder]");
                Console.WriteLine(@"Example: /input=..\docs\*.wiki /output=..\output");
                return -1;
            }

            // set default values
            Input = "*.wiki";
            DefaultSyntax = null;
            Format = "html";
            Extension = null;
            Output = "output";
            LocalLinks = string.Empty;
            SpaceLinks = string.Empty;
            Defines = new HashSet<string>();
            Replacements = new Dictionary<string, string>();

            foreach (var arg in args)
            {
                var kv = arg.Split('=');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "/input":
                            Input = kv[1];
                            break;
                        case "/output":
                            Output = kv[1];
                            continue;
                        case "/defaultsyntax":
                            DefaultSyntax = kv[1].ToLower();
                            continue;
                        case "/format":
                            Format = kv[1].ToLower();
                            continue;
                        case "/extension":
                            Extension = kv[1];
                            continue;
                        case "/template":
                            Template = kv[1];
                            continue;
                        case "/stylesheet":
                            Stylesheet = kv[1];
                            continue;
                        case "/locallinks":
                            LocalLinks = kv[1];
                            continue;
                        case "/spacelinks":
                            SpaceLinks = kv[1];
                            continue;
                        case "/define":
                            Defines.Add(kv[1]);
                            continue;
                        default:
                            Replacements.Add(kv[0].Trim('/'), kv[1]);
                            continue;
                    }
                }
            }

            if (Extension == null)
            {
                switch (Format)
                {
                    case "html":
                        Extension = ".html";
                        break;
                    case "owiki":
                        Extension = ".wiki";
                        break;
                    case "markdown":
                        Extension = ".md";
                        break;
                    case "word":
                        Extension = ".docx";
                        break;
                    default:
                        Extension = "." + Format;
                        break;
                }
            }

            Utilities.CreateDirectoryIfMissing(Output);

            var inputDirectory = Path.GetDirectoryName(Input);
            var searchPattern = Path.GetFileName(Input);

            var files = Utilities.FindFiles(inputDirectory, searchPattern).ToList();
            foreach (var f in files)
            {
                Console.Write(f);
                if (!Transform(f))
                {
                    Console.Write(" (no change)");
                }

                Console.WriteLine();
            }

            return 0;
        }

        private static bool IsFileModified(string filePath, string newContent)
        {
            if (!File.Exists(filePath))
            {
                return true;
            }

            var content = File.ReadAllText(filePath);
            return !string.Equals(content, newContent);
        }

        private static bool Transform(string filePath)
        {
            var doc = WikiParser.ParseFile(filePath, DefaultSyntax, Replacements, Defines);
            var outputPath = filePath;
            if (Output != null && filePath != null)
            {
                var fileName = Path.GetFileName(filePath) ?? string.Empty;
                outputPath = Path.Combine(Output, fileName);
            }

            outputPath = Path.ChangeExtension(outputPath, Extension);

            string outputText;

            switch (Format)
            {
                case "word":
                    {
                        WordFormatter.Format(doc, outputPath, new WordFormatterOptions { Template = Template });
                        return true;
                    }

                case "owiki":
                    outputText = OWikiFormatter.Format(doc);
                    break;

                case "creole":
                    outputText = CreoleFormatter.Format(doc);
                    break;

                case "markdown":
                    outputText = MarkdownFormatter.Format(doc);
                    break;
                case "xml":
                    outputText = XmlFormatter.Format(doc);
                    break;

                case "html":
                    var options = new HtmlFormatterOptions
                                      {
                                          Css = Stylesheet,
                                          Template = Template,
                                          LocalLinkFormatString = LocalLinks,
                                          SpaceLinkFormatString = SpaceLinks
                                      };
                    outputText = HtmlFormatter.Format(doc, options);
                    break;

                default:
                    throw new FormatException(string.Format("The output format '{0}' is not supported.", Format));
            }

            if (IsFileModified(outputPath, outputText))
            {
                File.WriteAllText(outputPath, outputText);
                return true;
            }

            return false;

        }
    }
}