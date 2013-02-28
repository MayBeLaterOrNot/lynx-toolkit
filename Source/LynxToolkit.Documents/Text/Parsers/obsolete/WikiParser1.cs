// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiParser.cs" company="Lynx Toolkit">
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
//   Parses the specified text.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class WikiParser1
    {
        private static readonly Regex DefineExpression;

        private static readonly Regex DirectivesExpression;

        private static readonly Regex IncludeExpression;

        private static readonly Regex ImportExpression;

        static WikiParser1()
        {
            DirectivesExpression = new Regex(@"(?<=[^\\]|^)     # Not escaped
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

            IncludeExpression = new Regex(@"(?<=[^\\]|^)     # Not escaped
(?:
(@include \s (?<include>.+?) \r?\n)|
(@import \s (?<import>.+?) \r?\n)|
(?<index>@index .*? \r?\n)|
(?<toc>@toc \s* (?<levels>\d+)? .*? \r?\n)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            ImportExpression = new Regex(@"(?<=[^\\]|^)     # Not escaped
(@import \s (?<import>.+?) \r?\n)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

            DefineExpression = new Regex(
                @"^\s*@if\s+(?<criteria>.+?)\s*$(?<block>.*?)^@endif",
                RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline);
        }

        public Document Document { get; set; }

        public string Syntax { get; set; }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="documentFolder">The include resolve path (used to resolve include files).</param>
        /// <param name="includeDefaultExtension">The default extension (used to resolve include files).</param>
        /// <param name="defaultSyntax">The default syntax.</param>
        /// <param name="replacements">The variables.</param>
        /// <param name="defines">The defines.</param>
        /// <returns>
        /// A Document.
        /// </returns>
        /// <exception cref="System.IO.FileNotFoundException">Include file not found</exception>
        public static Document Parse(
            string text,
            string documentFolder,
            string includeDefaultExtension,
            string defaultSyntax,
            Dictionary<string, string> replacements = null,
            HashSet<string> defines = null)
        {
            if (defines == null)
            {
                defines = new HashSet<string>();
            }

            string title = null,
                   description = null,
                   keywords = null,
                   syntax = defaultSyntax,
                   creator = null,
                   subject = null,
                   category = null,
                   date = null,
                   version = null,
                   revision = null;
            var includepaths = new List<string> { "." };

            if (replacements != null)
            {
                foreach (var kvp in replacements)
                {
                    text = text.Replace("$" + kvp.Key, kvp.Value);
                }
            }

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

            text = DirectivesExpression.Replace(
                text,
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

            text = ImportExpression.Replace(
                text,
                m =>
                {
                    var importFile = m.Groups["import"].Value;
                    var importFilePath = ResolveInclude(importFile, documentFolder, includepaths, includeDefaultExtension);
                    if (importFilePath == null)
                    {
                        throw new FileNotFoundException("Import file not found (" + importFile + ")", importFile);
                    }

                    return File.ReadAllText(importFilePath);
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
                        doc.Append(ParseCore(s, syntax, documentFolder));
                    }
                }

                index = match.Index + match.Length;

                string include = null;
                if (match.SetIfSuccess("include", ref include))
                {
                    // todo: support wildcards?
                    var includeFilePath = ResolveInclude(include, documentFolder, includepaths, includeDefaultExtension);
                    if (includeFilePath == null)
                    {
                        throw new FileNotFoundException("Include file not found (" + include + ")", include);
                    }

                    var includeText = File.ReadAllText(includeFilePath);
                    doc.Append(
                        Parse(
                            includeText,
                            Path.GetDirectoryName(includeFilePath),
                            includeDefaultExtension,
                            defaultSyntax,
                            replacements,
                            defines));
                    continue;
                }

                if (match.Groups["index"].Success)
                {
                    doc.Blocks.Add(new Index());
                }

                if (match.Groups["toc"].Success)
                {
                    var toc = new TableOfContents();
                    if (match.Groups["levels"].Success)
                    {
                        toc.Levels = int.Parse(match.Groups["levels"].Value);
                    }

                    doc.Blocks.Add(toc);
                }
            }

            if (index < text.Length)
            {
                var s = text.Substring(index);
                if (!string.IsNullOrWhiteSpace(s))
                {
                    doc.Append(ParseCore(s, syntax, documentFolder));
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

        public static Document ParseFile(string filePath, string defaultSyntax = null, Dictionary<string, string> replacements = null, HashSet<string> defines = null)
        {
            var text = File.ReadAllText(filePath);
            return Parse(text, Path.GetDirectoryName(filePath), Path.GetExtension(filePath), defaultSyntax, replacements, defines);
        }

        private static Document ParseCore(string text, string syntax, string documentFolder)
        {
            if (text.StartsWith("<?xml"))
            {
                return XmlFormatter.Parse(text);
            }

            Document doc;
            switch (syntax)
            {
                case "creole":
                    doc = CreoleParser.Parse(text, documentFolder);
                    break;
                case "markdown":
                case "md":
                    doc = MarkdownParser.Parse(text, documentFolder);
                    break;
                case "confluence":
                    throw new Exception("Confluence wiki parsing not supported.");

                // doc = ConfluenceParser.Parse(text);
                case "codeplex":
                    throw new Exception("Codeplex wiki parsing not supported.");

                // doc = CodeplexParser.Parse(text);
                default:
                    doc = OWikiParser1.Parse(text, documentFolder);
                    break;
            }

            return doc;
        }

        private static string ResolveInclude(
            string include, string documentPath, IEnumerable<string> includepaths, string ext)
        {
            foreach (var p in includepaths)
            {
                var f = Path.Combine(Path.Combine(documentPath, p), include);
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
    }
}