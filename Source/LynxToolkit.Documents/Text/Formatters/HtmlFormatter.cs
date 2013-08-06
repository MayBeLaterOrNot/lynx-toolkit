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
namespace LynxToolkit.Documents
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class HtmlFormatter : DocumentFormatter
    {
        public string SourceDirectory { get; private set; }

        public string OutputFile { get; private set; }

        public string OutputDirectory
        {
            get
            {
                return Path.GetDirectoryName(OutputFile);
            }
        }

        /// <summary>
        /// The encodings
        /// </summary>
        private static readonly Dictionary<string, string> Encodings;

        /// <summary>
        /// The output memory stream.
        /// </summary>
        private readonly MemoryStream ms;

        /// <summary>
        /// The xml writer.
        /// </summary>
        private readonly XmlWriter w;

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

            this.ms = new MemoryStream();
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true };
            this.w = XmlWriter.Create(this.ms, settings);
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
        /// Gets or sets a value indicating whether to force the output file to be written.
        /// </summary>
        /// <value><c>true</c> if [force output]; otherwise, <c>false</c>.</value>
        public bool ForceOutput { get; set; }

        /// <summary>
        /// Formats the specified document.
        /// </summary>
        /// <param name="doc">The document.</param>
        /// <param name="outputFile">The output file.</param>
        /// <returns><c>true</c> if the output file was modified, <c>false</c> otherwise</returns>
        public override bool Format(Document doc, string outputFile)
        {
            this.OutputFile = outputFile;
            this.source = doc;
            var html = this.FormatString(doc, outputFile);
            if (Utilities.IsFileModified(outputFile, html) || this.ForceOutput)
            {
                File.WriteAllText(outputFile, html, Encoding.UTF8);
                return true;
            }

            return false;
        }

        public string FormatString(Document doc, string outputFile)
        {
            this.WriteStartDocument();
            this.WriteBlocks(this.source.Blocks, null);

            if (!string.IsNullOrEmpty(this.Footer))
            {
                this.w.WriteElementString("footer", this.Footer);
            }

            this.w.WriteEndElement();
            this.w.WriteEndElement();
            this.w.Flush();

            var html = this.ToString();

            if (this.Template != null)
            {
                var body =
                    Regex.Match(html, "<body>(.*)</body>", RegexOptions.Compiled | RegexOptions.Singleline).Groups[1]
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

        public override string ToString()
        {
            return Encoding.UTF8.GetString(this.ms.ToArray());
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
                    if (string.IsNullOrWhiteSpace(space))
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

        protected override void Write(Header header, object parent)
        {
            this.w.WriteStartElement("h" + header.Level);
            this.WriteAttributes(header);
            this.WriteInlines(header.Content, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(TableOfContents toc, object parent)
        {
            bool preToc = true;
            int level = 0;
            int index = 0;
            foreach (var block in this.source.Blocks)
            {
                if (block == toc) preToc = false;
                var header = block as Header;
                if (header == null || header.Level > toc.Levels || preToc)
                {
                    continue;
                }

                while (level < header.Level)
                {
                    this.w.WriteStartElement("ul");
                    level++;
                }

                if (level > header.Level)
                {
                    this.w.WriteEndElement();
                    level--;
                }

                if (header.ID == null)
                {
                    header.ID = string.Format("{0}", index++);
                }

                this.w.WriteStartElement("li");
                this.w.WriteStartElement("a");
                this.w.WriteAttributeString("href", "#" + header.ID);
                this.WriteInlines(header.Content, parent);
                this.w.WriteEndElement();
                this.w.WriteEndElement(); // li
            }

            while (level > 0)
            {
                this.w.WriteEndElement();
                level--;
            }
        }

        private void WriteAttributes(Element e)
        {
            if (e.ID != null)
            {
                this.w.WriteAttributeString("id", e.ID);
            }

            if (e.Class != null)
            {
                this.w.WriteAttributeString("class", e.Class);
            }

            if (e.Title != null)
            {
                this.w.WriteAttributeString("title", e.Title);
            }
        }

        protected override void Write(Paragraph paragraph, object parent)
        {
            this.w.WriteStartElement("p");
            this.WriteAttributes(paragraph);
            this.WriteInlines(paragraph.Content, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(Section section, object parent)
        {
            this.w.WriteStartElement("div");
            this.WriteAttributes(section);
            this.WriteBlocks(section.Blocks, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(UnorderedList list, object parent)
        {
            this.w.WriteStartElement("ul");
            this.WriteAttributes(list);
            foreach (var item in list.Items)
            {
                Write(item, parent);
            }

            this.w.WriteEndElement();
        }

        protected override void Write(OrderedList list, object parent)
        {
            this.w.WriteStartElement("ol");
            this.WriteAttributes(list);
            foreach (var item in list.Items)
            {
                Write(item, parent);
            }

            this.w.WriteEndElement();
        }

        protected override void Write(ListItem item, object parent)
        {
            this.w.WriteStartElement("li");
            base.Write(item, parent);

            // todo: write nested list
            this.w.WriteEndElement();
        }

        protected override void Write(Table table, object parent)
        {
            this.w.WriteStartElement("table");
            this.WriteAttributes(table);
            base.Write(table, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(TableRow row, object parent)
        {
            this.w.WriteStartElement("tr");
            base.Write(row, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(TableCell cell, object parent)
        {
            this.w.WriteStartElement(cell is TableHeaderCell ? "th" : "td");

            if (cell.HorizontalAlignment == HorizontalAlignment.Center)
            {
                this.w.WriteAttributeString("class", "c");
            }

            if (cell.HorizontalAlignment == HorizontalAlignment.Right)
            {
                this.w.WriteAttributeString("class", "r");
            }

            if (cell.ColumnSpan > 1)
            {
                this.w.WriteAttributeString("colspan", cell.ColumnSpan.ToString());
            }

            if (cell.RowSpan > 1)
            {
                this.w.WriteAttributeString("rowspan", cell.RowSpan.ToString());
            }

            if (cell.Blocks.Count == 1 && cell.Blocks[0] is Paragraph)
            {
                var p = cell.Blocks[0] as Paragraph;
                this.WriteInlines(p.Content, parent);
            }
            else
            {
                base.Write(cell, parent);
            }

            this.w.WriteEndElement();
        }

        protected override void Write(Quote quote, object parent)
        {
            this.w.WriteStartElement("blockquote");
            this.WriteAttributes(quote);
            this.w.WriteStartElement("p");
            this.WriteInlines(quote.Content, parent);
            this.w.WriteEndElement();
            this.w.WriteEndElement();
        }

        protected override void Write(CodeBlock codeBlock, object parent)
        {
            this.w.WriteStartElement("pre");
            this.WriteAttributes(codeBlock);
            this.w.WriteRaw("<code>");
            this.w.WriteRaw(HighlightSyntax(Encode(codeBlock.Text), codeBlock.Language));
            this.w.WriteRaw("</code>");
            this.w.WriteEndElement();
        }

        protected override void Write(HorizontalRuler ruler, object parent)
        {
            this.w.WriteStartElement("hr");
            this.WriteAttributes(ruler);
            this.w.WriteEndElement();
        }

        protected override void Write(NonBreakingSpace nbsp, object parent)
        {
            this.w.WriteRaw("&nbsp;");
        }

        protected override void Write(Run run, object parent)
        {
            if (run.Text == null)
            {
                return;
            }

            var text = Encode(run.Text);
            this.w.WriteRaw(text);
        }

        protected override void Write(Span span, object parent)
        {
            this.w.WriteStartElement("span");
            this.WriteAttributes(span);
            this.WriteInlines(span.Content, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(Strong strong, object parent)
        {
            this.w.WriteStartElement("strong");
            this.WriteAttributes(strong);
            this.WriteInlines(strong.Content, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(Emphasized em, object parent)
        {
            this.w.WriteStartElement("em");
            this.WriteAttributes(em);
            this.WriteInlines(em.Content, parent);
            this.w.WriteEndElement();
        }

        protected override void Write(LineBreak linebreak, object parent)
        {
            this.w.WriteStartElement("br");
            this.WriteAttributes(linebreak);
            this.w.WriteEndElement();
        }

        protected override void Write(InlineCode inlineCode, object parent)
        {
            this.w.WriteStartElement("code");
            this.WriteAttributes(inlineCode);
            this.w.WriteRaw(HighlightSyntax(Encode(inlineCode.Code), inlineCode.Language));
            this.w.WriteEndElement();
        }

        protected override void Write(Symbol symbol, object parent)
        {
            var file = SymbolResolver.Decode(symbol.Name);
            var dir = this.SymbolDirectory ?? string.Empty;
            if (dir.Length > 0 && !dir.EndsWith("/"))
            {
                dir += "/";
            }

            var path = dir + file;

            this.w.WriteStartElement("img");
            this.WriteAttributes(symbol);
            this.w.WriteAttributeString("src", path);
            this.w.WriteAttributeString("alt", string.Empty);
            this.w.WriteEndElement();
        }

        protected override void Write(Anchor anchor, object parent)
        {
            this.w.WriteStartElement("a");
            this.WriteAttributes(anchor);
            this.w.WriteAttributeString("name", anchor.Name);
            this.w.WriteEndElement();
        }

        protected override void Write(Hyperlink hyperlink, object parent)
        {
            this.w.WriteStartElement("a");
            this.WriteAttributes(hyperlink);
            this.w.WriteAttributeString("href", this.ResolveLink(hyperlink.Url));
            this.WriteInlines(hyperlink.Content, parent);
            this.w.WriteEndElement();
        }

        private int equationCounter = 1;

        protected override void Write(Equation e, object parent)
        {
            var outputName = Path.GetFileNameWithoutExtension(this.OutputFile);
            var outputDir = Path.GetDirectoryName(this.OutputFile);

            var alt = string.Format("Equation {0}", equationCounter);
            var fileName = string.Format(this.EquationFormatString, outputName, this.equationCounter);
            this.equationCounter++;

            // Generate png
            var filePath = Path.Combine(outputDir, fileName);
            Utilities.CreateDirectoryIfMissing(Path.GetDirectoryName(filePath));

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

            this.w.WriteStartElement("img");
            this.WriteAttributes(e);
            this.w.WriteAttributeString("src", fileName);
            this.w.WriteAttributeString("alt", alt);
            this.w.WriteEndElement();
        }

        protected override void Write(Image image, object parent)
        {
            if (this.ImageWrapperClass != null)
            {
                this.w.WriteStartElement("div");
                this.w.WriteAttributeString("class", this.ImageWrapperClass);
            }

            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                this.w.WriteStartElement("a");
                this.w.WriteAttributeString("href", image.Link);
            }

            var src = image.Source;
            if (!string.IsNullOrEmpty(this.SourceDirectory))
            {
                src = src.Replace(this.SourceDirectory, this.OutputDirectory);
                src = Utilities.MakeRelativePath(src, this.OutputDirectory);
            }

            this.w.WriteStartElement("img");
            this.WriteAttributes(image);
            this.w.WriteAttributeString("src", src);
            this.w.WriteAttributeString("alt", image.AlternateText);
            this.w.WriteEndElement();
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                this.w.WriteEndElement();
            }

            if (this.ImageCaptionClass != null && image.AlternateText != null)
            {
                this.w.WriteStartElement("div");
                this.w.WriteAttributeString("class", this.ImageCaptionClass);
                this.w.WriteString(image.AlternateText);
                this.w.WriteEndElement();
            }

            if (this.ImageWrapperClass != null)
            {
                this.w.WriteEndElement();
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

        private void WriteStartDocument()
        {
            // w = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented};
            this.w.WriteStartDocument();
            this.w.WriteDocType(
                "html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);
            this.w.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
            this.w.WriteStartElement("head");
            if (!string.IsNullOrWhiteSpace(this.source.Title))
            {
                this.w.WriteElementString("title", this.source.Title);
            }

            if (!string.IsNullOrWhiteSpace(this.source.Description))
            {
                this.w.WriteStartElement("meta");
                this.w.WriteAttributeString("name", "description");
                this.w.WriteAttributeString("content", this.source.Description);
                this.w.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(this.source.Keywords))
            {
                this.w.WriteStartElement("meta");
                this.w.WriteAttributeString("name", "keywords");
                this.w.WriteAttributeString("content", this.source.Keywords);
                this.w.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(this.Css))
            {
                this.w.WriteStartElement("link");
                this.w.WriteAttributeString("rel", "stylesheet");
                this.w.WriteAttributeString("type", "text/css");
                this.w.WriteAttributeString("href", this.Css);
                this.w.WriteEndElement();
            }

            this.w.WriteEndElement();
            this.w.WriteStartElement("body");
        }
    }
}