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
        private static Dictionary<string, string> config;

        private static string configDirectory;

        public static int Main(string[] args)
        {
            Console.WriteLine(Application.Header);
            if (args.Length < 3)
            {
                Console.WriteLine("Arguments: <config-file> <directory> <search-pattern> <output-directory>");
                Console.WriteLine("Example: UserManual.config ..\\docs *.wiki ..\\output");
                return -1;
            }

            var configFile = args[0];
            var inputDirectory = args[1];
            var searchPattern = args[2];
            var outputDirectory = args.Length > 3 ? args[3] : null;
            config = LoadConfiguration(configFile);
            configDirectory = Path.GetDirectoryName(configFile);
            if (outputDirectory != null)
            {
                config["OutputDirectory"] = outputDirectory;
            }

            var files = FindFiles(inputDirectory, searchPattern).ToList();
            foreach (var f in files)
            {
                if (Transform(f))
                {
                    Console.WriteLine(f);
                }
            }

            return 0;
        }

        private static string CreateExtension(string format)
        {
            return format.StartsWith(".") ? format : "." + format;
        }

        private static IEnumerable<string> FindFiles(string directory, string searchPattern)
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
        ///     Gets a file name from the configuration dictionary.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns></returns>
        /// <remarks>
        ///     The file name must be resolved relative to the path of the configuration file.
        /// </remarks>
        private static string GetFileName(string key)
        {
            string path;
            if (config.TryGetValue(key, out path))
            {
                return Path.Combine(configDirectory, path);
            }

            return null;
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

        private static Dictionary<string, string> LoadConfiguration(string path)
        {
            var result = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("#"))
                {
                    continue;
                }

                var fields = line.Split('=');
                result.Add(fields[0].Trim(), fields[1].Trim());
            }

            return result;
        }

        private static void OpenForEdit(string outputPath, Dictionary<string, string> config)
        {
            Utilities.OpenForEdit(outputPath, config["Scc"]);
        }

        private static bool Transform(string filePath)
        {
            string defaultSyntax;
            config.TryGetValue("DefaultSyntax", out defaultSyntax);

            var text = File.ReadAllText(filePath);
            var doc = WikiParser.Parse(
                text, Path.GetDirectoryName(filePath), Path.GetExtension(filePath), defaultSyntax);
            var outputFormat = config["OutputFormat"];
            var outputDirectory = config["OutputDirectory"];
            var outputExtension = CreateExtension(outputFormat);
            var outputPath = filePath;
            if (outputDirectory != null)
            {
                outputPath = Path.Combine(outputDirectory, Path.GetFileName(filePath));
            }

            outputPath = Path.ChangeExtension(outputPath, outputExtension);

            switch (outputFormat)
            {
                case "docm":
                case "docx":
                    {
                        OpenForEdit(outputPath, config);
                        WordFormatter.Format(doc, outputPath, new WordFormatterOptions { Template = GetFileName("Template") });
                        return true;
                    }

                case "html":
                case "htm":
                    {
                        string localLinks, spaceLinks;
                        config.TryGetValue("LocalLinks", out localLinks);
                        config.TryGetValue("SpaceLinks", out spaceLinks);
                        var options = new HtmlFormatterOptions
                                          {
                                              Css = GetFileName("css"),
                                              Template = GetFileName("Template"),
                                              LocalLinkFormatString = localLinks,
                                              SpaceLinkFormatString = spaceLinks
                                          };
                        var html = HtmlFormatter.Format(doc, options);
                        if (IsFileModified(outputPath, html))
                        {
                            OpenForEdit(outputPath, config);
                            File.WriteAllText(outputPath, html);
                            return true;
                        }

                        return false;
                    }

                default:
                    throw new FormatException(string.Format("The output format '{0}' is not supported.", outputFormat));
            }
        }
    }
}