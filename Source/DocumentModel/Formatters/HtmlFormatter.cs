using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace LynxToolkit.Documents
{
    public class HtmlFormatter : DocumentFormatter
    {
        public string Stylesheet { get; set; }
        public string Footer { get; set; }
        public string SymbolDirectory { get; set; }

        public XmlDocument Document { get; private set; }

        private XmlWriter w;
        private MemoryStream ms;

        public HtmlFormatter(Document doc)
            : base(doc)
        {
            ms = new MemoryStream();
            var settings = new XmlWriterSettings() { OmitXmlDeclaration = true, Encoding = Encoding.UTF8, Indent = true };
            w = XmlWriter.Create(ms, settings);
        }

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

            if (!string.IsNullOrWhiteSpace(Stylesheet))
            {
                w.WriteStartElement("link");
                w.WriteAttributeString("rel", "stylesheet");
                w.WriteAttributeString("type", "text/css");
                w.WriteAttributeString("href", Stylesheet);
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
            if (!string.IsNullOrEmpty(this.Footer))
            {
                w.WriteElementString("footer", this.Footer);
            }
            w.WriteEndElement();
            w.WriteEndElement();
            w.Flush();
        }

        public static string Format(Document doc, string styleSheet = null, string symbolDirectory = null)
        {
            var wf = new HtmlFormatter(doc) { Stylesheet = styleSheet, SymbolDirectory = symbolDirectory };
            wf.FormatCore();
            return wf.ToString();
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

        private static Dictionary<string, string> encodings;

        static HtmlFormatter()
        {
            encodings = new Dictionary<string, string>();
            encodings.Add("&", "&amp;");
            encodings.Add(">", "&gt;");
            encodings.Add("<", "&lt;");
        }

        private static string Encode(string text)
        {
            foreach (var e in encodings)
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
            var dir = SymbolDirectory ?? string.Empty;
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

        protected override void Write(Hyperlink hyperlink)
        {
            w.WriteStartElement("a");
            w.WriteAttributeString("href", hyperlink.Url);
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