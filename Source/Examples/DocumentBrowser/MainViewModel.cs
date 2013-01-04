using System.IO;
using System.Text;
using System.Xml.Serialization;
using LynxToolkit.Documents;

namespace DocumentBrowser
{
    using DocumentModel.Wpf;

    public class MainViewModel : Observable
    {
        public MainViewModel()
        {
            Input = File.ReadAllText("Example.wiki");
        }

        private string input;

        public string Input
        {
            get { return input; }
            set
            {
                if (SetValue(ref input, value, "Input"))
                {
                    Update();
                }
            }
        }

        private void Update()
        {
            var doc = WikiParser.Parse(Input, null, null, null);

            Wiki = OWikiFormatter.Format(doc);
            WikiCreole = CreoleFormatter.Format(doc);
            WikiMarkdown = MarkdownFormatter.Format(doc);
            WikiConfluence = ConfluenceFormatter.Format(doc);
            WikiCodeplex = CodeplexFormatter.Format(doc);
            var options = new HtmlFormatterOptions { Css = "style.css", SymbolDirectory = "images" };
            Html = HtmlFormatter.Format(doc, options);

            //File.WriteAllText("temp.txt", Html, Encoding.UTF8);
            //System.Diagnostics.Process.Start("temp.txt");

            var doc2 = HtmlParser.Parse(Html);
            Wiki2 = OWikiFormatter.Format(doc2);

            // File.WriteAllText("Index.html", Html);
            //  var docCreole = CreoleParser.Parse(WikiCreole);
            //  var docMarkdown = MarkdownParser.Parse(WikiCreole);

            var serializer = new XmlSerializer(
                typeof(Document),
                new[] {
                    typeof(BlockCollection), typeof(InlineCollection),
                    typeof(Header),typeof(Paragraph),typeof(CodeBlock), typeof(Quote),typeof(Section),typeof(HorizontalRuler),
                    typeof(UnorderedList),typeof(OrderedList),typeof(ListItem),typeof(ListItemCollection),
                    typeof(Table),typeof(TableRowCollection), typeof(TableRow),typeof(TableCellCollection), typeof(TableCell),typeof(TableHeaderCell),
                    typeof(LineBreak),typeof(Run),typeof(Emphasized),typeof(Strong),typeof(Symbol),typeof(InlineCode),typeof(Anchor),typeof(Hyperlink),typeof(Image)});

            var ms = new MemoryStream();
            serializer.Serialize(ms, doc);
            Document = Encoding.UTF8.GetString(ms.ToArray());
            FlowDocument = FlowDocumentFormatter.Format(doc, "images");
            var js = new Newtonsoft.Json.JsonSerializer();
            var s = new StringWriter();
            var w = new Newtonsoft.Json.JsonTextWriter(s);
            w.Formatting = Newtonsoft.Json.Formatting.Indented;
            js.Serialize(w, doc);
            DocumentJSON = s.GetStringBuilder().ToString();
            //var json = new ServiceStack.Text.JsonSerializer<Document>();
            //DocumentJSON = json.SerializeToString(doc);
        }

        private string document;
        public string Document
        {
            get { return document; }
            set { SetValue(ref document, value, "Document"); }
        }

        private string documentjson;
        public string DocumentJSON
        {
            get { return documentjson; }
            set { SetValue(ref documentjson, value, "DocumentJSON"); }
        }

        private string wiki;
        public string Wiki
        {
            get { return wiki; }
            set { SetValue(ref wiki, value, "Wiki"); }
        }
        private string wiki2;
        public string Wiki2
        {
            get { return wiki2; }
            set { SetValue(ref wiki2, value, "Wiki2"); }
        }

        private string wikiCreole;
        public string WikiCreole
        {
            get { return wikiCreole; }
            set { SetValue(ref wikiCreole, value, "WikiCreole"); }
        }
        private string wikiMarkdown;
        public string WikiMarkdown
        {
            get { return wikiMarkdown; }
            set { SetValue(ref wikiMarkdown, value, "WikiMarkdown"); }
        }
        private string wikiConfluence;
        public string WikiConfluence
        {
            get { return wikiConfluence; }
            set { SetValue(ref wikiConfluence, value, "WikiConfluence"); }
        }
        private string wikiCodeplex;
        public string WikiCodeplex
        {
            get { return wikiCodeplex; }
            set { SetValue(ref wikiCodeplex, value, "WikiCodeplex"); }
        }

        private string html;
        public string Html
        {
            get { return html; }
            set { SetValue(ref html, value, "Html"); }
        }


        private System.Windows.Documents.FlowDocument flowDocument;
        public System.Windows.Documents.FlowDocument FlowDocument
        {
            get { return flowDocument; }
            set { SetValue(ref flowDocument, value, "FlowDocument"); }
        }
    }
}