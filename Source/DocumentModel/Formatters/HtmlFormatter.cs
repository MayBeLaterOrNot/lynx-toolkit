namespace LynxToolkit.Documents
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Xml;
    using System.Text.RegularExpressions;

    public class HtmlFormatterOptions : DocumentFormatterOptions
    {
        /// <summary>
        /// Gets or sets the format string for local links (without space or http prefix).
        /// </summary>
        /// <remarks>{0} will be replaced by the local link reference.</remarks>
        public string LocalLinkFormatString { get; set; }
        
        /// <summary>
        /// Gets or sets the format string for local links containing a space (e.g. "space:id").
        /// </summary>
        /// <remarks>{0} will be replaced by the space, and {1} will be replaced by the id.</remarks>
        public string SpaceLinkFormatString { get; set; }

        /// <summary>
        /// Gets the internal link space dictionary.
        /// </summary>
        public Dictionary<string, string> InternalLinkSpaces { get; private set; }

        /// <summary>
        /// Gets or sets the template document (this will override the Css style sheet).
        /// </summary>
        public string Template { get; set; }

        /// <summary>
        /// Gets or sets the cascading style sheet.
        /// </summary>
        public string Css { get; set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        public string Footer { get; set; }

        public HtmlFormatterOptions()
        {
            LocalLinkFormatString = "{0}.html";
            SpaceLinkFormatString = "{0}/{1}.html";

            this.InternalLinkSpaces = new Dictionary<string, string>
                    {
                        { "youtube", @"http://www.youtube.com/watch?v={0}" },
                        { "vimeo", @"http://vimeo.com/{0}" },
                        { "google", @"http://www.google.com/?q={0}" }
                    };
        }
    }

    public class HtmlFormatter : DocumentFormatter
    {
        private readonly XmlWriter w;
        private readonly MemoryStream ms;

        protected HtmlFormatter(Document doc, HtmlFormatterOptions options)
            : base(doc)
        {
            this.Options = options ?? new HtmlFormatterOptions();
            ms = new MemoryStream();
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true };
            w = XmlWriter.Create(ms, settings);
        }

        protected HtmlFormatterOptions Options { get; private set; }

        private void WriteStartDocument()
        {
            //w = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented};
            w.WriteStartDocument();
            w.WriteDocType("html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);
            w.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
            w.WriteStartElement("head");
            if (!string.IsNullOrWhiteSpace(doc.Title))
                w.WriteElementString("title", doc.Title);

            if (!string.IsNullOrWhiteSpace(doc.Description))
            {
                w.WriteStartElement("meta");
                w.WriteAttributeString("name", "description");
                w.WriteAttributeString("content", doc.Description);
                w.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(doc.Keywords))
            {
                w.WriteStartElement("meta");
                w.WriteAttributeString("name", "keywords");
                w.WriteAttributeString("content", doc.Keywords);
                w.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(Options.Css))
            {
                w.WriteStartElement("link");
                w.WriteAttributeString("rel", "stylesheet");
                w.WriteAttributeString("type", "text/css");
                w.WriteAttributeString("href", Options.Css);
                w.WriteEndElement();
            }

            w.WriteEndElement();
            w.WriteStartElement("body");
        }

        public override string ToString()
        {
            if (ms.Length == 0) this.FormatCore();
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public override void FormatCore()
        {
            this.WriteStartDocument();
            base.FormatCore();
            if (!string.IsNullOrEmpty(Options.Footer))
            {
                w.WriteElementString("footer", Options.Footer);
            }
            w.WriteEndElement();
            w.WriteEndElement();
            w.Flush();
        }

        public static string Format(Document doc, HtmlFormatterOptions options = null)
        {
            if (options == null)
            {
                options = new HtmlFormatterOptions();
            }

            var wf = new HtmlFormatter(doc, options);
            wf.FormatCore();
            var html = wf.ToString();

            if (options.Template != null)
            {
                var body = Regex.Match(html, "<body>(.*)</body>", RegexOptions.Compiled | RegexOptions.Singleline).Groups[1].Value.TrimEnd();

                // Read the contents of the template
                html = File.ReadAllText(options.Template);
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

        protected override void Write(Header header)
        {
            w.WriteStartElement("h" + header.Level);
            WriteInlines(header.Content);
            w.WriteEndElement();
        }

        protected override void Write(Paragraph paragraph)
        {
            w.WriteStartElement("p");
            WriteInlines(paragraph.Content);
            w.WriteEndElement();
        }

        protected override void Write(List list)
        {
            w.WriteStartElement("ul");
            foreach (var item in list.Items)
                Write(item);
            w.WriteEndElement();
        }

        protected override void Write(OrderedList list)
        {
            w.WriteStartElement("ol");
            foreach (var item in list.Items)
                Write(item);
            w.WriteEndElement();
        }

        protected override void Write(ListItem item)
        {
            w.WriteStartElement("li");
            base.Write(item);
            // todo: write nested list
            w.WriteEndElement();
        }

        protected override void Write(Table table)
        {
            w.WriteStartElement("table");
            base.Write(table);
            w.WriteEndElement();
        }

        protected override void Write(TableRow row)
        {
            w.WriteStartElement("tr");
            base.Write(row);
            w.WriteEndElement();
        }

        protected override void Write(TableCell cell)
        {
            w.WriteStartElement(cell is TableHeaderCell ? "th" : "td");
            if (cell.HorizontalAlignment == HorizontalAlignment.Center)
                w.WriteAttributeString("class", "c");
            if (cell.HorizontalAlignment == HorizontalAlignment.Right)
                w.WriteAttributeString("class", "r");
            base.Write(cell);
            w.WriteEndElement();
        }

        protected override void Write(Quote quote)
        {
            w.WriteStartElement("blockquote");
            w.WriteStartElement("p");
            WriteInlines(quote.Content);
            w.WriteEndElement();
            w.WriteEndElement();
        }

        protected override void Write(CodeBlock codeBlock)
        {
            w.WriteStartElement("pre");
            w.WriteRaw("<code>");
            w.WriteRaw(codeBlock.Text);
            w.WriteRaw("</code>");
            w.WriteEndElement();
        }

        protected override void Write(HorizontalRuler ruler)
        {
            w.WriteStartElement("hr");
            w.WriteEndElement();
        }

        protected override void Write(Run run)
        {
            if (run.Text == null)
            {
                return;
            }

            var text = Encode(run.Text);
            w.WriteRaw(text);
        }

        private static readonly Dictionary<string, string> Encodings;

        static HtmlFormatter()
        {
            Encodings = new Dictionary<string, string>();
            Encodings.Add("&", "&amp;");
            Encodings.Add(">", "&gt;");
            Encodings.Add("<", "&lt;");
        }

        private static string Encode(string text)
        {
            foreach (var e in Encodings)
            {
                text = text.Replace(e.Key, e.Value);
            }
            return text;
        }

        protected override void Write(Strong strong)
        {
            w.WriteStartElement("strong");
            WriteInlines(strong.Content);
            w.WriteEndElement();
        }

        protected override void Write(Emphasized em)
        {
            w.WriteStartElement("em");
            WriteInlines(em.Content);
            w.WriteEndElement();
        }

        protected override void Write(LineBreak linebreak)
        {
            w.WriteStartElement("br");
            w.WriteEndElement();
        }

        protected override void Write(InlineCode inlineCode)
        {
            w.WriteStartElement("code");
            w.WriteRaw(inlineCode.Text);
            w.WriteEndElement();
        }

        protected override void Write(Symbol symbol)
        {
            var file = SymbolResolver.Decode(symbol.Name);
            var dir = Options.SymbolDirectory ?? string.Empty;
            if (dir.Length > 0 && !dir.EndsWith("/"))
            {
                dir += "/";
            }
            var path = dir + file;

            w.WriteStartElement("img");
            w.WriteAttributeString("src", path);
            w.WriteAttributeString("alt", string.Empty);
            w.WriteEndElement();
        }

        protected override void Write(Anchor anchor)
        {
            w.WriteStartElement("a");
            w.WriteAttributeString("name", anchor.Name);
            w.WriteEndElement();
        }

        protected virtual string ResolveLink(string url)
        {
            if (!url.StartsWith("http"))
            {
                // internal link
                var match2 = Regex.Match(url, "(?:(.*):)?(.*)");
                var space = match2.Groups[1].Value.Trim();
                var id = match2.Groups[2].Value.Trim();
                string format;
                if (this.Options.InternalLinkSpaces.TryGetValue(space, out format))
                {
                    url = string.Format(format, id);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(space))
                    {
                        url = string.Format(this.Options.LocalLinkFormatString, id);
                    }
                    else
                    {
                        url = string.Format(this.Options.SpaceLinkFormatString, space, id);                        
                    }
                }
            }

            return url;
        }

        protected override void Write(Hyperlink hyperlink)
        {
            w.WriteStartElement("a");
            w.WriteAttributeString("href", this.ResolveLink(hyperlink.Url));
            if (!string.IsNullOrWhiteSpace(hyperlink.Title))
                w.WriteAttributeString("title", hyperlink.Title);
            WriteInlines(hyperlink.Content);
            w.WriteEndElement();
        }

        protected override void Write(Image image)
        {
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                w.WriteStartElement("a");
                w.WriteAttributeString("href", image.Link);
            }
            w.WriteStartElement("img");
            w.WriteAttributeString("src", image.Source);
            w.WriteAttributeString("alt", image.AlternateText);
            w.WriteEndElement();
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                w.WriteEndElement();
            }
        }
    }
}