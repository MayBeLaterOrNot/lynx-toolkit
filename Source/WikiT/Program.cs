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
//   The WikiT program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WikiT
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    using LynxToolkit;
    using LynxToolkit.Documents;
    using LynxToolkit.Documents.Html;
    using LynxToolkit.Documents.OpenXml;

    /// <summary>
    /// The WikiT program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Specifies the output formats
        /// </summary>
        public enum OutputFormats
        {
            /// <summary>
            /// Output to HTML
            /// </summary>
            Html,

            /// <summary>
            /// Output to Word
            /// </summary>
            Word,

            /// <summary>
            /// Output to latex
            /// </summary>
            Tex,

            /// <summary>
            /// Output to owiki
            /// </summary>
            Owiki,

            /// <summary>
            /// Output to Markdown
            /// </summary>
            Markdown,

            /// <summary>
            /// Output to Creole
            /// </summary>
            Creole,

            /// <summary>
            /// Output to XML
            /// </summary>
            Xml,

            /// <summary>
            /// Output to JSON
            /// </summary>
            Json
        }

        /// <summary>
        /// Gets or sets the input folder and search pattern.
        /// </summary>
        /// <value>The input.</value>
        public static string Input { get; set; }

        /// <summary>
        /// Gets or sets the output format.
        /// </summary>
        /// <value>The output format (html, word, tex, wiki, markdown, creole, xml).</value>
        public static OutputFormats Format { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to force output to be written.
        /// </summary>
        /// <value>
        ///   <c>true</c> if output files should always be written; otherwise, <c>false</c>.
        /// </value>
        public static bool ForceOutput { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to flatten the output file hierarchy.
        /// </summary>
        /// <value><c>true</c> if output should be flattened; otherwise, <c>false</c>.</value>
        public static bool FlattenOutput { get; set; }

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
        /// Gets or sets the define constants.
        /// </summary>
        /// <value>The define constants.</value>
        /// <remarks>
        /// The defines can be used in "@if DEFINE" ... "@endif" blocks
        /// </remarks>
        public static List<string> Defines { get; set; }

        /// <summary>
        /// Gets or sets the variables.
        /// </summary>
        /// <value>
        /// The variable strings.
        /// </value>
        /// <remarks>
        /// The variable keys will be prefixed by "$" and replaced by their values.
        /// </remarks>
        public static Dictionary<string, string> Variables { get; set; }

        /// <summary>
        /// The main entry point.
        /// </summary>
        /// <param name="args">The args.</param>
        /// <returns>The exit code, 0 if successful.</returns>
        public static int Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);
            if (args.Length == 0 || args[0] == "/?")
            {
                Console.WriteLine("Arguments: [/project=path] [/define=XYZ] [/key=value]");
                Console.WriteLine(@"Example: WikiT /project=..\docs\MyApplication.wikiproj /Version=1.0");
                Console.WriteLine("The wiki project file format is defined in x.");
                Console.WriteLine();
                Console.WriteLine("Arguments: [/input=folder/search-pattern] [/format=html|word|tex|wiki|markdown|creole|xml] [/output=output-folder] [/forceoutput] [/extension=.html] [/template=template.html] [/stylesheet=style.css] [/define=XYZ] [/key=value]");
                Console.WriteLine(@"Example: WikiT /input=..\docs\*.wiki /output=..\output");
                return -1;
            }

            ParseArguments(args);

            return ProcessFiles();
        }

        /// <summary>
        /// Processes the files.
        /// </summary>
        /// <returns>the exit code.</returns>
        private static int ProcessFiles()
        {
            var inputDirectory = Path.GetFullPath(Path.GetDirectoryName(Input) ?? ".");
            var searchPattern = Path.GetFileName(Input);

            Console.WriteLine("Input directory:  '{0}'", Path.GetFullPath(inputDirectory));
            Console.WriteLine("Search pattern:   '{0}'", searchPattern);
            Console.WriteLine("Output directory: '{0}'", Path.GetFullPath(Output));
            Console.WriteLine("Output extension: '{0}'", Extension);

            var files = Utilities.FindFiles(inputDirectory, searchPattern).ToList();

            Console.WriteLine();
            Console.WriteLine(files.Count + " input files found.");

            Console.WriteLine();

            var w = Stopwatch.StartNew();
#if !XDEBUG
            try
            {
#endif
                Utilities.CreateDirectoryIfMissing(Output);

                foreach (var f in files)
                {
                    var relativePath = Utilities.MakeRelativePath(inputDirectory + "\\", f);
                    Console.Write(Path.GetFileName(relativePath));
                    if (!Transform(inputDirectory, relativePath))
                    {
                        Console.Write(" (no change)");
                    }

                    Console.WriteLine();
                }
#if !XDEBUG
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("  Exception: " + e.Message);
                return 1;
            }
#endif
            Console.WriteLine(
                string.Format(CultureInfo.InvariantCulture, "\nExecution time: {0:0.000} s", w.ElapsedMilliseconds * 0.001));

            return 0;
        }

        /// <summary>
        /// Parses the command line arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        private static void ParseArguments(IEnumerable<string> args)
        {
            // set default values
            Input = "*.wiki";
            Format = OutputFormats.Html;
            Extension = null;
            Output = "output";
            Defines = new List<string>();
            Variables = new Dictionary<string, string>();

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
                        case "/format":
                            OutputFormats format;
                            if (Enum.TryParse(kv[1].ToLower(), true, out format))
                            {
                                Format = format;
                            }
                            else
                            {
                                Console.WriteLine("Unknown format: " + kv[1]);
                            }

                            continue;
                        case "/flatten":
                            FlattenOutput = true;
                            continue;
                        case "/force":
                            ForceOutput = true;
                            continue;
                        case "/extension":
                            Extension = kv[1];
                            continue;
                        case "/project":
                            ParseProject(kv[1]);
                            continue;
                        case "/template":
                            Template = kv[1];
                            continue;
                        case "/stylesheet":
                            Stylesheet = kv[1];
                            continue;
                        case "/define":
                            Defines.Add(kv[1]);
                            continue;
                        default:
                            if (kv.Length > 1)
                            {
                                var key = kv[0].Trim('/');
                                Variables[key] = kv[1];
                            }

                            continue;
                    }
                }
            }

            if (Extension == null)
            {
                switch (Format)
                {
                    case OutputFormats.Html:
                        Extension = ".html";
                        break;
                    case OutputFormats.Owiki:
                        Extension = ".wiki";
                        break;
                    case OutputFormats.Markdown:
                        Extension = ".md";
                        break;
                    case OutputFormats.Word:
                        Extension = ".docx";
                        break;
                    default:
                        Extension = "." + Format.ToString().ToLower();
                        break;
                }
            }

            if (!Extension.StartsWith("."))
            {
                Console.WriteLine("The output extension should start with '.'");
            }
        }

        /// <summary>
        /// Parses project settings from a wiki project file.
        /// </summary>
        /// <param name="fileName">Project filename.</param>
        private static void ParseProject(string fileName)
        {
            XDocument doc;
            using (var stream = File.OpenRead(fileName))
            {
                doc = XDocument.Load(stream);
            }

            var dir = Path.GetDirectoryName(fileName) ?? ".";

            foreach (var d in doc.Descendants())
            {
                if (d.Name == "Format")
                {
                    OutputFormats format;
                    if (Enum.TryParse(d.Value, true, out format))
                    {
                        Format = format;
                    }
                }

                if (d.Name == "Template")
                {
                    Template = PathUtilities.Simplify(Path.Combine(dir, d.Value));
                }

                if (d.Name == "StyleSheet")
                {
                    Stylesheet = PathUtilities.Simplify(Path.Combine(dir, d.Value));
                }

                if (d.Name == "LocalLinks")
                {
                }

                if (d.Name == "Input")
                {
                    Input = PathUtilities.Simplify(Path.Combine(dir, d.Value));
                }

                if (d.Name == "Output")
                {
                    Output = PathUtilities.Simplify(Path.Combine(dir, d.Value));
                }

                if (d.Name == "Variable")
                {
                    var key = d.Attribute("Key").Value;
                    Variables[key] = d.Value;
                }
            }
        }

        /// <summary>
        /// Transforms the specified file.
        /// </summary>
        /// <param name="inputFolder">The input folder.</param>
        /// <param name="relativeFilePath">The relative file path.</param>
        /// <returns>True if the file was modified.</returns>
        private static bool Transform(string inputFolder, string relativeFilePath)
        {
            if (inputFolder == null)
            {
                throw new ArgumentNullException("inputFolder");
            }

            if (relativeFilePath == null)
            {
                throw new ArgumentNullException("relativeFilePath");
            }

            var filePath = Path.Combine(inputFolder, relativeFilePath);
            var parser = new WikiParser(Defines, Variables, File.OpenRead) { IncludeDefaultExtension = Path.GetExtension(Input) };
            var doc = parser.ParseFile(filePath);

            if (FlattenOutput)
            {
                relativeFilePath = Path.GetFileName(relativeFilePath);
            }

            var outputPath = Output != null ? Path.Combine(Output, relativeFilePath) : relativeFilePath;
            outputPath = Path.ChangeExtension(outputPath, Extension);

            var outputDirectory = Path.GetDirectoryName(outputPath);

            IDocumentFormatter formatter = null;
            switch (Format)
            {
                case OutputFormats.Word:
                    formatter = new WordFormatter { Template = Template };
                    break;

                case OutputFormats.Owiki:
                    formatter = new OWikiFormatter();
                    break;

                case OutputFormats.Creole:
                    formatter = new CreoleFormatter();
                    break;

                case OutputFormats.Markdown:
                    formatter = new MarkdownFormatter();
                    break;

                case OutputFormats.Xml:
                    formatter = new XmlFormatter();
                    break;

                case OutputFormats.Json:
                    formatter = new JsonFormatter();
                    break;

                case OutputFormats.Tex:
                    formatter = new TexFormatter();
                    break;

                case OutputFormats.Html:
                    formatter = new HtmlFormatter
                                      {
                                          Css = Stylesheet,
                                          Template = Template,
                                          LocalLinkFormatString = "{0}" + Extension,
                                          SpaceLinkFormatString = "{0}/{1}" + Extension,
                                          Variables = Variables,
                                          OutputDirectory = outputDirectory,
                                          ImageBaseDirectory = inputFolder
                                      };
                    break;
            }

            if (formatter == null)
            {
                throw new FormatException(string.Format("The output format '{0}' is not supported.", Format));
            }

            Utilities.CreateDirectoryIfMissing(outputDirectory);
            using (var stream = File.Create(outputPath))
            {
                formatter.Format(doc, stream);
            }

            return true;
        }
    }
}