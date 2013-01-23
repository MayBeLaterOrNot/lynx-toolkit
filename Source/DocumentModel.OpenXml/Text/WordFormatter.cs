namespace LynxToolkit.Documents.OpenXml
{
    using System;
    using System.Globalization;
    using System.IO;

    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Drawing;
    using DocumentFormat.OpenXml.Drawing.Wordprocessing;
    using DocumentFormat.OpenXml.Office2010.Drawing;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;

    using Anchor = LynxToolkit.Documents.Anchor;
    using BottomBorder = DocumentFormat.OpenXml.Wordprocessing.BottomBorder;
    using Document = LynxToolkit.Documents.Document;
    using Header = LynxToolkit.Documents.Header;
    using Hyperlink = LynxToolkit.Documents.Hyperlink;
    using LeftBorder = DocumentFormat.OpenXml.Wordprocessing.LeftBorder;
    using NonVisualDrawingProperties = DocumentFormat.OpenXml.Drawing.NonVisualDrawingProperties;
    using NonVisualGraphicFrameDrawingProperties = DocumentFormat.OpenXml.Drawing.Wordprocessing.NonVisualGraphicFrameDrawingProperties;
    using Outline = DocumentFormat.OpenXml.Wordprocessing.Outline;
    using Paragraph = LynxToolkit.Documents.Paragraph;
    using ParagraphProperties = DocumentFormat.OpenXml.Wordprocessing.ParagraphProperties;
    using Picture = DocumentFormat.OpenXml.Wordprocessing.Picture;
    using RightBorder = DocumentFormat.OpenXml.Wordprocessing.RightBorder;
    using Run = LynxToolkit.Documents.Run;
    using RunProperties = DocumentFormat.OpenXml.Wordprocessing.RunProperties;
    using Style = LynxToolkit.Documents.Style;
    using TableCell = DocumentFormat.OpenXml.Wordprocessing.TableCell;
    using TableCellBorders = DocumentFormat.OpenXml.Wordprocessing.TableCellBorders;
    using TableCellProperties = DocumentFormat.OpenXml.Wordprocessing.TableCellProperties;
    using TableGrid = DocumentFormat.OpenXml.Wordprocessing.TableGrid;
    using TableProperties = DocumentFormat.OpenXml.Wordprocessing.TableProperties;
    using TableRow = DocumentFormat.OpenXml.Wordprocessing.TableRow;
    using TableStyle = DocumentFormat.OpenXml.Wordprocessing.TableStyle;
    using Text = DocumentFormat.OpenXml.Wordprocessing.Text;
    using TopBorder = DocumentFormat.OpenXml.Wordprocessing.TopBorder;
    using Transform2D = DocumentFormat.OpenXml.Drawing.Transform2D;

    public class WordFormatterOptions : DocumentFormatterOptions
    {
        public string Template { get; set; }
    }

    public class WordFormatter : DocumentFormatter
    {
        /// <summary>
        /// The body text id.
        /// </summary>
        private const string BodyTextID = "Normal";

        private const string CodeID = "Code";

        private const string QuoteID = "Code";

        private const string InlineCodeID = "InlineCode";

        private const string HyperlinkID = "Hyperlink";

        /// <summary>
        /// The figure text id.
        /// </summary>
        private const string FigureTextID = "FigureText";

        /// <summary>
        /// The header id.
        /// </summary>
        private const string HeaderID = "Heading{0}";

        /// <summary>
        /// The table header id.
        /// </summary>
        private const string TableHeaderID = "TableHeader";

        /// <summary>
        /// The table text id.
        /// </summary>
        private const string TableTextID = "TableText";

        /// <summary>
        /// The list paragraph id.
        /// </summary>
        private const string ListID = "ListParagraph";

        private readonly Body body;

        private readonly DocumentFormat.OpenXml.Wordprocessing.Document document;

        private readonly MainDocumentPart mainPart;

        private readonly MemoryStream stream;

        protected WordFormatter(Document doc, string template)
            : base(doc)
        {
            stream = new MemoryStream();
            if (template != null)
            {
                var templateBytes = File.ReadAllBytes(template);
                stream.Write(templateBytes, 0, templateBytes.Length);
                this.Package = WordprocessingDocument.Open(stream, true);
                this.mainPart = this.Package.MainDocumentPart;
                this.document = this.mainPart.Document;
                this.body = this.document.Body;
            }
            else
            {
                this.Package = WordprocessingDocument.Create(stream, WordprocessingDocumentType.Document);
                this.mainPart = this.Package.AddMainDocumentPart();
                this.document = this.CreateDocument();
                this.body = new Body();
                this.document.Body = body;
                //this.document.Append(this.body);
                this.mainPart.Document = this.document;
                this.AddStylePart(this.mainPart, doc.StyleSheet);
            }

            this.SetPackageProperties(this.Package);
        }

        public WordprocessingDocument Package { get; private set; }

        public static WordprocessingDocument Format(Document doc, string filePath, WordFormatterOptions options)
        {
            var wf = new WordFormatter(doc, options.Template);
            wf.Format();
            wf.Save(filePath);
            return wf.Package;
        }

        /// <summary>
        /// Saves this document.
        /// </summary>
        public void Save(string filepath)
        {
            this.document.Save();
            this.Package.Close();
            File.WriteAllBytes(filepath, stream.ToArray());
        }

        protected override void Write(Header header)
        {
            var headerID = string.Format(HeaderID, header.Level);
            var p = CreateParagraph(headerID);
            this.WriteInlines(header.Content, p);
            this.body.AppendChild(p);
        }

        protected override void Write(TableOfContents toc)
        {
            var sdt = new SdtContentBlock();
            this.body.AppendChild(sdt);
        }

        protected override void Write(Paragraph paragraph)
        {
            var p = CreateParagraph(BodyTextID);
            this.WriteInlines(paragraph.Content, p);
            this.body.AppendChild(p);
        }

        protected override void Write(Documents.Table t)
        {
            var table = new DocumentFormat.OpenXml.Wordprocessing.Table();

            var tableProperties1 = new TableProperties();
            var tableStyle1 = new TableStyle { Val = "TableGrid" };
            var tableWidth1 = new TableWidth { Width = "0", Type = TableWidthUnitValues.Auto };
            var tableLook1 = new TableLook
            {
                Val = "04A0",
                FirstRow = true,
                LastRow = false,
                FirstColumn = true,
                LastColumn = false,
                NoHorizontalBand = false,
                NoVerticalBand = true
            };

            tableProperties1.Append(tableStyle1);
            tableProperties1.Append(tableWidth1);
            tableProperties1.Append(tableLook1);

            var tableGrid1 = new TableGrid();
            //foreach (var tc in t.Columns)
            //{
            //    // tc.Width
            //    var gridColumn1 = new GridColumn { Width = "3070" };
            //    tableGrid1.Append(gridColumn1);
            //}

            foreach (var row in t.Rows)
            {
                var tr = new TableRow();

                //if (row.IsHeader)
                //{
                //    var trp = new TableRowProperties();
                //    var tableHeader1 = new TableHeader();
                //    trp.Append(tableHeader1);
                //    tr.Append(trp);
                //}

                foreach (var c in row.Cells)
                {
                    bool isHeader = c is TableHeaderCell;
                    var cell = new TableCell();
                    var tcp = new TableCellProperties();
                    var borders = new TableCellBorders();
                    borders.Append(
                        new BottomBorder
                        {
                            Val = BorderValues.Single,
                            Size = (UInt32Value)4U,
                            Space = (UInt32Value)0U,
                            Color = "auto"
                        });
                    borders.Append(
                        new TopBorder
                        {
                            Val = BorderValues.Single,
                            Size = (UInt32Value)4U,
                            Space = (UInt32Value)0U,
                            Color = "auto"
                        });
                    borders.Append(
                        new LeftBorder
                        {
                            Val = BorderValues.Single,
                            Size = (UInt32Value)4U,
                            Space = (UInt32Value)0U,
                            Color = "auto"
                        });
                    borders.Append(
                        new RightBorder
                        {
                            Val = BorderValues.Single,
                            Size = (UInt32Value)4U,
                            Space = (UInt32Value)0U,
                            Color = "auto"
                        });
                    tcp.Append(borders);

                    cell.Append(tcp);
                    string styleID = isHeader ? TableHeaderID : TableTextID;
                    var p = CreateParagraph(styleID);
                    this.WriteInlines(c.Content, p);
                    cell.Append(p);
                    tr.Append(cell);
                }

                table.Append(tr);
            }

            this.body.Append(table);
        }

        protected override void Write(UnorderedList list)
        {
            WriteListItems(list, 0);
        }

        private void WriteListItems(List list, int level)
        {
            foreach (var item in list.Items)
            {
                var p = this.GenerateListItemParagraph(item, ListID, level, list is UnorderedList);
                this.body.Append(p);
                if (item.NestedList != null)
                {
                    this.WriteListItems(item.NestedList, level + 1);
                }
            }
        }

        DocumentFormat.OpenXml.Wordprocessing.Paragraph GenerateListItemParagraph(Documents.ListItem item, string styleID, int level, bool unordered)
        {

            var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();

            var paragraphProperties1 = new ParagraphProperties();
            var paragraphStyleId1 = new ParagraphStyleId { Val = styleID };

            var numberingProperties1 = new NumberingProperties();
            var numberingLevelReference1 = new NumberingLevelReference { Val = level };
            var numberingId1 = new NumberingId { Val = unordered ? 1 : 2 };
            var paragraphMarkRunProperties1 = new ParagraphMarkRunProperties();

            numberingProperties1.Append(numberingLevelReference1);
            numberingProperties1.Append(numberingId1);

            paragraphProperties1.Append(paragraphStyleId1);
            paragraphProperties1.Append(numberingProperties1);
            paragraphProperties1.Append(paragraphMarkRunProperties1);
            WriteInlines(item.Content, p);

            p.Append(paragraphProperties1);
            return p;
        }

        protected override void Write(OrderedList list)
        {
            WriteListItems(list, 0);
        }

        protected override void Write(Quote quote)
        {
            var p = CreateParagraph(QuoteID);
            this.WriteInlines(quote.Content, p);
            this.body.AppendChild(p);
        }

        protected override void Write(CodeBlock codeBlock)
        {
            var p = CreateParagraph(CodeID);
            var runStyle = new RunStyle { Val = CodeID };
            var runProperties = new RunProperties(runStyle);
            var run = new DocumentFormat.OpenXml.Wordprocessing.Run(runProperties, new Text(codeBlock.Text));
            p.AppendChild(run);
            this.body.AppendChild(p);
        }

        protected override void Write(HorizontalRuler ruler)
        {
        }

        protected override void Write(NonBreakingSpace nbsp, object parent)
        {
            this.AppendElement(new DocumentFormat.OpenXml.Wordprocessing.Run(new Text(" ") { Space = SpaceProcessingModeValues.Preserve }), parent);
        }

        protected override void Write(Run r, object parent)
        {
            this.AppendElement(new DocumentFormat.OpenXml.Wordprocessing.Run(new Text(r.Text) { Space = SpaceProcessingModeValues.Preserve }), parent);
        }

        protected override void Write(Strong strong, object parent)
        {
            var r = new DocumentFormat.OpenXml.Wordprocessing.Run
                        {
                            RunProperties =
                                new RunProperties(
                                new Bold
                                    {
                                        Val =
                                            new OnOffValue
                                                {
                                                    Value = true
                                                }
                                    })
                        };
            this.WriteInlines(strong.Content, r);
            this.AppendElement(r, parent);
        }

        protected override void Write(Emphasized em, object parent)
        {
            var r = new DocumentFormat.OpenXml.Wordprocessing.Run
                        {
                            RunProperties =
                                new RunProperties(
                                new Italic
                                    {
                                        Val =
                                            new OnOffValue
                                                {
                                                    Value =
                                                        true
                                                }
                                    })
                        };
            this.WriteInlines(em.Content, r);
            this.AppendElement(r, parent);
        }

        protected override void Write(LineBreak linebreak, object parent)
        {
            this.AppendElement(new DocumentFormat.OpenXml.Wordprocessing.Break(), parent);
        }

        protected override void Write(InlineCode inlineCode, object parent)
        {
            var runStyle = new RunStyle { Val = InlineCodeID };
            var runProperties = new RunProperties(runStyle);
            var run = new DocumentFormat.OpenXml.Wordprocessing.Run(runProperties, new Text(inlineCode.Code));
            this.AppendElement(run, parent);
        }

        protected override void Write(Hyperlink hyperlink, object parent)
        {
            var h = new DocumentFormat.OpenXml.Wordprocessing.Hyperlink();
            h.Anchor = hyperlink.Url;
            h.Tooltip = hyperlink.Title;
            this.WriteInlines(hyperlink.Content, h);
            this.AppendElement(h, parent);
        }

        private void AppendElement(OpenXmlElement element, object parent)
        {
            var compositeElement = (OpenXmlCompositeElement)parent;
            compositeElement.AppendChild(element);
        }

        protected override void Write(Image image, object parent)
        {
            var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var source = System.IO.Path.Combine(image.BaseDirectory, image.Source);
            p.AppendChild(this.CreateImageRun(source, image.AlternateText));
            body.AppendChild(p);
        }

        protected override void Write(Symbol symbol, object parent)
        {
            var path = this.ResolveSymbolPath(symbol);
            var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            p.AppendChild(this.CreateImageRun(path, null));
            body.AppendChild(p);
        }

        /// <summary>
        /// The append image.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="description">
        /// The name.
        /// </param>
        private DocumentFormat.OpenXml.Wordprocessing.Run CreateImageRun(string source, string description)
        {
            if (!File.Exists(source))
            {
                throw new FileNotFoundException("The image was not found.", source);
            }

            // http://msdn.microsoft.com/en-us/library/bb497430.aspx
            string ext = System.IO.Path.GetExtension(source).ToLower();

            var ipt = ImagePartType.Jpeg;
            if (ext == ".png")
            {
                ipt = ImagePartType.Png;
            }

            var imagePart = this.mainPart.AddImagePart(ipt);
            using (var imageStream = File.OpenRead(source))
            {
                imagePart.FeedData(imageStream);
            }

            using (var bmp = new System.Drawing.Bitmap(source))
            {
                double width = bmp.Width / bmp.HorizontalResolution; // inches
                double height = bmp.Height / bmp.VerticalResolution; // inches
                double w = 15 / 2.54;
                double h = height / width * w;
                return this.CreateImageRun(this.mainPart.GetIdOfPart(imagePart), description, source, w, h);
            }
        }
        private DocumentFormat.OpenXml.Wordprocessing.Run CreateImageRun(
           string relationshipId, string name, string description, double width, double height)
        {
            // http://msdn.microsoft.com/en-us/library/documentformat.openxml.drawing.extents.aspx
            // http://polymathprogrammer.com/2009/10/22/english-metric-units-and-open-xml/

            // cx (Extent Length)
            // Specifies the length of the extents rectangle in EMUs. This rectangle shall dictate the size of the object as displayed (the result of any scaling to the original object).
            // Example: Consider a DrawingML object specified as follows:
            // <… cx="1828800" cy="200000"/>
            // The cx attributes specifies that this object has a height of 1828800 EMUs (English Metric Units). end example]
            // The possible values for this attribute are defined by the ST_PositiveCoordinate simple type (§20.1.10.42).

            // cy (Extent Width)
            // Specifies the width of the extents rectangle in EMUs. This rectangle shall dictate the size of the object as displayed (the result of any scaling to the original object).
            // Example: Consider a DrawingML object specified as follows:
            // < … cx="1828800" cy="200000"/>
            // The cy attribute specifies that this object has a width of 200000 EMUs (English Metric Units). end example]
            // The possible values for this attribute are defined by the ST_PositiveCoordinate simple type (§20.1.10.42).

            var run1 = new DocumentFormat.OpenXml.Wordprocessing.Run();

            var runProperties1 = new RunProperties();
            var noProof1 = new NoProof();

            runProperties1.Append(noProof1);

            var drawing1 = new Drawing();

            var inline1 = new DocumentFormat.OpenXml.Drawing.Wordprocessing.Inline
            {
                DistanceFromTop = 0U,
                DistanceFromBottom = 0U,
                DistanceFromLeft = 0U,
                DistanceFromRight = 0U
            };
            var extent1 = new Extent { Cx = 5753100L, Cy = 3600450L };
            extent1.Cx = (long)(width * 914400);
            extent1.Cy = (long)(height * 914400);

            var effectExtent1 = new EffectExtent { LeftEdge = 0L, TopEdge = 0L, RightEdge = 0L, BottomEdge = 0L };
            var docProperties1 = new DocProperties { Id = 1U, Name = name, Description = description };

            var nonVisualGraphicFrameDrawingProperties1 = new NonVisualGraphicFrameDrawingProperties();

            var graphicFrameLocks1 = new GraphicFrameLocks { NoChangeAspect = true };
            graphicFrameLocks1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            nonVisualGraphicFrameDrawingProperties1.Append(graphicFrameLocks1);

            var graphic1 = new Graphic();
            graphic1.AddNamespaceDeclaration("a", "http://schemas.openxmlformats.org/drawingml/2006/main");

            var graphicData1 = new GraphicData { Uri = "http://schemas.openxmlformats.org/drawingml/2006/picture" };

            var picture1 = new Picture();
            picture1.AddNamespaceDeclaration("pic", "http://schemas.openxmlformats.org/drawingml/2006/picture");

            var nonVisualPictureProperties1 = new NonVisualPictureProperties();
            var nonVisualDrawingProperties1 = new NonVisualDrawingProperties
            {
                Id = 0U,
                Name = name,
                Description = description
            };

            var nonVisualPictureDrawingProperties1 = new NonVisualPictureDrawingProperties();
            var pictureLocks1 = new PictureLocks { NoChangeAspect = true, NoChangeArrowheads = true };

            nonVisualPictureDrawingProperties1.Append(pictureLocks1);

            nonVisualPictureProperties1.Append(nonVisualDrawingProperties1);
            nonVisualPictureProperties1.Append(nonVisualPictureDrawingProperties1);

            var blipFill1 = new BlipFill();

            var blip1 = new Blip { Embed = relationshipId };

            var blipExtensionList1 = new BlipExtensionList();

            var blipExtension1 = new BlipExtension { Uri = "{28A0092B-C50C-407E-A947-70E740481C1C}" };

            var useLocalDpi1 = new UseLocalDpi { Val = false };
            useLocalDpi1.AddNamespaceDeclaration("a14", "http://schemas.microsoft.com/office/drawing/2010/main");

            blipExtension1.Append(useLocalDpi1);

            blipExtensionList1.Append(blipExtension1);

            blip1.Append(blipExtensionList1);
            var sourceRectangle1 = new SourceRectangle();

            var stretch1 = new Stretch();
            var fillRectangle1 = new FillRectangle();

            stretch1.Append(fillRectangle1);

            blipFill1.Append(blip1);
            blipFill1.Append(sourceRectangle1);
            blipFill1.Append(stretch1);

            var shapeProperties1 = new ShapeProperties { BlackWhiteMode = BlackWhiteModeValues.Auto };

            var transform2D1 = new Transform2D();
            var offset1 = new Offset { X = 0L, Y = 0L };
            var extents1 = new Extents { Cx = extent1.Cx, Cy = extent1.Cy };

            transform2D1.Append(offset1);
            transform2D1.Append(extents1);

            var presetGeometry1 = new PresetGeometry { Preset = ShapeTypeValues.Rectangle };
            var adjustValueList1 = new AdjustValueList();

            presetGeometry1.Append(adjustValueList1);
            var noFill1 = new NoFill();

            var outline1 = new Outline();
            var noFill2 = new NoFill();

            //outline1.Append(noFill2);

            shapeProperties1.Append(transform2D1);
            shapeProperties1.Append(presetGeometry1);
            shapeProperties1.Append(noFill1);
            shapeProperties1.Append(outline1);

            picture1.Append(nonVisualPictureProperties1);
            picture1.Append(blipFill1);
            picture1.Append(shapeProperties1);

            graphicData1.Append(picture1);

            graphic1.Append(graphicData1);

            inline1.Append(extent1);
            inline1.Append(effectExtent1);
            inline1.Append(docProperties1);
            inline1.Append(nonVisualGraphicFrameDrawingProperties1);
            inline1.Append(graphic1);

            drawing1.Append(inline1);

            run1.Append(runProperties1);
            run1.Append(drawing1);


            return run1;
        }
        private int bookmarkId;

        protected override void Write(Anchor anchor, object parent)
        {
            string id = bookmarkId.ToString();
            bookmarkId++;
            var bookmarkStart1 = new BookmarkStart() { Name = anchor.Name, Id = id };
            var bookmarkEnd1 = new BookmarkEnd() { Id = id };
            this.AppendElement(bookmarkStart1, parent);
            this.AppendElement(bookmarkEnd1, parent);
        }

        private static void AppendStyle(
            StyleDefinitionsPart part,
            Style documentStyle,
            string styleID,
            string styleName,
            string basedOnStyleID,
            string nextStyleID,
            bool isDefault = false,
            bool isCustomStyle = true)
        {
            if (documentStyle == null)
            {
                return;
            }

            // todo: add font to FontTable?
            var runProperties = new StyleRunProperties();

            // http://msdn.microsoft.com/en-us/library/documentformat.openxml.wordprocessing.color.aspx
            if (documentStyle.Foreground != null)
            {
                var color = new Color { Val = documentStyle.Foreground.ToString().Trim('#').Substring(2) };
                runProperties.Append(color);
            }

            // http://msdn.microsoft.com/en-us/library/cc850848.aspx
            runProperties.Append(new RunFonts { Ascii = documentStyle.FontFamily, HighAnsi = documentStyle.FontFamily });
            if (documentStyle.FontSize.HasValue)
            {
                runProperties.Append(
                    new FontSize
                        {
                            Val =
                                new StringValue(
                                (documentStyle.FontSize.Value * 2).ToString(CultureInfo.InvariantCulture))
                        });
                runProperties.Append(
                    new FontSizeComplexScript
                        {
                            Val =
                                new StringValue(
                                (documentStyle.FontSize.Value * 2).ToString(
                                    CultureInfo.InvariantCulture))
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
                spacingBetweenLines2.After = string.Format(
                    CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Bottom * 20);
                spacingBetweenLines2.Before = string.Format(
                    CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Top * 20);
                indentation.Left = string.Format(
                    CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Left * 20);
                indentation.Right = string.Format(
                    CultureInfo.InvariantCulture, "{0}", documentStyle.Margin.Value.Right * 20);
            }
            spacingBetweenLines2.LineRule = LineSpacingRuleValues.Auto;

            // pPr.Append(new KeepNext());
            // pPr.Append(new KeepLines());

            if (documentStyle.PageBreakBefore)
            {
                pPr.Append(new PageBreakBefore());
            }

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

        private static DocumentFormat.OpenXml.Wordprocessing.Paragraph CreateParagraph(string styleID)
        {
            var p = new DocumentFormat.OpenXml.Wordprocessing.Paragraph();
            var pp = new ParagraphProperties { ParagraphStyleId = new ParagraphStyleId { Val = styleID } };
            p.Append(pp);
            return p;
        }

        /// <summary>
        /// Creates the document.
        /// </summary>
        /// <returns>
        /// </returns>
        private DocumentFormat.OpenXml.Wordprocessing.Document CreateDocument()
        {
            var d = new DocumentFormat.OpenXml.Wordprocessing.Document
                        {
                            MCAttributes =
                                new MarkupCompatibilityAttributes
                                    {
                                        Ignorable
                                            =
                                            "w14 wp14"
                                    }
                        };
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

        private StyleDefinitionsPart AddStylePart(MainDocumentPart mainPart, StyleSheet style)
        {
            var sdp = mainPart.AddNewPart<StyleDefinitionsPart>();

            sdp.Styles = new Styles();

            AppendStyle(sdp, style.ParagraphStyle, BodyTextID, "Normal", null, null, true, false);
            for (int i = 1; i <= 5; i++)
            {
                AppendStyle(sdp, style.HeaderStyles[i - 1], string.Format(HeaderID, i), string.Format("Heading {0}", i), "Heading1", BodyTextID, false, false);
            }

            AppendStyle(sdp, style.QuoteStyle, QuoteID, "Quote", BodyTextID, BodyTextID);

            AppendStyle(sdp, style.CodeStyle, CodeID, "Code", BodyTextID, BodyTextID);
            AppendStyle(sdp, style.InlineCodeStyle, InlineCodeID, "Inline Code", CodeID, BodyTextID);

            AppendStyle(sdp, style.TableStyle, TableTextID, "Table Text", BodyTextID, BodyTextID);
            AppendStyle(sdp, style.TableHeaderStyle, TableHeaderID, "Table Header", TableTextID, BodyTextID);

            AppendStyle(sdp, style.UnorderedListStyle, ListID, "List Paragraph", BodyTextID, BodyTextID);

            sdp.Styles.Save();

            return sdp;
        }

        /// <summary>
        /// Set the package properties.
        /// </summary>
        /// <param name="p">
        /// The package.
        /// </param>
        private void SetPackageProperties(OpenXmlPackage p)
        {
            p.PackageProperties.Creator = this.doc.Creator;
            p.PackageProperties.Title = this.doc.Title;
            p.PackageProperties.Subject = this.doc.Subject;
            p.PackageProperties.Category = this.doc.Category;
            p.PackageProperties.Description = this.doc.Description;
            p.PackageProperties.Keywords = this.doc.Keywords;
            p.PackageProperties.Version = this.doc.Version;
            p.PackageProperties.Revision = this.doc.Revision;
            p.PackageProperties.Created = DateTime.Now;
            p.PackageProperties.Modified = DateTime.Now;
            p.PackageProperties.LastModifiedBy = this.doc.Creator;
        }
    }
}