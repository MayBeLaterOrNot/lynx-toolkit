namespace LynxToolkit.Documents
{
    using System;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;

    public class HtmlParser : DocumentParser
    {
        public void ParseCore(string input)
        {
            // remove the DTD header
            var inputHtml = input.Substring(input.IndexOf("<html"));

            // remove the namespace
            inputHtml = inputHtml.Replace(@" xmlns=""http://www.w3.org/1999/xhtml""", "");
            //XNamespace xhtml = "http://www.w3.org/1999/xhtml/";

            //var ms = new MemoryStream(Encoding.UTF8.GetBytes(inputHtml));
            //var reader = XmlReader.Create(ms);
            //var xdoc = XDocument.Load(reader, LoadOptions.PreserveWhitespace);
            var xdoc = XDocument.Parse(inputHtml, LoadOptions.PreserveWhitespace);

            var html = xdoc.Element("html");
            if (html == null)
            {
                throw new FormatException("No <html> root element found.");
            }

            var head = html.Element("head");
            if (head != null)
            {
                doc.Description = this.GetMetaContent(head, "description");
                doc.Keywords = this.GetMetaContent(head, "keywords");
            }

            var body = html.Element("body");
            if (body == null)
            {
                throw new FormatException("No <body> element found.");
            }

            foreach (var element in body.Elements())
            {
                switch (element.Name.ToString())
                {
                    case "h1":
                    case "h2":
                    case "h3":
                    case "h4":
                    case "h5":
                        this.ParseHeader(element, int.Parse(element.Name.ToString().Substring(1)));
                        break;
                    case "p":
                        this.ParseParagraph(element);
                        break;
                    case "ul":
                        this.ParseUnorderedList(element);
                        break;
                    case "ol":
                        this.ParseOrderedList(element);
                        break;
                    case "dl":
                        this.ParseDefinitionList(element);
                        break;
                    case "pre":
                        this.ParseCode(element);
                        break;
                    case "table":
                        this.ParseTable(element);
                        break;
                    case "blockquote":
                        this.ParseQuote(element);
                        break;
                    case "hr":
                        doc.Blocks.Add(new HorizontalRuler());
                        break;
                }
            }
        }

        private void ParseQuote(XElement element)
        {
            var quote = new Quote();
            doc.Blocks.Add(quote);
        }

        private void ParseTable(XElement element)
        {
            var table = new Table();
            doc.Blocks.Add(table);
        }

        private void ParseCode(XElement element)
        {
            var code = new CodeBlock();
            code.Text = element.Value;
            doc.Blocks.Add(code);
        }

        private void ParseOrderedList(XElement element)
        {
            List list = new OrderedList();
            this.ParseListItems(element, list);
            doc.Blocks.Add(list);
        }

        private void ParseUnorderedList(XElement element)
        {
            List list = new UnorderedList();
            this.ParseListItems(element, list);
            doc.Blocks.Add(list);
        }

        private void ParseListItems(XElement element, List list)
        {
            foreach (var e in element.Elements())
            {
                if (e.Name == "li")
                {
                    var li = new ListItem();
                    this.ParseInlines(e, li.Content);
                    list.Items.Add(li);
                }
            }
        }

        private void ParseDefinitionList(XElement element)
        {
            var list = new DefinitionList();
            foreach (var e in element.Elements())
            {
                switch (e.Name.LocalName)
                {
                    case "dt":
                        var dt = new DefinitionTerm();
                        this.ParseInlines(e, dt.Content);
                        list.Items.Add(dt);
                        break;
                    case "dd":
                        var dd = new DefinitionDescription();
                        this.ParseInlines(e, dd.Content);
                        list.Items.Add(dd);
                        break;
                }
            }

            doc.Blocks.Add(list);
        }

        private void ParseParagraph(XElement element)
        {
            var p = new Paragraph();
            this.ParseInlines(element, p.Content);
            doc.Blocks.Add(p);
        }

        private void ParseHeader(XElement element, int level)
        {
            var h = new Header { Level = level };
            this.ParseInlines(element, h.Content);
            doc.Blocks.Add(h);
        }

        private void ParseInlines(XElement element, InlineCollection ic)
        {
            foreach (var node in element.Nodes())
            {
                switch (node.NodeType)
                {
                    case XmlNodeType.Text:
                        ParseText(ic, (XText)node);
                        break;
                    case XmlNodeType.Element:
                        this.ParseElement(ic, (XElement)node);
                        break;
                }
            }
        }

        private void ParseElement(InlineCollection ic, XElement e)
        {
            switch (e.Name.LocalName)
            {
                case "strong":
                    var strong = new Strong();
                    this.ParseInlines(e, strong.Content);
                    ic.Add(strong);
                    break;
                case "em":
                    var emphasized = new Emphasized();
                    this.ParseInlines(e, emphasized.Content);
                    ic.Add(emphasized);
                    break;
                case "a":
                    var href = e.Attribute("href");
                    var title = e.Attribute("title");
                    var a = new Hyperlink { Url = href != null ? href.Value : null, Title = title != null ? title.Value : null };
                    this.ParseInlines(e, a.Content);
                    ic.Add(a);
                    break;
                case "br":
                    ic.Add(new LineBreak());
                    break;
                case "img":
                    var src = e.Attribute("src");
                    var alt = e.Attribute("alt");
                    var img = new Image { Source = src != null ? src.Value : null, AlternateText = alt != null ? alt.Value : null };
                    ic.Add(img);
                    break;
                case "code":
                    var code = new InlineCode();
                    code.Code = e.Value;
                    // ParseInlines(e, code.Content);
                    ic.Add(code);
                    break;
                default:
                    var run = new Run(e.ToString());
                    ic.Add(run);
                    break;
            }
        }

        private static void ParseText(InlineCollection ic, XText textNode)
        {
            var text = textNode.Value.Trim();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var run = new Run(text);
            ic.Add(run);
        }

        private string GetMetaContent(XElement head, string name)
        {
            var keywords =
                head.Elements()
                    .FirstOrDefault(
                        e => e.Name == "meta" && e.Attribute("name") != null && e.Attribute("name").Value == name);
            if (keywords != null)
            {
                var a = keywords.Attribute("content");
                if (a != null)
                {
                    return a.Value;
                }
            }
            return null;
        }

        public static Document Parse(string input)
        {
            var p = new HtmlParser();
            p.ParseCore(input);
            return p.Document;
        }
    }
}