namespace WikiT
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    using LynxToolkit;
    using LynxToolkit.Documents;
    using LynxToolkit.Documents.OpenXml;

    class Program
    {
        static int Main(string[] args)
        {
            Console.WriteLine(Application.Header);
            if (args.Length != 3)
            {
                Console.WriteLine("Arguments: <config-file> <directory> <search-pattern>");
                Console.WriteLine("Example: UserManual.cfg ..\\docs *.wiki");
                return -1;
            }

            var configFile = args[0];
            var inputDirectory = args[1];
            var searchPattern = args[2];
            var config = LoadConfig(configFile);
            config["BaseDirectory"] = Path.GetDirectoryName(configFile);

            var files = FindFiles(inputDirectory, searchPattern).ToList();
            foreach (var f in files)
            {
                if (Transform(f, config))
                {
                    Console.WriteLine(f);
                }
            }

            return 0;
        }

        private static bool Transform(string filePath, Dictionary<string, string> config)
        {
            string defaultSyntax;
            config.TryGetValue("DefaultSyntax", out defaultSyntax);

            var text = File.ReadAllText(filePath);
            var doc = WikiParser.Parse(text, Path.GetDirectoryName(filePath), Path.GetExtension(filePath), defaultSyntax);
            var outputFormat = config["OutputFormat"];
            switch (outputFormat)
            {
                case "docm":
                case "docx":
                    {
                        var outputPath = Path.ChangeExtension(filePath, CreateExtension(outputFormat));
                        OpenForEdit(outputPath, config);
                        WordFormatter.Format(doc, outputPath, GetFileName(config, "Template"));
                        return true;
                    }

                case "html":
                case "htm":
                    {
                        var outputPath = Path.ChangeExtension(filePath, CreateExtension(outputFormat));
                        string localLinks, spaceLinks;
                        config.TryGetValue("LocalLinks", out localLinks);
                        config.TryGetValue("SpaceLinks", out spaceLinks);
                        var options = new HtmlFormatterOptions
                                          {
                                              Css = GetFileName(config, "css"),
                                              Template = GetFileName(config, "Template"),
                                              LocalLinkFormatString = localLinks,
                                              SpaceLinkFormatString = spaceLinks
                                          };
                        var html = HtmlFormatter.Format(doc, options);
                        if (IsModified(outputPath, html))
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

        private static string GetFileName(Dictionary<string, string> config, string key)
        {
            var basedir = config["BaseDirectory"];
            string path;
            if (config.TryGetValue(key, out path))
                return Path.Combine(basedir, path);
            return null;
        }

        private static bool IsModified(string filePath, string newContent)
        {
            if (!File.Exists(filePath)) return true;
            var content = File.ReadAllText(filePath);
            return !string.Equals(content, newContent);
        }

        private static string CreateExtension(string format)
        {
            return format.StartsWith(".") ? format : "." + format;
        }

        private static void OpenForEdit(string outputPath, Dictionary<string, string> config)
        {
            Utilities.OpenForEdit(outputPath, config["Scc"]);
        }

        private static Dictionary<string, string> LoadConfig(string path)
        {
            var result = new Dictionary<string, string>();
            foreach (var line in File.ReadAllLines(path))
            {
                if (line.StartsWith("#")) continue;
                var fields = line.Split('=');
                result.Add(fields[0].Trim(), fields[1].Trim());
            }

            return result;
        }

        private static IEnumerable<string> FindFiles(string directory, string searchPattern)
        {
            foreach (var file in Directory.GetFiles(directory, searchPattern)) yield return file;

            foreach (var file in Directory.GetDirectories(directory).SelectMany(d => FindFiles(d, searchPattern)))
            {
                yield return file;
            }
        }
    }
}
