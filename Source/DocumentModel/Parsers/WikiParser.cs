namespace LynxToolkit.Documents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class WikiParser
    {
        public Document Document { get; set; }
        public string Syntax { get; set; }

        private static readonly Regex DirectivesExpression;
        private static readonly Regex IncludeExpression;
        private static readonly Regex DefineExpression;

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
            DefineExpression = new Regex(
                @"^\s*@if\s*(?<criteria>.+?)\s*$(?<block>.*?)^@endif", RegexOptions.Compiled | RegexOptions.Singleline);
        }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="includePath">The include resolve path (used to resolve include files).</param>
        /// <param name="includeDefaultExtension">The default extension (used to resolve include files).</param>
        /// <param name="defaultSyntax">The default syntax.</param>
        /// <param name="defines">The defines.</param>
        /// <returns>A Document.</returns>
        /// <exception cref="System.IO.FileNotFoundException">Include file not found</exception>
        public static Document Parse(string text, string includePath, string includeDefaultExtension, string defaultSyntax, HashSet<string> defines = null)
        {
            if (defines == null)
            {
                defines = new HashSet<string>();
            }

            string title = null, description = null, keywords = null, syntax = defaultSyntax, creator = null, subject = null, category = null, date = null, version = null, revision = null;
            var includepaths = new List<string> { "." };

            // solve @if ... @endif expressions
            text = DefineExpression.Replace(
                text,
                m =>
                {
                    var criteria = m.Groups["criteria"].Value;
                    if (defines.Contains(criteria))
                    {
                        return m.Groups["block"].Value;
                    }
                    return string.Empty;
                });

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
                    // todo: support wildcards?
                    var includeFilePath = ResolveInclude(include, includePath, includepaths, includeDefaultExtension);
                    if (includeFilePath == null)
                    {
                        throw new FileNotFoundException("Include file not found", include);
                    }

                    var includeText = File.ReadAllText(includeFilePath);
                    doc.Append(Parse(includeText, Path.GetDirectoryName(includeFilePath), includeDefaultExtension, defaultSyntax, defines));
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
                if (ext != null && string.IsNullOrEmpty(Path.GetExtension(f)))
                {
                    f = Path.ChangeExtension(f, ext);
                }

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

        public static Document ParseFile(string filePath, string defaultSyntax = null, HashSet<string> defines = null)
        {
            var text = File.ReadAllText(filePath);
            return Parse(text, Path.GetDirectoryName(filePath), Path.GetExtension(filePath), defaultSyntax, defines);
        }
    }
}