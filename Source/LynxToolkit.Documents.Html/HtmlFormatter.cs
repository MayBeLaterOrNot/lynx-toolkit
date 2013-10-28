// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlFormatter.cs" company="Lynx Toolkit">
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
//   Gets or sets the cascading style sheet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents.Html
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class HtmlFormatter : DocumentFormatter<XmlWriter>
    {

        public string OutputDirectory { get; private set; }

        public string EquationFilePrefix { get; set; }

        /// <summary>
        /// The encodings
        /// </summary>
        private static readonly Dictionary<string, string> Encodings;

        /// <summary>
        /// Initializes static members of the <see cref="HtmlFormatter"/> class.
        /// </summary>
        static HtmlFormatter()
        {
            Encodings = new Dictionary<string, string> { { "&", "&amp;" }, { ">", "&gt;" }, { "<", "&lt;" } };
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HtmlFormatter"/> class.
        /// </summary>
        public HtmlFormatter()
        {
            this.OutputDirectory = ".";
            this.EquationFilePrefix = "Equations";
            this.LocalLinkFormatString = "{0}.html";
            this.SpaceLinkFormatString = "{0}/{1}.html";
            this.EquationFormatString = "{0}/Equation{1}.png";
            this.CacheEquations = true;

            this.InternalLinkSpaces = new Dictionary<string, string>
                                          {
                                              { "youtube", @"http://www.youtube.com/watch?v={0}" },
                                              { "vimeo", @"http://vimeo.com/{0}" },
                                              { "google", @"http://www.google.com/?q={0}" }
                                          };

            this.ImageWrapperClass = "figure";
            this.ImageCaptionClass = "caption";
        }

        /// <summary>
        ///     Gets or sets the cascading style sheet.
        /// </summary>
        public string Css { get; set; }

        /// <summary>
        ///     Gets or sets the footer.
        /// </summary>
        public string Footer { get; set; }

        /// <summary>
        ///     Gets the internal link space dictionary.
        /// </summary>
        public Dictionary<string, string> InternalLinkSpaces { get; private set; }

        /// <summary>
        ///     Gets or sets the format string for local links (without space or http prefix).
        /// </summary>
        /// <remarks>{0} will be replaced by the local link reference.</remarks>
        public string LocalLinkFormatString { get; set; }

        /// <summary>
        ///     Gets or sets the format string for local links containing a space (e.g. "space:id").
        /// </summary>
        /// <remarks>{0} will be replaced by the space, and {1} will be replaced by the id.</remarks>
        public string SpaceLinkFormatString { get; set; }

        /// <summary>
        /// Gets or sets the equation format string. {0} will be replaced by the page name. {1} will be replaced by the equation number within the page.
        /// </summary>
        public string EquationFormatString { get; set; }

        /// <summary>
        /// Gets or sets the image wrapper class. If this property is not defined, a wrapper will not be generated.
        /// </summary>
        /// <value>The image wrapper class.</value>
        public string ImageWrapperClass { get; set; }

        /// <summary>
        /// Gets or sets the image caption class. If this property is not defined, a caption will not be generated.
        /// </summary>
        /// <value>The image caption class.</value>
        public string ImageCaptionClass { get; set; }

        /// <summary>
        /// Gets or sets the variable strings.
        /// </summary>
        /// <value>
        /// The variable strings.
        /// </value>
        /// <remarks>
        /// The variable keys will be prefixed by "$" and replaced by their values.
        /// </remarks>
        public Dictionary<string, string> Variables { get; set; }

        /// <summary>
        ///     Gets or sets the template document (this will override the Css style sheet).
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to cache equation images.
        /// </summary>
        public bool CacheEquations { get; set; }

        /// <summary>
        /// Formats the specified document as HTML.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <returns><c>true</c> if the output file was modified, <c>false</c> otherwise</returns>
        public override void Format(Document doc, Stream stream)
        {
            var w = new StringWriter();

            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true };
            var context = XmlWriter.Create(w, settings);

            this.Format(doc, context);

            var html = w.ToString();
            html = this.ApplyTemplate(doc, html);
            var tw = new StreamWriter(stream, Encoding.UTF8);
            tw.Write(html);
        }

        /// <summary>
        /// Applies the template to the formatted HTML.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="html">The HTML.</param>
        /// <returns>The HTML with the template.</returns>
        private string ApplyTemplate(Document doc, string html)
        {
            if (this.Template != null)
            {
                var body =
                    Regex.Match(html, "<body>(.*)</body>", RegexOptions.Singleline).Groups[1]
                        .Value.TrimEnd();

                // Read the contents of the template
                html = File.ReadAllText(this.Template);

                // Replace custom replacement strings
                if (this.Variables != null)
                {
                    foreach (var kvp in this.Variables)
                    {
                        html = html.Replace("$" + kvp.Key, kvp.Value);
                    }
                }

                // Replace standard fields
                html = html.Replace("$title", doc.Title);
                html = html.Replace("$keywords", doc.Keywords);
                html = html.Replace("$description", doc.Description);
                html = html.Replace("$date", doc.Date);
                html = html.Replace("$version", doc.Version);
                html = html.Replace("$revision", doc.Revision);
                html = html.Replace("$creator", doc.Creator);
                html = html.Replace("$category", doc.Category);
                html = html.Replace("$subject", doc.Subject);

                // Replace with the body of the generated html
                html = html.Replace("$body", body);
            }

            return html;
        }

        /// <summary>
        /// Writes the specified document as HTML to the specified <see cref="XmlWriter"/>.
        /// </summary>
        /// <param name="doc">The document.</param>
        private void Format(Document doc, XmlWriter context)
        {
            this.Source = doc;
            this.WriteStartDocument(context);
            this.WriteBlocks(this.Source.Blocks, context);

            if (!string.IsNullOrEmpty(this.Footer))
            {
                context.WriteElementString("footer", this.Footer);
            }

            context.WriteEndElement();
            context.WriteEndElement();
            context.Flush();
        }

        protected virtual string ResolveLink(string url)
        {
            if (url != null && !url.StartsWith("http"))
            {
                // internal link
                var match2 = Regex.Match(url, "(?:(.*):)?(.*)");
                var space = match2.Groups[1].Value.Trim();
                var id = match2.Groups[2].Value.Trim();
                string format;
                if (this.InternalLinkSpaces.TryGetValue(space, out format))
                {
                    url = string.Format(format, id);
                }
                else
                {
                    if (string.IsNullOrEmpty(space))
                    {
                        url = string.Format(this.LocalLinkFormatString, id);
                    }
                    else
                    {
                        url = string.Format(this.SpaceLinkFormatString, space, id);
                    }
                }
            }

            return url;
        }

        protected override void Write(Header header, XmlWriter context)
        {
            context.WriteStartElement("h" + header.Level);
            this.WriteAttributes(header, context);
            this.WriteInlines(header.Content, context);
            context.WriteEndElement();
        }

        protected override void Write(TableOfContents toc, XmlWriter context)
        {
            bool preToc = true;
            int level = 0;
            int index = 0;
            foreach (var block in this.Source.Blocks)
            {
                if (block == toc) preToc = false;
                var header = block as Header;
                if (header == null || header.Level > toc.Levels || preToc)
                {
                    continue;
                }

                while (level < header.Level)
                {
                    context.WriteStartElement("ul");
                    level++;
                }

                if (level > header.Level)
                {
                    context.WriteEndElement();
                    level--;
                }

                if (header.ID == null)
                {
                    header.ID = string.Format("{0}", index++);
                }

                context.WriteStartElement("li");
                context.WriteStartElement("a");
                context.WriteAttributeString("href", "#" + header.ID);
                this.WriteInlines(header.Content, context);
                context.WriteEndElement();
                context.WriteEndElement(); // li
            }

            while (level > 0)
            {
                context.WriteEndElement();
                level--;
            }
        }

        private void WriteAttributes(Element e, XmlWriter context)
        {
            if (e.ID != null)
            {
                context.WriteAttributeString("id", e.ID);
            }

            if (e.Class != null)
            {
                context.WriteAttributeString("class", e.Class);
            }

            if (e.Title != null)
            {
                context.WriteAttributeString("title", e.Title);
            }
        }

        protected override void Write(Paragraph paragraph, XmlWriter context)
        {
            context.WriteStartElement("p");
            this.WriteAttributes(paragraph, context);
            this.WriteInlines(paragraph.Content, context);
            context.WriteEndElement();
        }

        protected override void Write(Section section, XmlWriter context)
        {
            context.WriteStartElement("div");
            this.WriteAttributes(section, context);
            this.WriteBlocks(section.Blocks, context);
            context.WriteEndElement();
        }

        protected override void Write(UnorderedList list, XmlWriter context)
        {
            context.WriteStartElement("ul");
            this.WriteAttributes(list, context);
            foreach (var item in list.Items)
            {
                Write(item, context);
            }

            context.WriteEndElement();
        }

        protected override void Write(OrderedList list, XmlWriter context)
        {
            context.WriteStartElement("ol");
            this.WriteAttributes(list, context);
            foreach (var item in list.Items)
            {
                Write(item, context);
            }

            context.WriteEndElement();
        }

        protected override void Write(ListItem item, XmlWriter context)
        {
            context.WriteStartElement("li");
            base.Write(item, context);

            // todo: write nested list
            context.WriteEndElement();
        }

        protected override void Write(Table table, XmlWriter context)
        {
            context.WriteStartElement("table");
            this.WriteAttributes(table, context);
            base.Write(table, context);
            context.WriteEndElement();
        }

        protected override void Write(TableRow row, XmlWriter context)
        {
            context.WriteStartElement("tr");
            base.Write(row, context);
            context.WriteEndElement();
        }

        protected override void Write(TableCell cell, XmlWriter context)
        {
            context.WriteStartElement(cell is TableHeaderCell ? "th" : "td");

            if (cell.HorizontalAlignment == HorizontalAlignment.Center)
            {
                context.WriteAttributeString("class", "c");
            }

            if (cell.HorizontalAlignment == HorizontalAlignment.Right)
            {
                context.WriteAttributeString("class", "r");
            }

            if (cell.ColumnSpan > 1)
            {
                context.WriteAttributeString("colspan", cell.ColumnSpan.ToString(CultureInfo.InvariantCulture));
            }

            if (cell.RowSpan > 1)
            {
                context.WriteAttributeString("rowspan", cell.RowSpan.ToString(CultureInfo.InvariantCulture));
            }

            if (cell.Blocks.Count == 1 && cell.Blocks[0] is Paragraph)
            {
                var p = cell.Blocks[0] as Paragraph;
                this.WriteInlines(p.Content, context);
            }
            else
            {
                base.Write(cell, context);
            }

            context.WriteEndElement();
        }

        protected override void Write(Quote quote, XmlWriter context)
        {
            context.WriteStartElement("blockquote");
            this.WriteAttributes(quote, context);
            context.WriteStartElement("p");
            this.WriteInlines(quote.Content, context);
            context.WriteEndElement();
            context.WriteEndElement();
        }

        protected override void Write(CodeBlock codeBlock, XmlWriter context)
        {
            context.WriteStartElement("pre");
            this.WriteAttributes(codeBlock, context);
            context.WriteRaw("<code>");
            context.WriteRaw(HighlightSyntax(Encode(codeBlock.Text), codeBlock.Language));
            context.WriteRaw("</code>");
            context.WriteEndElement();
        }

        protected override void Write(HorizontalRuler ruler, XmlWriter context)
        {
            context.WriteStartElement("hr");
            this.WriteAttributes(ruler, context);
            context.WriteEndElement();
        }

        protected override void Write(NonBreakingSpace nbsp, XmlWriter context)
        {
            context.WriteRaw("&nbsp;");
        }

        protected override void Write(Run run, XmlWriter context)
        {
            if (run.Text == null)
            {
                return;
            }

            var text = Encode(run.Text);
            context.WriteRaw(text);
        }

        protected override void Write(Span span, XmlWriter context)
        {
            context.WriteStartElement("span");
            this.WriteAttributes(span, context);
            this.WriteInlines(span.Content, context);
            context.WriteEndElement();
        }

        protected override void Write(Strong strong, XmlWriter context)
        {
            context.WriteStartElement("strong");
            this.WriteAttributes(strong, context);
            this.WriteInlines(strong.Content, context);
            context.WriteEndElement();
        }

        protected override void Write(Emphasized em, XmlWriter context)
        {
            context.WriteStartElement("em");
            this.WriteAttributes(em, context);
            this.WriteInlines(em.Content, context);
            context.WriteEndElement();
        }

        protected override void Write(LineBreak linebreak, XmlWriter context)
        {
            context.WriteStartElement("br");
            this.WriteAttributes(linebreak, context);
            context.WriteEndElement();
        }

        protected override void Write(InlineCode inlineCode, XmlWriter context)
        {
            context.WriteStartElement("code");
            this.WriteAttributes(inlineCode, context);
            context.WriteRaw(HighlightSyntax(Encode(inlineCode.Code), inlineCode.Language));
            context.WriteEndElement();
        }

        protected override void Write(Symbol symbol, XmlWriter context)
        {
            var file = SymbolResolver.Decode(symbol.Name);
            var dir = this.SymbolDirectory ?? string.Empty;
            if (dir.Length > 0 && !dir.EndsWith("/"))
            {
                dir += "/";
            }

            var path = dir + file;

            context.WriteStartElement("img");
            this.WriteAttributes(symbol, context);
            context.WriteAttributeString("src", path);
            context.WriteAttributeString("alt", string.Empty);
            context.WriteEndElement();
        }

        protected override void Write(Anchor anchor, XmlWriter context)
        {
            context.WriteStartElement("a");
            this.WriteAttributes(anchor, context);
            context.WriteAttributeString("name", anchor.Name);
            context.WriteEndElement();
        }

        protected override void Write(Hyperlink hyperlink, XmlWriter context)
        {
            context.WriteStartElement("a");
            this.WriteAttributes(hyperlink, context);
            context.WriteAttributeString("href", this.ResolveLink(hyperlink.Url));
            this.WriteInlines(hyperlink.Content, context);
            context.WriteEndElement();
        }

        private int equationCounter = 1;

        /// <summary>
        ///     Creates the directory if missing.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void CreateDirectoryIfMissing(string path)
        {
            path = System.IO.Path.GetFullPath(path);
            var current = string.Empty;
            foreach (var d in path.Split('\\'))
            {
                current = Path.Combine(current, d);
                if (current.EndsWith(":"))
                {
                    current += "\\";
                }

                if (!Directory.Exists(current))
                {
                    Directory.CreateDirectory(current);
                }
            }
        }

        protected override void Write(Equation e, XmlWriter context)
        {
            var alt = string.Format("Equation {0}", equationCounter);
            var fileName = string.Format(this.EquationFormatString, this.EquationFilePrefix, this.equationCounter);
            this.equationCounter++;

            // Generate png
            var filePath = Path.Combine(this.OutputDirectory, fileName);
            CreateDirectoryIfMissing(Path.GetDirectoryName(filePath));

            var tex = "$" + e.Content + "$";
            bool exists = false;
            if (this.CacheEquations)
            {
                var texFilePath = Path.ChangeExtension(filePath, ".tex");
                if (File.Exists(filePath))
                {
                    if (File.Exists(texFilePath))
                    {
                        var current = File.ReadAllText(texFilePath);
                        if (current == tex)
                        {
                            exists = true;
                        }
                    }
                }

                File.WriteAllText(texFilePath, tex);
            }
            if (!exists)
            {
                var tc = new TexConverter();
                if (!tc.Convert(tex, filePath))
                {
                    // invalid tex
                    return;
                }
            }

            context.WriteStartElement("img");
            this.WriteAttributes(e, context);
            context.WriteAttributeString("src", fileName);
            context.WriteAttributeString("alt", alt);
            context.WriteEndElement();
        }

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <remarks>See <a href="http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path">Stack overflow</a></remarks>
        private static string MakeRelativePath(string fromPath, string toPath)
        {
            if (string.IsNullOrEmpty(fromPath))
            {
                throw new ArgumentNullException("fromPath");
            }

            if (string.IsNullOrEmpty(toPath))
            {
                throw new ArgumentNullException("toPath");
            }

            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', System.IO.Path.DirectorySeparatorChar);
        }

        protected override void Write(Image image, XmlWriter context)
        {
            if (this.ImageWrapperClass != null)
            {
                context.WriteStartElement("div");
                context.WriteAttributeString("class", this.ImageWrapperClass);
            }

            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                context.WriteStartElement("a");
                context.WriteAttributeString("href", image.Link);
            }

            var src = image.Source;

            // TODO: copy image??
            // src = MakeRelativePath(src, this.OutputDirectory);

            context.WriteStartElement("img");
            this.WriteAttributes(image, context);
            context.WriteAttributeString("src", src);
            context.WriteAttributeString("alt", image.AlternateText);
            context.WriteEndElement();
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                context.WriteEndElement();
            }

            if (this.ImageCaptionClass != null && image.AlternateText != null)
            {
                context.WriteStartElement("div");
                context.WriteAttributeString("class", this.ImageCaptionClass);
                context.WriteString(image.AlternateText);
                context.WriteEndElement();
            }

            if (this.ImageWrapperClass != null)
            {
                context.WriteEndElement();
            }
        }

        private static string Encode(string text)
        {
            foreach (var e in Encodings)
            {
                text = text.Replace(e.Key, e.Value);
            }

            return text;
        }

        private static string HighlightSyntax(string text, Language language)
        {
            switch (language)
            {
                case Language.Cs:
                    return CsSyntaxHighlighter.Highlight(text);
                case Language.Xml:
                    return XmlSyntaxHighlighter.Highlight(text);
                default:
                    return text;
            }
        }

        private void WriteStartDocument(XmlWriter context)
        {
            // w = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented};
            context.WriteStartDocument();
            context.WriteDocType(
                "html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);
            context.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
            context.WriteStartElement("head");
            if (!string.IsNullOrWhiteSpace(this.Source.Title))
            {
                context.WriteElementString("title", this.Source.Title);
            }

            if (!string.IsNullOrWhiteSpace(this.Source.Description))
            {
                context.WriteStartElement("meta");
                context.WriteAttributeString("name", "description");
                context.WriteAttributeString("content", this.Source.Description);
                context.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(this.Source.Keywords))
            {
                context.WriteStartElement("meta");
                context.WriteAttributeString("name", "keywords");
                context.WriteAttributeString("content", this.Source.Keywords);
                context.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(this.Css))
            {
                context.WriteStartElement("link");
                context.WriteAttributeString("rel", "stylesheet");
                context.WriteAttributeString("type", "text/css");
                context.WriteAttributeString("href", this.Css);
                context.WriteEndElement();
            }

            context.WriteEndElement();
            context.WriteStartElement("body");
        }
    }
}