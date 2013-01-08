// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocFormatter.cs" company="Lynx">
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
namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;

    using LynxToolkit;
    using LynxToolkit.Documents;
    using LynxToolkit.Documents.OpenXml;

    public class DocFormatter
    {
        private static readonly Regex DescriptionExpression = new Regex(@"(?:
(?:\<see \s+ cref\s*=\s*""(?<cref>.*?)"" \s* /\>)|
(?:\<c\>(?<c>.*?)\</c\>)
)", RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public string Format { get; private set; }

        public string Output { get; private set; }

        public string OutputExtension { get; private set; }

        public bool SinglePage { get; private set; }

        public string StyleSheet { get; private set; }

        public string Template { get; private set; }

        private NamespaceCollection NamespaceCollection { get; set; }

        private Document doc { get; set; }

        // public void Save(string outputDirectory, string fileName)
        // {
        // string docPath = Path.Combine(outputDirectory, Path.ChangeExtension(fileName, ".txt"));
        // Utilities.CreateDirectoryIfMissing(outputDirectory);
        // Console.WriteLine("Output:             {0}", Path.GetFileName(docPath));
        // }
        public static void CreatePages(NamespaceCollection namespaceCollection, string output, string format, string outputExtension, string styleSheet, string template, bool singlePage, bool createMemberPages, int topLevel)
        {
            var f = new DocFormatter
                        {
                            NamespaceCollection = namespaceCollection,
                            Output = output,
                            OutputExtension = outputExtension,
                            Format = format,
                            SinglePage = singlePage,
                            StyleSheet = styleSheet,
                            Template = template
                        };

            f.CreateNamespacesPage(namespaceCollection, topLevel);
            foreach (var ns in namespaceCollection.Namespaces.Values.OrderBy(ns => ns.Name))
            {
                f.CreateNamespacePage(ns, topLevel);
                foreach (var t in ns.Types)
                {
                    f.CreateTypePage(t, createMemberPages, topLevel);

                    if (createMemberPages)
                    {
                        foreach (var m in t.Properties)
                        {
                            f.CreateMemberPage(m, topLevel);
                        }

                        foreach (var m in t.Methods)
                        {
                            f.CreateMemberPage(m, topLevel);
                        }

                        foreach (var m in t.Constructors)
                        {
                            f.CreateMemberPage(m, topLevel);
                        }

                        foreach (var m in t.Events)
                        {
                            f.CreateMemberPage(m, topLevel);
                        }
                    }
                }
            }

            if (singlePage)
            {
                f.WritePage(null, null, null);
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
                     .Select(t => this.NamespaceCollection.Find(t))
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
            p.Content.Add(new Hyperlink("Namespaces", new Run("Namespaces")));
            p.Content.Add(new LineBreak());

            foreach (var ns in namespaces)
            {
                p.Content.Add(this.CreateNamespaceLink(ns.Name, "{0} Namespace"));
                p.Content.Add(new LineBreak());
            }

            this.doc.Blocks.Add(p);
        }

        private void AddTable(string header, IEnumerable<Model> content, Type scope, bool createLinks = true, int headerLevel = 2)
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
                if (createLinks)
                {
                    td0.Content.Add(this.CreateLink(t, strong: !inherited));
                }
                else
                {
                    td0.Content.Add(new Run(t.ToString()));
                }

                var td1 = new TableCell();
                this.AddText(td1.Content, description, scope);
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
                this.AddText(td0.Content, t.ToString(), scope);

                var td1 = new TableCell();
                td1.Content.Add(this.CreateLink(t.Type));

                var td2 = new TableCell();
                this.AddText(td2.Content, t.Description, scope);
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

            DescriptionExpression.Match(
                description,
                s => content.Add(new Run(s)),
                match =>
                {
                    if (match.Groups["cref"].Success)
                    {
                        var cref = match.Groups["cref"].Value;
                        Model model;
                        if (this.NamespaceCollection.Find(cref, scope, out model))
                        {
                            content.Add(CreateLink(model));
                        }
                        else
                        {
                            if (cref.Length > 2 && cref[1] == ':')
                            {
                                cref = cref.Substring(2);
                            }

                            content.Add(new Run(cref));
                        }
                    }

                    if (match.Groups["c"].Success)
                    {
                        var c = match.Groups["c"].Value;
                        content.Add(new InlineCode { Text = c, Language = Language.Cs });
                    }
                });
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
            var model = this.NamespaceCollection.Find(type);
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
            var nm = this.NamespaceCollection.Namespaces.Values.FirstOrDefault(ns => ns.Name == name);
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
            this.AddTable("Structures", ns.Types.Where(t => t.Type.IsValueType && !t.Type.IsEnum), null, true, topLevel + 1);
            this.AddTable("Interfaces", ns.Types.Where(t => t.Type.IsInterface), null, true, topLevel + 1);

            // this.AddTable("Delegates", ns.Types.Where(t => t.Type.IsDelegate));
            this.AddTable("Enumerations", ns.Types.Where(t => t.Type.IsEnum), null, true, topLevel + 1);

            this.AddSeeAlso(this.NamespaceCollection.Namespaces.Values.Where(n => n != ns).OrderBy(n => n.Name));

            this.WritePage(ns.GetFileName(), ns.GetTitle(), ns.Description);
        }

        private void CreateNamespacesPage(NamespaceCollection namespaceCollection, int topLevel)
        {
            this.CreatePage();
            
            var commonNamespace = GetCommonStartString(namespaceCollection.Namespaces.Keys);
            if (commonNamespace != null)
            {
                commonNamespace = commonNamespace.TrimEnd('.');
            }

            this.AddHeader(namespaceCollection.Title ?? (commonNamespace + " namespaces"), topLevel, "namespaces");
            this.AddText(namespaceCollection.Description, null);
            this.AddTable(null, namespaceCollection.Namespaces.Values, null, true, topLevel + 1);
            this.WritePage(commonNamespace, null, null);
        }

        private string GetCommonStartString(IEnumerable<string> keys)
        {
            string result = null;
            foreach (var key in keys)
            {
                if (result == null)
                {
                    result = key;
                    continue;
                }

                int i;
                for (i = 0; i < result.Length && i < key.Length && result[i] == key[i]; i++) ;

                result = key.Substring(0, i);
            }

            return result;
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

            bool showInheritanceHierarchy = !typeModel.Type.IsEnum;

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

                    p.Content.Add(this.CreateTypeLink(t, t == typeModel.Type, true));
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

                    p.Content.Add(this.CreateTypeLink(t.Type, false, true));
                }

                this.doc.Blocks.Add(p);
            }

            this.AddNamespaceInfo(typeModel.Type);

            var syntax = typeModel.GetSyntax();
            if (syntax != null)
            {
                this.AddHeader("Syntax", topLevel + 1);
                this.doc.Blocks.Add(new CodeBlock { Text = syntax });
            }

            this.AddText(string.Format("The {0} type exposes the following members.", typeModel), null);

            this.AddTable("Constructors", typeModel.Constructors, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Properties", typeModel.Properties, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Methods", typeModel.Methods, typeModel.Type, createMemberPages, topLevel + 1);
            this.AddTable("Members", typeModel.EnumMembers, typeModel.Type, false, topLevel + 1);
            this.AddRemarks(typeModel, typeModel.Type, topLevel + 1);
            this.AddExamples(typeModel, typeModel.Type, topLevel + 1);
            this.WritePage(typeModel.GetFileName(), typeModel.GetPageTitle(), typeModel.Description);
        }

        private string GetLink(Type type)
        {
            var tm = this.NamespaceCollection.Find(type);
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

        private void WritePage(string fileName, string title, string description)
        {
            if (this.SinglePage && fileName != null)
            {
                doc.Blocks.Add(new HorizontalRuler());
                return;
            }

            this.doc.Title = title;
            this.doc.Description = description;

            switch (this.Format)
            {
                case "html":
                    {
                        var path = GetOutputFile(fileName, ".html");
                        var options = new HtmlFormatterOptions { Css = this.StyleSheet, Template = this.Template };
                        if (this.SinglePage)
                        {
                            options.LocalLinkFormatString = "#{0}";
                        }

                        var html = HtmlFormatter.Format(this.doc, options);
                        File.WriteAllText(path, html);
                        break;
                    }

                case "owiki":
                    {
                        var path = GetOutputFile(fileName, ".wiki");
                        var src = OWikiFormatter.Format(this.doc);
                        File.WriteAllText(path, src);
                        break;
                    }

                case "xml":
                    {
                        var path = GetOutputFile(fileName, ".xml");
                        var src = XmlFormatter.Format(this.doc);
                        File.WriteAllText(path, src);
                        break;
                    }

                case "word":
                    {
                        var path = GetOutputFile(fileName, ".docx");
                        var options = new WordFormatterOptions { Template = this.Template };
                        if (this.SinglePage)
                        {
                            // options.LocalLinkFormatString = "#{0}";
                        }

                        WordFormatter.Format(this.doc, path, options);
                        break;
                    }
            }

            this.doc = null;
        }

        private string GetOutputFile(string fileName, string defaultExt)
        {
            if (fileName == null)
            {
                return this.Output;
            }

            var ext = this.OutputExtension ?? defaultExt;
            return Path.Combine(this.Output, fileName + ext);
        }
    }
}