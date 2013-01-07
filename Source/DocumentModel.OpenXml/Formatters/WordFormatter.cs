namespace LynxToolkit.Documents.OpenXml
{
    using System.Globalization;
    using System;

    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;

    using LynxToolkit.Documents;

    using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
    using Paragraph = DocumentFormat.OpenXml.Wordprocessing.Paragraph;
    using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

    public class WordFormatterOptions : DocumentFormatterOptions
    {
        public string Template { get; set; }
    }

    public class WordFormatter : LynxToolkit.Documents.DocumentFormatter
    {
        public WordprocessingDocument Package { get; private set; }

        protected WordFormatter(LynxToolkit.Documents.Document doc, string filePath)
            : base(doc)
        {
            this.Package = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            this.SetPackageProperties(this.Package);

            this.mainPart = this.Package.AddMainDocumentPart();
            this.stylePart = this.CreateStylePart(this.mainPart, doc.StyleSheet);

            this.document = this.CreateDocument();
            this.body = new Body();
            this.document.Append(this.body);

            this.mainPart.Document = this.document;
        }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <returns>
        /// </returns>
        private Document CreateDocument()
        {
            var d = new Document { MCAttributes = new MarkupCompatibilityAttributes { Ignorable = "w14 wp14" } };
            d.AddNamespaceDeclaration("wpc", "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas");
            d.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            d.AddNamespaceDeclaration("o", "urn:schemas-microsoft-com:office:office");
            d.AddNamespaceDeclaration("r", "http://schemas.openxmlformats.org/officeDocument/2006/relationships");
            d.AddNamespaceDeclaration("m", "http://schemas.openxmlformats.org/officeDocument/2006/math");
            d.AddNamespaceDeclaration("v", "urn:schemas-microsoft-com:vml");
            d.AddNamespaceDeclaration("wp14", "http://schemas.microsoft.com/office/word/2010/wordprocessingDrawing");
            d.AddNamespaceDeclaration("wp", "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing");
            d.AddNamespaceDeclaration("w10", "urn:schemas-microsoft-com:office:word");
            d.AddNamespaceDeclaration("w", "http://schemas.openxmlformats.org/wordprocessingml/2006/main");
            d.AddNamespaceDeclaration("w14", "http://schemas.microsoft.com/office/word/2010/wordml");
            d.AddNamespaceDeclaration("wpg", "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup");
            d.AddNamespaceDeclaration("wpi", "http://schemas.microsoft.com/office/word/2010/wordprocessingInk");
            d.AddNamespaceDeclaration("wne", "http://schemas.microsoft.com/office/word/2006/wordml");
            d.AddNamespaceDeclaration("wps", "http://schemas.microsoft.com/office/word/2010/wordprocessingShape");
            return d;
        }

        private StyleDefinitionsPart CreateStylePart(MainDocumentPart mainPart, LynxToolkit.Documents.StyleSheet style)
        {
            var sdp = mainPart.AddNewPart<StyleDefinitionsPart>();

            sdp.Styles = new Styles();

            AppendStyle(sdp, style.ParagraphStyle, BodyTextID, BodyTextName, null, null, true, false);
            AppendStyle(sdp, style.Header1Style, string.Format(HeaderID, 1), string.Format(HeaderName, 1), "Heading1", BodyTextID, false, false);
            AppendStyle(sdp, style.Header2Style, string.Format(HeaderID, 2), string.Format(HeaderName, 2), "Heading1", BodyTextID, false, false);
            AppendStyle(sdp, style.Header3Style, string.Format(HeaderID, 3), string.Format(HeaderName, 3), "Heading1", BodyTextID, false, false);
            AppendStyle(sdp, style.Header4Style, string.Format(HeaderID, 4), string.Format(HeaderName, 4), "Heading1", BodyTextID, false, false);
            AppendStyle(sdp, style.Header5Style, string.Format(HeaderID, 5), string.Format(HeaderName, 5), "Heading1", BodyTextID, false, false);

            //sdp.Styles.Append(CreateStyle(style.TableTextStyle, TableTextID, TableTextName, null, null));
            //sdp.Styles.Append(CreateStyle(style.TableHeaderStyle, TableHeaderID, TableHeaderName, null, null));
            //sdp.Styles.Append(CreateStyle(style.TableCaptionStyle, TableCaptionID, TableCaptionName, null, null));

            //sdp.Styles.Append(CreateStyle(style.FigureTextStyle, FigureTextID, FigureTextName, null, null));

            sdp.Styles.Save();

            return sdp;
        }

        private static void AppendStyle(
            StyleDefinitionsPart part,
           LynxToolkit.Documents.Style documentStyle,
           string styleID,
           string styleName,
           string basedOnStyleID,
           string nextStyleID,
           bool isDefault = false,
           bool isCustomStyle = true)
        {
            if (documentStyle == null) return;

            // todo: add font to FontTable?
            var runProperties = new StyleRunProperties();

            // http://msdn.microsoft.com/en-us/library/documentformat.openxml.wordprocessing.color.aspx
            if (documentStyle.Foreground != null)
            {
                var color = new DocumentFormat.OpenXml.Wordprocessing.Color { Val = documentStyle.Foreground.ToString().Trim('#').Substring(2) };
                runProperties.Append(color);
            }

            // http://msdn.microsoft.com/en-us/library/cc850848.aspx
            runProperties.Append(new RunFonts { Ascii = documentStyle.FontFamily, HighAnsi = documentStyle.FontFamily });
            if (documentStyle.FontSize.HasValue)
            {
                runProperties.Append(
                    new FontSize
                        {
                            Val = new StringValue((documentStyle.FontSize.Value * 2).ToString(CultureInfo.InvariantCulture))
                        });
                runProperties.Append(
                    new FontSizeComplexScript
                        {
                            Val =
                                new StringValue(
                                (documentStyle.FontSize.Value * 2).ToString(CultureInfo.InvariantCulture))
                        });
            }

            if (documentStyle.FontWeight == FontWeight.Bold)
            {
                runProperties.Append(new Bold());
            }

            if (documentStyle.FontStyle == FontStyle.Italic)
            {
                runProperties.Append(new Italic());
            }

            var pPr = new StyleParagraphProperties();
            var spacingBetweenLines2 = new SpacingBetweenLines();
            var indentation = new Indentation();
            var contextualSpacing1 = new ContextualSpacing();
            if (documentStyle.Margin.HasValue)
            {
                spacingBetweenLines2.After = string.Format(CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Bottom * 20);
                spacingBetweenLines2.Before = string.Format(CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Top * 20);
                indentation.Left = string.Format(CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Left * 20);
                indentation.Right = string.Format(CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Right * 20);
            }
            spacingBetweenLines2.LineRule = LineSpacingRuleValues.Auto;

            pPr.Append(spacingBetweenLines2);
            pPr.Append(contextualSpacing1);
            pPr.Append(indentation);

            // http://msdn.microsoft.com/en-us/library/documentformat.openxml.wordprocessing.style.aspx
            var style = new DocumentFormat.OpenXml.Wordprocessing.Style
            {
                Default = new OnOffValue(isDefault),
                CustomStyle = new OnOffValue(isCustomStyle),
                StyleId = styleID,
                Type = StyleValues.Paragraph
            };

            style.Append(new Name { Val = styleName });
            if (basedOnStyleID != null)
            {
                style.Append(new BasedOn { Val = basedOnStyleID });
            }

            var rsid = new Rsid();

            // style.Append(rsid);
            var primaryStyle = new PrimaryStyle();
            style.Append(primaryStyle);
            if (nextStyleID != null)
            {
                style.Append(new NextParagraphStyle { Val = nextStyleID });
            }

            style.Append(runProperties);
            style.Append(pPr);
            part.Styles.Append(style);
        }

        /// <summary>
        /// Saves this document.
        /// </summary>
        public void Save()
        {
            this.mainPart.Document.Save();
        }

        public void Close()
        {
            this.Package.Close();
        }

        /// <summary>
        /// Set the package properties.
        /// </summary>
        /// <param name="p">
        /// The package.
        /// </param>
        private void SetPackageProperties(OpenXmlPackage p)
        {
            p.PackageProperties.Creator = doc.Creator;
            p.PackageProperties.Title = doc.Title;
            p.PackageProperties.Subject = doc.Subject;
            p.PackageProperties.Category = doc.Category;
            p.PackageProperties.Description = doc.Description;
            p.PackageProperties.Keywords = doc.Keywords;
            p.PackageProperties.Version = doc.Version;
            p.PackageProperties.Revision = doc.Revision;
            p.PackageProperties.Created = DateTime.Now;
            p.PackageProperties.Modified = DateTime.Now;
            p.PackageProperties.LastModifiedBy = doc.Creator;
        }

        /// <summary>
        /// The body text id.
        /// </summary>
        private const string BodyTextID = "Normal";

        /// <summary>
        /// The body text name.
        /// </summary>
        private const string BodyTextName = "Normal";

        /// <summary>
        /// The figure text id.
        /// </summary>
        private const string FigureTextID = "FigureText";

        /// <summary>
        /// The figure text name.
        /// </summary>
        private const string FigureTextName = "Figure text";

        /// <summary>
        /// The header id.
        /// </summary>
        private const string HeaderID = "Heading{0}";

        /// <summary>
        /// The header name.
        /// </summary>
        private const string HeaderName = "Heading {0}";

        /// <summary>
        /// The table caption id.
        /// </summary>
        private const string TableCaptionID = "TableCaption";

        /// <summary>
        /// The table caption name.
        /// </summary>
        private const string TableCaptionName = "Table caption";

        /// <summary>
        /// The table header id.
        /// </summary>
        private const string TableHeaderID = "TableHeader";

        /// <summary>
        /// The table header name.
        /// </summary>
        private const string TableHeaderName = "Table header";

        /// <summary>
        /// The table text id.
        /// </summary>
        private const string TableTextID = "TableText";

        /// <summary>
        /// The table text name.
        /// </summary>
        private const string TableTextName = "Table text";

        protected override void Write(LynxToolkit.Documents.Header header)
        {
            var headerID = string.Format(HeaderID, header.Level);
            var p = CreateParagraph(headerID);
            WriteInlines(header.Content, p);
            this.body.AppendChild(p);
        }

        protected override void Write(LynxToolkit.Documents.Paragraph paragraph)
        {
            var p = CreateParagraph(BodyTextID);
            WriteInlines(paragraph.Content, p);
            this.body.AppendChild(p);
        }

        private static Paragraph CreateParagraph(string styleID)
        {
            var p = new Paragraph();
            var pp = new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = styleID } };
            p.Append(pp);
            return p;
        }

        protected override void Write(LynxToolkit.Documents.List list)
        {

        }

        protected override void Write(LynxToolkit.Documents.OrderedList list)
        {

        }

        protected override void Write(LynxToolkit.Documents.Quote quote)
        {

        }

        protected override void Write(LynxToolkit.Documents.CodeBlock codeBlock)
        {

        }

        protected override void Write(LynxToolkit.Documents.HorizontalRuler ruler)
        {

        }

        protected override void Write(NonBreakingSpace nbsp, object parent)
        {
        }

        protected override void Write(LynxToolkit.Documents.Run run, object parent)
        {
            var compositeElement = (OpenXmlCompositeElement)parent;
            var text = new Text(run.Text);
            compositeElement.Append(new Run(text));
        }

        protected override void Write(LynxToolkit.Documents.Strong strong, object parent)
        {
            var compositeElement = (OpenXmlCompositeElement)parent;
            var r = new Run { RunProperties = new RunProperties(new Bold { Val = new OnOffValue { Value = true } }) };
            WriteInlines(strong.Content, r);
            compositeElement.Append(r);
        }

        protected override void Write(LynxToolkit.Documents.Emphasized em, object parent)
        {
            var compositeElement = (OpenXmlCompositeElement)parent;
            var r = new Run { RunProperties = new RunProperties(new Italic { Val = new OnOffValue { Value = true } }) };
            WriteInlines(em.Content, r);
            compositeElement.Append(r);
        }

        protected override void Write(LynxToolkit.Documents.LineBreak linebreak, object parent)
        {

        }

        protected override void Write(LynxToolkit.Documents.InlineCode inlineCode, object parent)
        {

        }

        protected override void Write(LynxToolkit.Documents.Hyperlink hyperlink, object parent)
        {

        }

        protected override void Write(LynxToolkit.Documents.Image image, object parent)
        {

        }

        protected override void Write(LynxToolkit.Documents.Symbol symbol, object parent)
        {

        }

        protected override void Write(LynxToolkit.Documents.Anchor anchor, object parent)
        {

        }

        private readonly StyleDefinitionsPart stylePart;
        private Body body;
        private MainDocumentPart mainPart;
        private Document document;

        public static WordprocessingDocument Format(LynxToolkit.Documents.Document doc, string filePath, WordFormatterOptions options)
        {
            var wf = new WordFormatter(doc, filePath);
            wf.FormatCore();
            wf.Save();
            wf.Close();
            return wf.Package;
        }
    }
}
