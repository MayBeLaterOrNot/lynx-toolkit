namespace LynxToolkit.Documents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;
    public static class MatchExtensions
    {
        public static bool SetIfSuccess(this Match m, string key, ref string output)
        {
            if (m.Groups[key].Success)
            {
                output = m.Groups[key].Value;
                return true;
            }

            return false;
        }
    }

    public class WikiParser
    {
        public Document Document { get; set; }
        public string Syntax { get; set; }

        private static readonly Regex DirectivesExpression;
        private static readonly Regex IncludeExpression;

        static WikiParser()
        {
            DirectivesExpression = new Regex(
@"(?<=[^\\]|^)     # Not escaped
@(?:
(?:syntax \s (?<syntax>.+?) \r?\n)|
(?:title \s (?<title>.+?) \r?\n)|
(?:description \s (?<description>.+?) \r?\n)|
(?:keywords \s (?<keywords>.+?) \r?\n)|
(?:creator \s (?<creator>.+?) \r?\n)|
(?:subject \s (?<subject>.+?) \r?\n)|
(?:category \s (?<category>.+?) \r?\n)|
(?:version \s (?<version>.+?) \r?\n)|
(?:revision \s (?<revision>.+?) \r?\n)|
(?:date \s (?<date>.+?) \r?\n)|
(?:includepath \s (?<includepath>.+?) \r?\n)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            IncludeExpression = new Regex(
@"(?<=[^\\]|^)     # Not escaped
(?:
(@include \s (?<include>.+?) \r?\n)|
(?<index>@index .*? \r?\n)|
(?<toc>@toc .*? \r?\n)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        }

        public static Document Parse(string text, string path, string ext, string defaultSyntax)
        {
            string title = null, description = null, keywords = null, syntax = defaultSyntax, creator = null, subject = null, category = null, date = null, version = null, revision = null;
            var includepaths = new List<string>();
            includepaths.Add(".");

            text = DirectivesExpression.Replace(text,
                m =>
                {
                    m.SetIfSuccess("syntax", ref syntax);
                    m.SetIfSuccess("title", ref title);
                    m.SetIfSuccess("description", ref description);
                    m.SetIfSuccess("keywords", ref keywords);
                    m.SetIfSuccess("creator", ref creator);
                    m.SetIfSuccess("subject", ref subject);
                    m.SetIfSuccess("category", ref category);
                    m.SetIfSuccess("version", ref version);
                    m.SetIfSuccess("revision", ref revision);
                    m.SetIfSuccess("date", ref date);
                    string includepath = null;
                    if (m.SetIfSuccess("includepath", ref includepath))
                    {
                        includepaths.Add(includepath);
                    }

                    return string.Empty;
                });

            var doc = new Document();
            int index = 0;

            foreach (Match match in IncludeExpression.Matches(text))
            {
                if (match.Index > index)
                {
                    var s = text.Substring(index, match.Index - index);
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        doc.Append(ParseCore(s, syntax));
                    }
                }
                index = match.Index + match.Length;

                string include = null;
                if (match.SetIfSuccess("include", ref include))
                {
                    var includeFilePath = ResolveInclude(include, path, includepaths, ext);
                    if (includeFilePath == null)
                    {
                        throw new FileNotFoundException("Include file not found", include);
                    }

                    var includeText = File.ReadAllText(includeFilePath);
                    doc.Append(Parse(includeText, Path.GetDirectoryName(includeFilePath), ext, defaultSyntax));
                    continue;
                }
                if (match.Groups["index"].Success)
                {
                    doc.Blocks.Add(new Index());                    
                }
                if (match.Groups["toc"].Success)
                {
                    doc.Blocks.Add(new TableOfContents());
                }
            }

            if (index < text.Length)
            {
                var s = text.Substring(index);
                if (!string.IsNullOrWhiteSpace(s))
                {
                    doc.Append(ParseCore(s, syntax));
                }
            }

            doc.Title = title;
            doc.Description = description;
            doc.Keywords = keywords;
            doc.Creator = creator;
            doc.Subject = subject;
            doc.Category = category;
            doc.Version = version;
            doc.Revision = revision;
            doc.Date = date;

            return doc;
        }

        private static string ResolveInclude(string include, string path, IEnumerable<string> includepaths, string ext)
        {
            foreach (var p in includepaths)
            {
                var f = Path.Combine(Path.Combine(path, p), include);
                if (ext != null)
                {
                    f = Path.ChangeExtension(f, ext);
                }
                var fullPath = Path.GetFullPath(f);
                if (File.Exists(f))
                {
                    return f;
                }
            }

            return null;
        }

        private static Document ParseCore(string text, string syntax)
        {
            Document doc;
            switch (syntax)
            {
                case "creole":
                    doc = CreoleParser.Parse(text);
                    break;
                case "markdown":
                case "md":
                    doc = MarkdownParser.Parse(text);
                    break;
                case "confluence":
                    throw new Exception("Confluence wiki parsing not supported.");
                // doc = ConfluenceParser.Parse(text);
                case "codeplex":
                    throw new Exception("Codeplex wiki parsing not supported.");
                // doc = CodeplexParser.Parse(text);
                default:
                    doc = OWikiParser.Parse(text);
                    break;
            }
            return doc;
        }

        public static Document ParseFile(string filePath, string defaultSyntax = null)
        {
            var text = File.ReadAllText(filePath);
            return Parse(text, Path.GetDirectoryName(filePath), Path.GetExtension(filePath), defaultSyntax);
        }
    }
}