// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocFormatter.cs" company="Lynx Toolkit">
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
//   Adds the text.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using HhcGen;

    using LynxToolkit;
    using LynxToolkit.Documents;
    using LynxToolkit.Documents.Html;
    using LynxToolkit.Documents.OpenXml;

    public class DocFormatter
    {
        private static readonly Regex DescriptionBlocksExpression = new Regex(@"(?:
(?:\<code\>(?<code>.*?)\</code\>)|
(?:\<para\>(?<code>.*?)\</para\>)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private static readonly Regex DescriptionExpression = new Regex(@"(?:
(?:\<see \s+ cref\s*=\s*""(?<see>.*?)"" \s* /\>)|
(?:\<seealso \s+ cref\s*=\s*""(?<seealso>.*?)"" \s* /\>)|
(?:\<paramref \s+ name\s*=\s*""(?<paramref>.*?)"" \s* /\>)|
(?:\<typeparamref \s+ name\s*=\s*""(?<typeparamref>.*?)"" \s* /\>)|
(?:\<c\>(?<c>.*?)\</c\>)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public DocFormatter()
        {
            this.Model = new LibraryModel();
            this.Output = ".";
            this.Format = "html";
            this.OutputExtension = null;
            this.StyleSheet = null;
            this.Template = null;
            this.SinglePage = false;
            this.CreateMemberPages = false;
            this.TopLevel = 1;
            this.IndexPage = "Index";
            this.IndexTitle = null;
            this.LongHierarchyTypeNames = false;
            this.Replacements = new Dictionary<string, string>();
        }

        public bool CreateMemberPages { get; set; }

        public string Format { get; set; }

        public string HtmlHelpContents { get; set; }

        public string IndexPage { set { this.Model.FileName = value; } }

        public string IndexTitle { set { this.Model.Title = value; } }

        public bool LongHierarchyTypeNames { get; set; }

        public LibraryModel Model { get; set; }

        public string Output { get; set; }

        public string OutputExtension { get; set; }

        public bool SinglePage { get; set; }

        public string StyleSheet { get; set; }

        public string Template { get; set; }

        public int TopLevel { get; set; }

        /// <summary>
        /// Gets or sets the replacement strings.
        /// </summary>
        /// <value>
        /// The replacement strings.
        /// </value>
        /// <remarks>
        /// The replacement keys will be prefixed by "$" and replaced by their values.
        /// </remarks>
        public Dictionary<string, string> Replacements { get; set; }

        private Document doc { get; set; }

        public void WritePages()
        {
            if (this.OutputExtension == null)
            {
                switch (this.Format)
                {
                    case "html":
                        this.OutputExtension = ".html";
                        break;
                    case "owiki":
                        this.OutputExtension = ".wiki";
                        break;

                    case "xml":
                        this.OutputExtension = ".xml";
                        break;

                    case "word":
                        this.OutputExtension = ".docx";
                        break;
                }
            }

            var siteMap = new SiteMap();

            this.CreateNamespacesPage(this.Model, this.TopLevel);
            var index = new SiteMap(this.Model.Title, this.Model.GetFileName() + this.OutputExtension);
            siteMap.Children.Add(index);
            foreach (var ns in this.Model.Namespaces.Values.OrderBy(ns => ns.Name))
            {
                var namespacePage = new SiteMap(ns.GetTitle(), ns.GetFileName() + this.OutputExtension);
                index.Children.Add(namespacePage);

                this.CreateNamespacePage(ns, this.TopLevel);
                foreach (var t in ns.Types)
                {
                    this.CreateTypePage(t, this.CreateMemberPages, this.TopLevel);

                    var typePage = new SiteMap(t.GetTitle(), t.GetFileName() + this.OutputExtension);
                    namespacePage.Children.Add(typePage);

                    if (this.CreateMemberPages)
                    {
                        foreach (var m in t.Properties)
                        {
                            this.CreateMemberPage(m, this.TopLevel);
                        }

                        foreach (var m in t.Fields)
                        {
                            this.CreateMemberPage(m, this.TopLevel);
                        }

                        foreach (var m in t.Methods)
                        {
                            this.CreateMemberPage(m, this.TopLevel);
                        }

                        foreach (var m in t.Operators)
                        {
                            this.CreateMemberPage(m, this.TopLevel);
                        }

                        foreach (var m in t.Constructors)
                        {
                            this.CreateMemberPage(m, this.TopLevel);
                        }

                        foreach (var m in t.Events)
                        {
                            this.CreateMemberPage(m, this.TopLevel);
                        }
                    }
                }
            }

            if (this.SinglePage)
            {
                this.WritePage(null, null, null);
            }

            if (this.HtmlHelpContents != null)
            {
                siteMap.WriteContentsFile(this.HtmlHelpContents);
            }
        }

        private void AddExamples(Model c, Type scope, int headerLevel)
        {
            if (!string.IsNullOrWhiteSpace(c.Example))
            {
                this.AddHeader("Examples", headerLevel);
                this.AddText(c.Example, scope);

                // var code = new CodeBlock();
                // code.Text = c.Example;
                // code.Language = Language.Cs;
                // this.doc.Blocks.Add(code);
            }
        }

        private Header AddHeader(string text, int level, string id = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var h = new Header { Level = level, ID = id };
            h.Content.Add(new Run(text));
            this.doc.Blocks.Add(h);
            return h;
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

        private void AddRemarks(Model c, Type scope, int headerLevel)
        {
            if (!string.IsNullOrWhiteSpace(c.Remarks))
            {
                this.AddHeader("Remarks", headerLevel);
                this.AddText(c.Remarks, scope);
            }
        }

        private void AddSeeAlso(IEnumerable<Type> types, int headerLevel)
        {
            var typeModels =
                types.Distinct()
                     .Select(t => this.Model.Find(t))
                     .Where(tm => tm != null)
                     .OrderBy(t => t.Type.FullName)
                     .ToList();
            if (typeModels.Count == 0)
            {
                return;
            }

            this.AddHeader("See Also", headerLevel);

            var p = new Paragraph();
            foreach (var type in typeModels)
            {
                p.Content.Add(this.CreateLink(type, true));
                p.Content.Add(new LineBreak());
            }

            var namespaces = typeModels.Select(t => t.Type.Namespace).Distinct().OrderBy(ns => ns);
            foreach (var ns in namespaces)
            {
                p.Content.Add(this.CreateNamespaceLink(ns, "{0} Namespace"));
                p.Content.Add(new LineBreak());
            }

            this.doc.Blocks.Add(p);
        }

        private void AddSeeAlso(IEnumerable<NamespaceModel> namespaces)
        {
            this.AddHeader("See Also", 2);

            var p = new Paragraph();
            p.Content.Add(new Hyperlink(this.Model.GetFileName(), new Run(this.Model.Title)));
            p.Content.Add(new LineBreak());

            foreach (var ns in namespaces)
            {
                p.Content.Add(this.CreateNamespaceLink(ns.Name, "{0} Namespace"));
                p.Content.Add(new LineBreak());
            }

            this.doc.Blocks.Add(p);
        }

        private void AddTable(
            string header, IEnumerable<Model> content, Type scope, bool createLinks = true, int headerLevel = 2)
        {
            var table = new Table();
            var trh = new TableRow();
            trh.Cells.Add(new TableHeaderCell("Name"));
            trh.Cells.Add(new TableHeaderCell("Description"));
            table.Rows.Add(trh);
            foreach (var t in content.OrderBy(t => t.ToString()))
            {
                var inherited = t.IsInherited();
                var description = t.GetDescription();

                var tr = new TableRow();
                var td0 = new TableCell();
                var p0 = new Paragraph();
                td0.Blocks.Add(p0);
                if (createLinks)
                {
                    p0.Content.Add(this.CreateLink(t, strong: !inherited));
                }
                else
                {
                    p0.Content.Add(new Run(t.ToString()));
                }

                var td1 = new TableCell();
                var p1 = new Paragraph();
                this.AddText(p1.Content, description, scope);
                td1.Blocks.Add(p1);
                tr.Cells.Add(td0);
                tr.Cells.Add(td1);
                table.Rows.Add(tr);
            }

            if (table.Rows.Count > 1)
            {
                if (header != null)
                {
                    this.AddHeader(header, headerLevel);
                }

                this.AddTable(table);
            }
        }

        private void AddTable(string header, IEnumerable<ParameterModel> content, Type scope, int headerLevel = 2)
        {
            var table = new Table();
            var trh = new TableRow();
            trh.Cells.Add(new TableHeaderCell("Name"));
            trh.Cells.Add(new TableHeaderCell("Type"));
            trh.Cells.Add(new TableHeaderCell("Description"));
            table.Rows.Add(trh);
            foreach (var t in content)
            {
                var tr = new TableRow();
                var td0 = new TableCell();
                var p0 = new Paragraph();
                td0.Blocks.Add(p0);
                this.AddText(p0.Content, t.ToString(), scope);

                var td1 = new TableCell();
                var p1 = new Paragraph();
                td1.Blocks.Add(p1);                
                p1.Content.Add(this.CreateLink(t.Type));

                var td2 = new TableCell();
                var p2 = new Paragraph();
                td2.Blocks.Add(p2);
                this.AddText(p2.Content, t.Description, scope);
                tr.Cells.Add(td0);
                tr.Cells.Add(td1);
                tr.Cells.Add(td2);
                table.Rows.Add(tr);
            }

            if (table.Rows.Count > 1)
            {
                this.AddHeader(header, headerLevel);
                this.AddTable(table);
            }
        }

        private void AddTable(Table table)
        {
            this.doc.Blocks.Add(table);
        }

        /// <summary>
        ///     Adds the text.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <param name="description">The description.</param>
        /// <param name="scope">The scope used to resolve cross references.</param>
        private void AddText(InlineCollection content, string description, Type scope)
        {
            if (description == null)
            {
                return;
            }

            // todo: code and para

            DescriptionExpression.Match(description, s => content.Add(new Run(s)), match => this.AppendDescription(content, scope, match));
        }

        private void AppendDescription(InlineCollection content, Type scope, Match match)
        {
            var seeSuccess = match.Groups["see"].Success;
            var seeAlsoSuccess = match.Groups["seealso"].Success;
            if (seeSuccess || seeAlsoSuccess)
            {
                var cref = match.Groups[seeSuccess ? "see" : "seealso"].Value;
                string url, title, text;
                if (this.ResolveCrossReference(cref, scope, out url, out title, out text))
                {
                    var link = new Hyperlink { Url = url, Title = title };
                    link.Content.Add(new Run(text));
                    content.Add(link);
                }
                else
                {
                    if (cref.Length > 2 && cref[1] == ':')
                    {
                        cref = cref.Substring(2);
                    }

                    content.Add(new Run(cref));
                }

                return;
            }

            if (match.Groups["paramref"].Success)
            {
                var name = match.Groups["paramref"].Value;
                content.Add(new InlineCode(name));
            }

            if (match.Groups["typeparamref"].Success)
            {
                var name = match.Groups["typeparamref"].Value;
                content.Add(new InlineCode(name));
            }

            if (match.Groups["c"].Success)
            {
                var c = match.Groups["c"].Value;
                content.Add(new InlineCode { Code = c, Language = Language.Cs });
            }
        }

        private bool ResolveCrossReference(string cref, Type scope, out string url, out string title, out string text)
        {
            Model model;
            if (this.Model.Find(cref, scope, out model))
            {
                url = model.GetFileName();
                title = model.GetTitle();
                text = model.ToString();
                return true;
            }

            Type type;
            if (FindType(cref, scope, out type))
            {
                url = GetLink(type);
                title = XmlUtilities.GetNiceTypeName(type);
                text = XmlUtilities.GetNiceTypeName(type);
                return true;
            }

            if (cref.Length > 2 && cref[1] == ':')
            {
                cref = cref.Substring(2);
            }

            url = null;
            title = null;
            text = cref;
            return false;
        }

        private bool FindType(string name, Type scope, out Type type)
        {
            if (name.StartsWith("T:"))
            {
                name = name.Substring(2);
            }

            type = Type.GetType(name);
            if (type != null)
                return true;

            if (scope != null)
            {
                type = Type.GetType(string.Format("{0}.{1}", scope.Namespace, name));
                if (type != null)
                {
                    return true;
                }

                type = Type.GetType(string.Format("{0}.{1}", scope.FullName, name));
                if (type != null)
                {
                    return true;
                }
            }

            return false;
        }

        private void AddText(string text, Type scope)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            var para = new Paragraph();
            this.AddText(para.Content, text, scope);
            this.doc.Blocks.Add(para);
        }

        private Hyperlink CreateLink(Type type)
        {
            var model = this.Model.Find(type);
            if (model != null)
            {
                return this.CreateLink(model);
            }

            var link = this.GetLink(type);
            var name = XmlUtilities.GetNiceTypeName(type);
            var a = new Hyperlink { Url = link, Title = name };
            a.Content.Add(new Run(name));
            return a;
        }

        private Hyperlink CreateLink(Model c, bool longtitle = false, bool strong = false)
        {
            var a = new Hyperlink { Url = c.GetFileName(), Title = c.GetTitle() };
            var text = longtitle ? c.GetTitle() : c.ToString();
            if (strong)
            {
                a.Content.Add(new Strong().Add(new Run(text)));
            }
            else
            {
                a.Content.Add(new Run(text));
            }

            return a;
        }

        private void CreateMemberPage(MemberModel c, int topLevel)
        {
            this.CreatePage();
            this.AddHeader(c.GetTitle(), topLevel, c.GetFileName());
            this.AddText(c.Description, c.DeclaringType);

            this.AddNamespaceInfo(c.Info.DeclaringType);

            var syntax = c.GetSyntax();
            if (syntax != null)
            {
                this.AddHeader("Syntax", topLevel + 1);
                this.doc.Blocks.Add(new CodeBlock { Text = syntax });
            }

            this.AddTable("Parameters", c.GetParameters(), c.DeclaringType, topLevel + 2);
            var mm = c as MethodModel;
            if (mm != null && mm.ReturnType != typeof(void))
            {
                this.AddHeader("Return Value", topLevel + 2);
                var p = new Paragraph();
                p.Content.Add(new Run("Type: "));
                p.Content.Add(this.CreateLink(mm.ReturnType));
                p.Content.Add(new LineBreak());
                this.AddText(p.Content, mm.ReturnValueDescription, c.DeclaringType);
                this.doc.Blocks.Add(p);
            }

            // this.AddImplements(c);
            this.AddRemarks(c, c.DeclaringType, topLevel + 1);
            this.AddExamples(c, c.DeclaringType, topLevel + 1);

            this.AddSeeAlso(c.GetRelatedTypes(), topLevel + 1);

            this.WritePage(c.GetFileName(), c.GetPageTitle(), c.Description);
        }

        private Inline CreateNamespaceLink(string name, string formatString = "{0}")
        {
            var nm = this.Model.Namespaces.Values.FirstOrDefault(ns => ns.Name == name);
            if (nm == null)
            {
                return new Run(name);
            }

            var a = new Hyperlink { Url = nm.Name };
            a.Content.Add(new Run(string.Format(formatString, name)));
            return a;
        }

        private void CreateNamespacePage(NamespaceModel ns, int topLevel)
        {
            this.CreatePage();
            this.AddHeader(ns.GetTitle(), topLevel, ns.GetFileName());
            this.AddText(ns.Description, null);

            this.AddTable("Classes", ns.Types.Where(t => t.Type.IsClass), null, true, topLevel + 1);
            this.AddTable(
                "Structures", ns.Types.Where(t => t.Type.IsValueType && !t.Type.IsEnum), null, true, topLevel + 1);
            this.AddTable("Interfaces", ns.Types.Where(t => t.Type.IsInterface), null, true, topLevel + 1);

            // this.AddTable("Delegates", ns.Types.Where(t => t.Type.IsDelegate));
            this.AddTable("Enumerations", ns.Types.Where(t => t.Type.IsEnum), null, true, topLevel + 1);

            this.AddSeeAlso(this.Model.Namespaces.Values.Where(n => n != ns).OrderBy(n => n.Name));

            this.WritePage(ns.GetFileName(), ns.GetTitle(), ns.Description);
        }

        private void CreateNamespacesPage(LibraryModel libraryModel, int topLevel)
        {
            this.CreatePage();

            this.AddHeader(libraryModel.Title, topLevel, "namespaces");
            this.AddText(libraryModel.Description, null);
            this.AddTable(null, libraryModel.Namespaces.Values, null, true, topLevel + 1);
            this.WritePage(this.Model.GetFileName(), libraryModel.Title, libraryModel.Description);
        }

        private void CreatePage()
        {
            if (this.doc != null)
            {
                return;
            }

            this.doc = new Document();
        }

        private Inline CreateTypeLink(Type t, bool strong = false, bool fullName = false)
        {
            var url = this.GetLink(t);
            var typeName = XmlUtilities.GetNiceTypeName(t, fullName);

            if (url == null)
            {
                return new Run(typeName);
            }

            var a = new Hyperlink { Url = url };

            if (strong)
            {
                a.Content.Add(new Strong().Add(new Run(typeName)));
            }
            else
            {
                a.Content.Add(new Run(typeName));
            }

            return a;
        }

        private void CreateTypePage(TypeModel typeModel, bool createMemberPages, int topLevel)
        {
            this.CreatePage();
            this.AddHeader(typeModel.GetTitle(), topLevel, typeModel.GetFileName());
            this.AddText(typeModel.Description, typeModel.Type);

            bool showInheritanceHierarchy = !typeModel.Type.IsEnum && !typeModel.Type.IsValueType;

            if (showInheritanceHierarchy)
            {
                this.AddHeader("Inheritance hierarchy", topLevel + 1);
                var p = new Paragraph();
                int level = 0;
                foreach (var t in typeModel.InheritedTypes)
                {
                    if (p.Content.Count > 0)
                    {
                        p.Content.Add(new LineBreak());
                    }

                    for (int i = 0; i < level; i++)
                    {
                        p.Content.Add(new NonBreakingSpace());
                    }

                    p.Content.Add(this.CreateTypeLink(t, t == typeModel.Type, this.LongHierarchyTypeNames));
                    level += 2;
                }

                foreach (var t in typeModel.DerivedTypes)
                {
                    if (p.Content.Count > 0)
                    {
                        p.Content.Add(new LineBreak());
                    }

                    for (int i = 0; i < level; i++)
                    {
                        p.Content.Add(new NonBreakingSpace());
                    }

                    p.Content.Add(this.CreateTypeLink(t.Type, false, this.LongHierarchyTypeNames));
                }

                this.doc.Blocks.Add(p);
            }

            this.AddNamespaceInfo(typeModel.Type);

            var syntax = typeModel.GetSyntax();
            if (syntax != null)
            {
                this.AddHeader("Syntax", topLevel + 1);
                this.doc.Blocks.Add(new CodeBlock { Text = syntax, Language = Language.Cs });
            }

            // this.AddText(string.Format("The {0} type exposes the following members.", typeModel), null);

            this.AddTable("Constructors", typeModel.Constructors, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Properties", typeModel.Properties, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Methods", typeModel.Methods, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Operators", typeModel.Operators, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Fields", typeModel.Fields, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Events", typeModel.Events, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Members", typeModel.EnumMembers, typeModel.Type, false, topLevel + 1);
            this.AddRemarks(typeModel, typeModel.Type, topLevel + 1);
            this.AddExamples(typeModel, typeModel.Type, topLevel + 1);
            this.WritePage(typeModel.GetFileName(), typeModel.GetPageTitle(), typeModel.Description);
        }

        private string GetLink(Type type)
        {
            var tm = this.Model.Find(type);
            if (tm == null)
            {
                if (type.Namespace.StartsWith("System") && type.FullName != null)
                {
                    return "http://msdn.microsoft.com/en-us/library/" + type.FullName.ToLower() + ".aspx";
                }

                return null;
            }

            return tm.GetFileName();
        }

        private string GetOutputFile(string fileName)
        {
            if (fileName == null)
            {
                return this.Output;
            }

            return System.IO.Path.Combine(this.Output, fileName + this.OutputExtension);
        }

        private void WritePage(string fileName, string title, string description)
        {
            if (this.SinglePage && fileName != null)
            {
                this.doc.Blocks.Add(new HorizontalRuler());
                return;
            }

            this.doc.Title = title;
            this.doc.Description = description;

            IDocumentFormatter formatter=null;
            switch (this.Format)
            {
                case "html":
                    {
                        formatter = new HtmlFormatter { Css = this.StyleSheet, Template = this.Template, Variables = this.Replacements };
                        if (this.SinglePage)
                        {
                            ((HtmlFormatter)formatter).LocalLinkFormatString = "#{0}";
                        }

                        break;
                    }

                //case "owiki":
                //    {
                //        var path = this.GetOutputFile(fileName);
                //        var src = OWikiFormatter.Format(this.doc);
                //        File.WriteAllText(path, src);
                //        break;
                //    }

                //case "xml":
                //    {
                //        var path = this.GetOutputFile(fileName);
                //        var src = XmlFormatter.Format(this.doc);
                //        File.WriteAllText(path, src);
                //        break;
                //    }

                case "word":
                    {
                        formatter = new WordFormatter { Template = this.Template };                    
                        break;
                    }
            }

            if (formatter == null)
            {
                throw new FormatException(string.Format("The output format '{0}' is not supported.", Format));
            }

            var path = this.GetOutputFile(fileName);
            formatter.Format(this.doc, File.OpenWrite(path));
            
            this.doc = null;
        }
    }
}