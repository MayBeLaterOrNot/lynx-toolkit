// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainViewModel.cs" company="Lynx">
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
//   Defines the MainViewModel type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace DocumentBrowser
{
    using System.IO;
    using System.Windows.Documents;

    using LynxToolkit.Documents;
    using LynxToolkit.Documents.Wpf;

    using Newtonsoft.Json;

    public class MainViewModel : Observable
    {
        private string document;

        private string documentjson;

        private FlowDocument flowDocument;

        private string html;

        private string input;

        private string wiki;

        private string wiki2;

        private string wikiCodeplex;

        private string wikiConfluence;

        private string wikiCreole;

        private string wikiMarkdown;

        public MainViewModel()
        {
            this.Input = File.ReadAllText("Example.wiki");
        }

        public string Document
        {
            get
            {
                return this.document;
            }

            set
            {
                this.SetValue(ref this.document, value, "Document");
            }
        }

        public string DocumentJSON
        {
            get
            {
                return this.documentjson;
            }

            set
            {
                this.SetValue(ref this.documentjson, value, "DocumentJSON");
            }
        }

        public FlowDocument FlowDocument
        {
            get
            {
                return this.flowDocument;
            }

            set
            {
                this.SetValue(ref this.flowDocument, value, "FlowDocument");
            }
        }

        public string Html
        {
            get
            {
                return this.html;
            }

            set
            {
                this.SetValue(ref this.html, value, "Html");
            }
        }

        public string Input
        {
            get
            {
                return this.input;
            }

            set
            {
                if (this.SetValue(ref this.input, value, "Input"))
                {
                    this.Update();
                }
            }
        }

        public string Wiki
        {
            get
            {
                return this.wiki;
            }

            set
            {
                this.SetValue(ref this.wiki, value, "Wiki");
            }
        }

        public string Wiki2
        {
            get
            {
                return this.wiki2;
            }

            set
            {
                this.SetValue(ref this.wiki2, value, "Wiki2");
            }
        }

        public string WikiCodeplex
        {
            get
            {
                return this.wikiCodeplex;
            }

            set
            {
                this.SetValue(ref this.wikiCodeplex, value, "WikiCodeplex");
            }
        }

        public string WikiConfluence
        {
            get
            {
                return this.wikiConfluence;
            }

            set
            {
                this.SetValue(ref this.wikiConfluence, value, "WikiConfluence");
            }
        }

        public string WikiCreole
        {
            get
            {
                return this.wikiCreole;
            }

            set
            {
                this.SetValue(ref this.wikiCreole, value, "WikiCreole");
            }
        }

        public string WikiMarkdown
        {
            get
            {
                return this.wikiMarkdown;
            }

            set
            {
                this.SetValue(ref this.wikiMarkdown, value, "WikiMarkdown");
            }
        }

        private void Update()
        {
            var doc = WikiParser.Parse(this.Input, null, null, null);

            this.Wiki = OWikiFormatter.Format(doc);
            this.WikiCreole = CreoleFormatter.Format(doc);
            this.WikiMarkdown = MarkdownFormatter.Format(doc);
            this.WikiConfluence = ConfluenceFormatter.Format(doc);
            this.WikiCodeplex = CodeplexFormatter.Format(doc);
            var options = new HtmlFormatterOptions { Css = "style.css", SymbolDirectory = "images" };
            this.Html = HtmlFormatter.Format(doc, options);

            // File.WriteAllText("temp.txt", Html, Encoding.UTF8);
            // System.Diagnostics.Process.Start("temp.txt");
            var doc2 = HtmlParser.Parse(this.Html);
            this.Wiki2 = OWikiFormatter.Format(doc2);

            // File.WriteAllText("Index.html", Html);
            // var docCreole = CreoleParser.Parse(WikiCreole);
            // var docMarkdown = MarkdownParser.Parse(WikiCreole);
            this.Document = XmlFormatter.Format(doc);
            this.FlowDocument = FlowDocumentFormatter.Format(doc, "images");
            var js = new JsonSerializer();
            var s = new StringWriter();
            var w = new JsonTextWriter(s);
            w.Formatting = Formatting.Indented;
            js.Serialize(w, doc);
            this.DocumentJSON = s.GetStringBuilder().ToString();

            // var json = new ServiceStack.Text.JsonSerializer<Document>();
            // DocumentJSON = json.SerializeToString(doc);
        }
    }
}