using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DocumentModel.OpenXml
{
    using System.Globalization;

    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;

    using LynxToolkit.Documents;

    using Document = DocumentFormat.OpenXml.Wordprocessing.Document;
    using Run = DocumentFormat.OpenXml.Wordprocessing.Run;

    public class WordFormatter : LynxToolkit.Documents.DocumentFormatter
    {
        public WordprocessingDocument Package { get; private set; }

        protected WordFormatter(LynxToolkit.Documents.Document doc, string filePath)
            : base(doc)
        {
            this.Package = WordprocessingDocument.Create(filePath, WordprocessingDocumentType.Document);
            this.mainPart = this.Package.AddMainDocumentPart();
            this.stylePart = this.mainPart.AddNewPart<StyleDefinitionsPart>();
            this.SetPackageProperties(this.Package);
            this.document = this.CreateDocument();
            this.body = new Body();
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

        private void AddStyles(StyleDefinitionsPart sdp, LynxToolkit.Documents.StyleSheet style)
        {
            sdp.Styles = new Styles();

            sdp.Styles.Append(CreateStyle(style.ParagraphStyle, BodyTextID, BodyTextName, null, null, true, false));
            sdp.Styles.Append(CreateStyle(style.Header1Style, string.Format(HeaderID, 1), string.Format(HeaderName, 1), "Heading1", BodyTextID, false, false));

            //sdp.Styles.Append(CreateStyle(style.TableTextStyle, TableTextID, TableTextName, null, null));
            //sdp.Styles.Append(CreateStyle(style.TableHeaderStyle, TableHeaderID, TableHeaderName, null, null));
            //sdp.Styles.Append(CreateStyle(style.TableCaptionStyle, TableCaptionID, TableCaptionName, null, null));

            //sdp.Styles.Append(CreateStyle(style.FigureTextStyle, FigureTextID, FigureTextName, null, null));
        }

        private static DocumentFormat.OpenXml.Wordprocessing.Style CreateStyle(
           LynxToolkit.Documents.Style ps,
           string styleID,
           string styleName,
           string basedOnStyleID,
           string nextStyleID,
           bool isDefault = false,
           bool isCustomStyle = true)
        {
            // todo: add font to FontTable?
            var rPr = new StyleRunProperties();

            // http://msdn.microsoft.com/en-us/library/documentformat.openxml.wordprocessing.color.aspx
            var color = new DocumentFormat.OpenXml.Wordprocessing.Color { Val = ps.Foreground.ToString().Trim('#').Substring(2) };
            rPr.Append(color);

            // http://msdn.microsoft.com/en-us/library/cc850848.aspx
            rPr.Append(new RunFonts { Ascii = ps.FontFamily, HighAnsi = ps.FontFamily });
            if (ps.FontSize.HasValue)
            {
                rPr.Append(
                    new FontSize
                        {
                            Val = new StringValue((ps.FontSize.Value * 2).ToString(CultureInfo.InvariantCulture))
                        });
                rPr.Append(
                    new FontSizeComplexScript
                        {
                            Val =
                                new StringValue(
                                (ps.FontSize.Value * 2).ToString(CultureInfo.InvariantCulture))
                        });
            }

            if (ps.FontWeight == FontWeight.Bold)
            {
                rPr.Append(new Bold());
            }

            if (ps.FontStyle == FontStyle.Italic)
            {
                rPr.Append(new Italic());
            }

            var pPr = new StyleParagraphProperties();
            var spacingBetweenLines2 = new SpacingBetweenLines
            {
                After = string.Format(CultureInfo.InvariantCulture, "{0}", ps.Margin.Value.Bottom * 20),
                Before = string.Format(CultureInfo.InvariantCulture, "{0}", ps.Margin.Value.Top * 20),
                // Line = string.Format(CultureInfo.InvariantCulture, "{0}", ps.LineSpacing * 240),
                LineRule = LineSpacingRuleValues.Auto
            };
            var indentation = new Indentation
            {
                Left = string.Format(CultureInfo.InvariantCulture, "{0}", ps.Margin.Value.Left * 20),
                Right = string.Format(CultureInfo.InvariantCulture, "{0}", ps.Margin.Value.Right * 20)
            };
            var contextualSpacing1 = new ContextualSpacing();

            pPr.Append(spacingBetweenLines2);
            pPr.Append(contextualSpacing1);
            pPr.Append(indentation);

            // StyleRunProperties styleRunProperties7 = new StyleRunProperties();
            // RunFonts runFonts8 = new RunFonts() { Ascii = "Verdana", HighAnsi = "Verdana" };
            // Color color7 = new Color() { Val = "000000" };

            // styleRunProperties7.Append(runFonts8);
            // styleRunProperties7.Append(color7);

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

            style.Append(rPr);
            style.Append(pPr);
            return style;
        }

        /// <summary>
        /// Saves this document.
        /// </summary>
        public void Save()
        {
            //this.SetPackageProperties(this.package);
            this.document.Append(this.body);
            this.mainPart.Document = this.document;

            this.stylePart.Styles.Save();
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
        /// The create paragraph.
        /// </summary>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <param name="styleID">
        /// The style id.
        /// </param>
        /// <returns>
        /// </returns>
        private static DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateParagraph(InlineCollection content, string styleID = null)
        {
            var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

            if (styleID != null)
            {
                var pp = new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = styleID } };
                p.Append(pp);
            }


            WriteInlines(p, content);
            return p;
        }

        private static void WriteInlines(OpenXmlCompositeElement compositeElement, InlineCollection content)
        {
            foreach (var inline in content)
            {
                var run = inline as LynxToolkit.Documents.Run;
                if (run != null)
                {
                    var text = new Text(run.Text);
                    compositeElement.Append(new Run(text));
                }

                var strong = inline as LynxToolkit.Documents.Strong;
                if (strong != null)
                {
                    var r = new Run();
                    r.RunProperties = new RunProperties(new Bold() { Val = new OnOffValue { Value = true } });
                    WriteInlines(r, strong.Content);
                    compositeElement.Append(r);
                }
            }
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
            this.body.AppendChild(CreateParagraph(header.Content, string.Format(HeaderID, header.Level)));
        }

        protected override void Write(LynxToolkit.Documents.Paragraph paragraph)
        {
            this.body.AppendChild(CreateParagraph(paragraph.Content));
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

        protected override void Write(LynxToolkit.Documents.Run run)
        {

        }

        protected override void Write(LynxToolkit.Documents.Strong strong)
        {

        }

        protected override void Write(LynxToolkit.Documents.Emphasized em)
        {

        }

        protected override void Write(LynxToolkit.Documents.LineBreak linebreak)
        {

        }

        protected override void Write(LynxToolkit.Documents.InlineCode inlineCode)
        {

        }

        protected override void Write(LynxToolkit.Documents.Hyperlink hyperlink)
        {

        }

        protected override void Write(LynxToolkit.Documents.Image image)
        {

        }

        protected override void Write(LynxToolkit.Documents.Symbol symbol)
        {

        }

        protected override void Write(LynxToolkit.Documents.Anchor anchor)
        {

        }

        private readonly StyleDefinitionsPart stylePart;
        private Body body;
        private MainDocumentPart mainPart;
        private Document document;

        public static WordprocessingDocument Format(LynxToolkit.Documents.Document doc, string filePath, string symbolDirectory = null)
        {
            var wf = new WordFormatter(doc, filePath);
            wf.FormatCore();
            wf.Save();
            wf.Close();
            return wf.Package;
        }
    }
}
