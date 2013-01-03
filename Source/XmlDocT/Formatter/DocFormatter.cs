namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using LynxToolkit.Documents;

    public class DocFormatter
    {
        public string OutputDirectory { get; private set; }

        public bool SinglePage { get; private set; }

        public string Format { get; private set; }
        public string StyleSheet { get; private set; }
        private Document doc { get; set; }
        private Model Model { get; set; }

        //public void Save(string outputDirectory, string fileName)
        //{
        //    string docPath = Path.Combine(outputDirectory, Path.ChangeExtension(fileName, ".txt"));
        //    Utilities.CreateDirectoryIfMissing(outputDirectory);
        //    Console.WriteLine("Output:             {0}", Path.GetFileName(docPath));
        //}
        void CreatePage()
        {
            if (doc != null) return;
            doc = new Document();
        }

        void WritePage(string fileName, string title, string description)
        {
            if (SinglePage)
                return;

            doc.Title = title;
            doc.Description = description;

            var name = fileName.Replace('<', '~').Replace('>', '~');

            var path = Path.Combine(this.OutputDirectory, name + ".html");
            var html = HtmlFormatter.Format(doc, StyleSheet);
            File.WriteAllText(path, html);

            doc = null;
        }

        public static void CreatePages(Model model, string outputdir, string format, string styleSheet, bool singlePage)
        {
            var f = new DocFormatter { Model = model, OutputDirectory = outputdir, Format = format, SinglePage = singlePage, StyleSheet = styleSheet };

            f.CreateNamespacesPage(model);
            foreach (var ns in model.Namespaces.Values.OrderBy(ns => ns.Name))
            {
                f.CreateNamespacePage(ns);
                foreach (var t in ns.Types)
                {
                    f.CreateTypePage(t);
                    foreach (var m in t.Properties) f.CreateMethodPage(m);
                    foreach (var m in t.Methods) f.CreateMethodPage(m);
                    foreach (var m in t.Constructors) f.CreateMethodPage(m);
                    foreach (var m in t.Events) f.CreateMethodPage(m);
                }
            }
            if (singlePage)
            {
                f.SinglePage = false;
                f.WritePage("Index", null, null);
            }
        }

        private void CreateTypePage(TypeModel c)
        {
            CreatePage();
            AddHeader(c.GetTitle(), 1);
            this.AddText(c.Description);

            AddHeader("Inheritance hierarchy", 2);
            var p = new Paragraph();
            var prefix = "";
            foreach (var t in c.InheritedTypes)
            {
                p.Content.Add(new Run(prefix));
                p.Content.Add(this.CreateTypeLink(t, t == c.Type));
                p.Content.Add(new LineBreak());
                prefix += "  ";
            }
            foreach (var t in c.DerivedTypes)
            {
                p.Content.Add(new Run(prefix));
                p.Content.Add(CreateTypeLink(t.Type));
                p.Content.Add(new LineBreak());
            }
            doc.Blocks.Add(p);

            this.AddNamespaceInfo(c.Type);

            var syntax = c.GetSyntax();
            if (syntax != null)
            {
                AddHeader("Syntax", 2);
                doc.Blocks.Add(new CodeBlock { Text = syntax });
            }

            this.AddTable("Constructors", c.Constructors, c.Type);
            this.AddTable("Properties", c.Properties, c.Type);
            this.AddTable("Methods", c.Methods, c.Type);
            this.AddRemarks(c);
            this.AddExamples(c);
            WritePage(c.GetFileName(), c.GetPageTitle(), c.Description);
        }

        private void CreateMethodPage(Content c)
        {
            CreatePage();
            AddHeader(c.GetTitle(), 1);
            this.AddText(c.Description);

            this.AddNamespaceInfo(c.Info.DeclaringType);

            var syntax = c.GetSyntax();
            if (syntax != null)
            {
                AddHeader("Syntax", 2);
                doc.Blocks.Add(new CodeBlock { Text = syntax });
            }

            this.AddRemarks(c);
            this.AddExamples(c);

            this.AddSeeAlso(c.Info.DeclaringType);

            WritePage(c.GetFileName(), c.GetPageTitle(), c.Description);
        }

        private void AddRemarks(Content c)
        {
            if (!string.IsNullOrWhiteSpace(c.Remarks))
            {
                this.AddHeader("Remarks", 2);
                this.AddText(c.Remarks);
            }
        }

        private void AddExamples(Content c)
        {
            if (!string.IsNullOrWhiteSpace(c.Example))
            {
                this.AddHeader("Examples", 2);
                this.AddText(c.Example);
            }
        }

        private void AddSeeAlso(Type type)
        {
            this.AddHeader("See Also", 2);
            var p = new Paragraph();
            var c = Model.Find(type);
            if (c != null)
            {
                p.Content.Add(this.CreateLink(c, true));
                p.Content.Add(new LineBreak());
            }

            p.Content.Add(this.CreateNamespaceLink(type.Namespace, "{0} Namespace"));
            p.Content.Add(new LineBreak());
            this.doc.Blocks.Add(p);
        }

        private void AddNamespaceInfo(Type type)
        {
            var p = new Paragraph();
            p.Content.Add(new Strong().Add(new Run("Namespace:")));
            p.Content.Add(this.CreateNamespaceLink(type.Namespace));
            p.Content.Add(new LineBreak());
            p.Content.Add(new Strong().Add(new Run("Assembly: ")));
            p.Content.Add(new Run(type.Assembly.GetName().Name + " (in " + type.Assembly.GetName().Name + ".dll)"));
            this.doc.Blocks.Add(p);
        }

        private Hyperlink CreateTypeLink(Type t, bool strong = false)
        {
            var filename = this.ResolveFileName(t);
            var a = new Hyperlink { Url = filename };
            if (strong)
            {
                a.Content.Add(new Strong().Add(new Run(Utilities.GetNiceTypeName(t))));
            }
            else
            {
                a.Content.Add(new Run(Utilities.GetNiceTypeName(t)));
            }
            return a;
        }

        private Inline CreateNamespaceLink(string name, string formatString = "{0}")
        {
            var nm = Model.Namespaces.Values.FirstOrDefault(ns => ns.Name == name);
            if (nm == null) return new Run(name);

            var a = new Hyperlink { Url = nm.Name + ".html" };
            a.Content.Add(new Run(string.Format(formatString, name)));
            return a;
        }

        private string ResolveFileName(Type type)
        {
            var tm = Model.Find(type);
            if (tm == null)
            {
                if (type.Namespace.StartsWith("System"))
                {
                    return "http://msdn.microsoft.com/en-us/library/" + type.FullName.ToLower() + ".aspx";
                }
                return null;
            }
            return tm.GetFileName() + ".html";
        }

        private void CreateNamespacePage(NamespaceModel ns)
        {
            CreatePage();
            AddHeader(ns.GetTitle(), 1);
            this.AddText(ns.Description);

            this.AddTable("Classes", ns.Types.Where(t => t.Type.IsClass));
            this.AddTable("Structures", ns.Types.Where(t => t.Type.IsValueType));
            this.AddTable("Interfaces", ns.Types.Where(t => t.Type.IsInterface));
            // this.AddTable("Delegates", ns.Types.Where(t => t.Type.IsDelegate));
            this.AddTable("Enumerations", ns.Types.Where(t => t.Type.IsEnum));

            WritePage(ns.GetFileName(), ns.GetTitle(), ns.Description);
        }

        private void AddTable(string header, IEnumerable<Content> content, Type type = null)
        {
            var table = new Table();
            var trh = new TableRow();
            trh.Cells.Add(new TableHeaderCell("Name"));
            trh.Cells.Add(new TableHeaderCell("Description"));
            table.Rows.Add(trh);
            foreach (var t in content.OrderBy(t => t.ToString()))
            {
                var strong = t.Info != null && t.Info.DeclaringType == type;
                var tr = new TableRow();
                var td0 = new TableCell();
                td0.Content.Add(CreateLink(t, strong: strong));
                var td1 = new TableCell();
                AddText(td1.Content, t.Description);
                tr.Cells.Add(td0);
                tr.Cells.Add(td1);
                table.Rows.Add(tr);
            }
            if (table.Rows.Count > 1)
            {
                this.AddHeader(header, 2);
                this.AddTable(table);
            }
        }

        private void AddText(InlineCollection content, string description)
        {
            content.Add(new Run(description));
        }

        Hyperlink CreateLink(Content c, bool longtitle = false, bool strong = false)
        {
            var a = new Hyperlink { Url = c.GetFileName() + ".html", Title = c.GetTitle() };
            var text = longtitle ? c.GetTitle() : c.ToString();
            if (strong)
                a.Content.Add(new Strong().Add(new Run(text)));
            else
                a.Content.Add(new Run(text));
            return a;
        }

        private void AddTable(Table table)
        {
            doc.Blocks.Add(table);
        }

        private void AddText(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var para = new Paragraph();
            para.Content.Add(new Run(text));
            doc.Blocks.Add(para);
        }

        private void AddHeader(string text, int level)
        {
            if (string.IsNullOrWhiteSpace(text))
                return;
            var h = new Header { Level = level };
            h.Content.Add(new Run(text));
            doc.Blocks.Add(h);
        }

        private void CreateNamespacesPage(Model model)
        {
            CreatePage();
            AddHeader(model.Title, 1);
            AddText(model.Description);
            AddTable("Namespaces", model.Namespaces.Values);
            WritePage("Namespaces", null, null);
        }
    }
}