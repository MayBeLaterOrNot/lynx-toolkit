// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HtmlFormatter.cs" company="Lynx">
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

namespace LynxToolkit.Documents
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    public class HtmlFormatterOptions : DocumentFormatterOptions
    {
        public HtmlFormatterOptions()
        {
            this.LocalLinkFormatString = "{0}.html";
            this.SpaceLinkFormatString = "{0}/{1}.html";

            this.InternalLinkSpaces = new Dictionary<string, string>
                                          {
                                              {
                                                  "youtube", 
                                                  @"http://www.youtube.com/watch?v={0}"
                                              }, 
                                              { "vimeo", @"http://vimeo.com/{0}" }, 
                                              { "google", @"http://www.google.com/?q={0}" }
                                          };
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
        ///     Gets or sets the template document (this will override the Css style sheet).
        /// </summary>
        public string Template { get; set; }
    }

    public class HtmlFormatter : DocumentFormatter
    {
        private static readonly Dictionary<string, string> Encodings;

        private readonly MemoryStream ms;

        private readonly XmlWriter w;

        static HtmlFormatter()
        {
            Encodings = new Dictionary<string, string>();
            Encodings.Add("&", "&amp;");
            Encodings.Add(">", "&gt;");
            Encodings.Add("<", "&lt;");
        }

        protected HtmlFormatter(Document doc, HtmlFormatterOptions options)
            : base(doc)
        {
            this.Options = options ?? new HtmlFormatterOptions();
            this.ms = new MemoryStream();
            var settings = new XmlWriterSettings { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true };
            this.w = XmlWriter.Create(this.ms, settings);
        }

        protected HtmlFormatterOptions Options { get; private set; }

        public static string Format(Document doc, HtmlFormatterOptions options = null)
        {
            if (options == null)
            {
                options = new HtmlFormatterOptions();
            }

            var wf = new HtmlFormatter(doc, options);
            wf.Format();
            var html = wf.ToString();

            if (options.Template != null)
            {
                var body =
                    Regex.Match(html, "<body>(.*)</body>", RegexOptions.Compiled | RegexOptions.Singleline).Groups[1]
                        .Value.TrimEnd();

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

        public override void Format()
        {
            this.WriteStartDocument();
            base.Format();
            if (!string.IsNullOrEmpty(this.Options.Footer))
            {
                this.w.WriteElementString("footer", this.Options.Footer);
            }

            this.w.WriteEndElement();
            this.w.WriteEndElement();
            this.w.Flush();
        }

        public override string ToString()
        {
            if (this.ms.Length == 0)
            {
                this.Format();
            }

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

        protected override void Write(Header header)
        {
            this.w.WriteStartElement("h" + header.Level);
            this.WriteAttributes(header);
            this.WriteInlines(header.Content);
            this.w.WriteEndElement();
        }

        protected override void Write(TableOfContents toc)
        {
            bool preToc = true;
            int level = 0;
            int index = 0;
            foreach (var block in doc.Blocks)
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
                this.WriteInlines(header.Content);
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

        protected override void Write(Paragraph paragraph)
        {
            this.w.WriteStartElement("p");
            this.WriteAttributes(paragraph);
            this.WriteInlines(paragraph.Content);
            this.w.WriteEndElement();
        }

        protected override void Write(UnorderedList list)
        {
            this.w.WriteStartElement("ul");
            this.WriteAttributes(list);
            foreach (var item in list.Items)
            {
                Write(item);
            }

            this.w.WriteEndElement();
        }

        protected override void Write(OrderedList list)
        {
            this.w.WriteStartElement("ol");
            this.WriteAttributes(list);
            foreach (var item in list.Items)
            {
                Write(item);
            }

            this.w.WriteEndElement();
        }

        protected override void Write(ListItem item)
        {
            this.w.WriteStartElement("li");
            base.Write(item);

            // todo: write nested list
            this.w.WriteEndElement();
        }

        protected override void Write(Table table)
        {
            this.w.WriteStartElement("table");
            this.WriteAttributes(table);
            base.Write(table);
            this.w.WriteEndElement();
        }

        protected override void Write(TableRow row)
        {
            this.w.WriteStartElement("tr");
            base.Write(row);
            this.w.WriteEndElement();
        }

        protected override void Write(TableCell cell)
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

            base.Write(cell);
            this.w.WriteEndElement();
        }

        protected override void Write(Quote quote)
        {
            this.w.WriteStartElement("blockquote");
            this.WriteAttributes(quote);
            this.w.WriteStartElement("p");
            this.WriteInlines(quote.Content);
            this.w.WriteEndElement();
            this.w.WriteEndElement();
        }

        protected override void Write(CodeBlock codeBlock)
        {
            this.w.WriteStartElement("pre");
            this.WriteAttributes(codeBlock);
            this.w.WriteRaw("<code>");
            this.w.WriteRaw(codeBlock.Text);
            this.w.WriteRaw("</code>");
            this.w.WriteEndElement();
        }

        protected override void Write(HorizontalRuler ruler)
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

        protected override void Write(Strong strong, object parent)
        {
            this.w.WriteStartElement("strong");
            this.WriteAttributes(strong);
            this.WriteInlines(strong.Content);
            this.w.WriteEndElement();
        }

        protected override void Write(Emphasized em, object parent)
        {
            this.w.WriteStartElement("em");
            this.WriteAttributes(em);
            this.WriteInlines(em.Content);
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
            this.w.WriteRaw(inlineCode.Code);
            this.w.WriteEndElement();
        }

        protected override void Write(Symbol symbol, object parent)
        {
            var file = SymbolResolver.Decode(symbol.Name);
            var dir = this.Options.SymbolDirectory ?? string.Empty;
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
            this.WriteInlines(hyperlink.Content);
            this.w.WriteEndElement();
        }

        protected override void Write(Image image, object parent)
        {
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                this.w.WriteStartElement("a");
                this.w.WriteAttributeString("href", image.Link);
            }

            this.w.WriteStartElement("img");
            this.WriteAttributes(image);
            this.w.WriteAttributeString("src", image.Source);
            this.w.WriteAttributeString("alt", image.AlternateText);
            this.w.WriteEndElement();
            if (!string.IsNullOrWhiteSpace(image.Link))
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

        private void WriteStartDocument()
        {
            // w = new XmlTextWriter(ms, Encoding.UTF8) { Formatting = Formatting.Indented};
            this.w.WriteStartDocument();
            this.w.WriteDocType(
                "html", "-//W3C//DTD XHTML 1.0 Strict//EN", "http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd", null);
            this.w.WriteStartElement("html", "http://www.w3.org/1999/xhtml");
            this.w.WriteStartElement("head");
            if (!string.IsNullOrWhiteSpace(this.doc.Title))
            {
                this.w.WriteElementString("title", this.doc.Title);
            }

            if (!string.IsNullOrWhiteSpace(this.doc.Description))
            {
                this.w.WriteStartElement("meta");
                this.w.WriteAttributeString("name", "description");
                this.w.WriteAttributeString("content", this.doc.Description);
                this.w.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(this.doc.Keywords))
            {
                this.w.WriteStartElement("meta");
                this.w.WriteAttributeString("name", "keywords");
                this.w.WriteAttributeString("content", this.doc.Keywords);
                this.w.WriteEndElement();
            }

            if (!string.IsNullOrWhiteSpace(this.Options.Css))
            {
                this.w.WriteStartElement("link");
                this.w.WriteAttributeString("rel", "stylesheet");
                this.w.WriteAttributeString("type", "text/css");
                this.w.WriteAttributeString("href", this.Options.Css);
                this.w.WriteEndElement();
            }

            this.w.WriteEndElement();
            this.w.WriteStartElement("body");
        }
    }
}