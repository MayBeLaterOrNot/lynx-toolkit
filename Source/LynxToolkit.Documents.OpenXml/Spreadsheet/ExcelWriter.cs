// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExcelWriter.cs" company="Lynx Toolkit">
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
//   Provides export of spreadsheet models to Excel OpenXML files.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents.OpenXml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Provides export of spreadsheet models to Excel OpenXML files.
    /// </summary>
    public static class ExcelWriter
    {
        /// <summary>
        /// The Excel day zero.
        /// </summary>
        private static readonly DateTime ExcelDayZero = new DateTime(1899, 12, 30);

        /// <summary>
        /// Exports the specified book to the specified file.
        /// </summary>
        /// <param name="book">
        /// The book.
        /// </param>
        /// <param name="filePath">
        /// The file path.
        /// </param>
        public static void Export(Spreadsheet.Workbook book, string filePath)
        {
            using (var stream = File.Create(filePath))
            {
                Export(book, stream);
            }
        }

        /// <summary>
        /// Exports the specified book to a stream.
        /// </summary>
        /// <param name="book">
        /// The book.
        /// </param>
        /// <param name="stream">
        /// The stream.
        /// </param>
        public static void Export(Spreadsheet.Workbook book, Stream stream)
        {
            using (var document = SpreadsheetDocument.Create(stream, SpreadsheetDocumentType.Workbook))
            {
                Export(book, document);
            }
        }

        /// <summary>
        /// Converts the border style.
        /// </summary>
        /// <param name="borderStyle">
        /// The border style.
        /// </param>
        /// <returns>
        /// A BorderStyleValues.
        /// </returns>
        private static BorderStyleValues ConvertBorderStyle(Spreadsheet.BorderStyle borderStyle)
        {
            switch (borderStyle)
            {
                case Spreadsheet.BorderStyle.Thin:
                    return BorderStyleValues.Thin;
                case Spreadsheet.BorderStyle.Thick:
                    return BorderStyleValues.Thick;
                case Spreadsheet.BorderStyle.Double:
                    return BorderStyleValues.Double;
                case Spreadsheet.BorderStyle.Hair:
                    return BorderStyleValues.Hair;
                case Spreadsheet.BorderStyle.Dashed:
                    return BorderStyleValues.Dashed;
                default:
                    return BorderStyleValues.None;
            }
        }

        /// <summary>
        /// Creates the style sheet.
        /// </summary>
        /// <param name="styles">
        /// The styles.
        /// </param>
        /// <returns>
        /// A <see cref="Stylesheet"/>.
        /// </returns>
        private static Stylesheet CreateStylesheet(IEnumerable<Spreadsheet.Style> styles)
        {
            var numberingFormats = new List<NumberingFormat>();
            var fontElements = new List<Font>();
            var cellFormatElements = new List<CellFormat>();
            var borderElements = new List<Border> { new Border() };

            var fillElements = new List<Fill>
                                   {
                                       new Fill(new PatternFill { PatternType = PatternValues.None }),
                                       new Fill(new PatternFill { PatternType = PatternValues.Gray125 })
                                   };

            uint numberFormatId = 1;
            uint fontId = 0;
            uint fillId = (uint)fillElements.Count;
            uint borderId = 1;
            uint formatId = 0;

            foreach (var style in styles)
            {
                var cellFormat = new CellFormat
                                     {
                                         BorderId = 0U,
                                         FillId = 0U,
                                         FontId = fontId,
                                         FormatId = formatId,
                                         NumberFormatId = 0U,
                                         ApplyNumberFormat = false
                                     };

                if (style.NumberFormat != null)
                {
                    cellFormat.ApplyNumberFormat = true;
                    cellFormat.NumberFormatId = numberFormatId;
                    var nf = new NumberingFormat
                                 {
                                     NumberFormatId = numberFormatId,
                                     FormatCode = StringValue.FromString(style.NumberFormat)
                                 };
                    numberingFormats.Add(nf);
                    numberFormatId++;
                }

                if (style.Background != null)
                {
                    cellFormat.ApplyFill = true;
                    cellFormat.FillId = fillId;
                    var color = style.Background.Value.ToString("X6");
                    var patternFill3 = new PatternFill { PatternType = PatternValues.Solid };
                    patternFill3.AppendChild(new ForegroundColor { Rgb = color });
                    patternFill3.AppendChild(new BackgroundColor { Indexed = 64U });
                    fillElements.Add(new Fill(patternFill3));
                    fillId++;
                }

                var fontSize1 = new FontSize { Val = style.FontSize };
                var color1 = new Color();
                if (style.Foreground != null)
                {
                    color1.Rgb = style.Foreground.Value.ToString("X6");
                }
                else
                {
                    color1.Theme = 1U;
                }

                var fontName1 = new FontName { Val = style.FontName };
                var fontFamilyNumbering1 = new FontFamilyNumbering { Val = 2 };
                var fontScheme1 = new FontScheme { Val = FontSchemeValues.Minor };

                var font1 = new Font();
                if (style.Bold)
                {
                    font1.AppendChild(new Bold());
                }

                if (style.Italic)
                {
                    font1.AppendChild(new Italic());
                }

                font1.AppendChild(fontSize1);
                font1.AppendChild(color1);
                font1.AppendChild(fontName1);
                font1.AppendChild(fontFamilyNumbering1);
                if (formatId == 0)
                {
                    font1.AppendChild(fontScheme1);
                }

                fontElements.Add(font1);
                cellFormat.ApplyFont = true;

                if (style.HasBorder())
                {
                    var leftBorder1 = new LeftBorder();
                    var rightBorder1 = new RightBorder();
                    var topBorder1 = new TopBorder();
                    var bottomBorder1 = new BottomBorder();
                    var diagonalBorder1 = new DiagonalBorder();

                    leftBorder1.Style = ConvertBorderStyle(style.LeftBorderStyle);
                    rightBorder1.Style = ConvertBorderStyle(style.RightBorderStyle);
                    topBorder1.Style = ConvertBorderStyle(style.TopBorderStyle);
                    bottomBorder1.Style = ConvertBorderStyle(style.BottomBorderStyle);
                    leftBorder1.Color = new Color { Rgb = style.LeftBorderColor.ToString("X6") };
                    rightBorder1.Color = new Color { Rgb = style.RightBorderColor.ToString("X6") };
                    topBorder1.Color = new Color { Rgb = style.TopBorderColor.ToString("X6") };
                    bottomBorder1.Color = new Color { Rgb = style.BottomBorderColor.ToString("X6") };

                    var border1 = new Border(leftBorder1, rightBorder1, topBorder1, bottomBorder1, diagonalBorder1);
                    borderElements.Add(border1);

                    cellFormat.ApplyBorder = true;
                    cellFormat.BorderId = borderId;

                    borderId++;
                }

                var alignment = new Alignment();
                switch (style.HorizontalAlignment)
                {
                    case Spreadsheet.HorizontalAlignment.Left:
                        alignment.Horizontal = HorizontalAlignmentValues.Left;
                        cellFormat.ApplyAlignment = true;
                        break;
                    case Spreadsheet.HorizontalAlignment.Center:
                        alignment.Horizontal = HorizontalAlignmentValues.Center;
                        cellFormat.ApplyAlignment = true;
                        break;
                    case Spreadsheet.HorizontalAlignment.Right:
                        alignment.Horizontal = HorizontalAlignmentValues.Right;
                        cellFormat.ApplyAlignment = true;
                        break;
                }

                switch (style.VerticalAlignment)
                {
                    case Spreadsheet.VerticalAlignment.Top:
                        alignment.Vertical = VerticalAlignmentValues.Top;
                        cellFormat.ApplyAlignment = true;
                        break;
                    case Spreadsheet.VerticalAlignment.Middle:
                        alignment.Vertical = VerticalAlignmentValues.Center;
                        cellFormat.ApplyAlignment = true;
                        break;
                    case Spreadsheet.VerticalAlignment.Bottom:
                        alignment.Vertical = VerticalAlignmentValues.Bottom;
                        cellFormat.ApplyAlignment = true;
                        break;
                }

                if (cellFormat.ApplyAlignment != null)
                {
                    cellFormat.AppendChild(alignment);
                }

                cellFormatElements.Add(cellFormat);

                formatId++;
                fontId++;
            }

            var borders = new Borders(borderElements) { Count = (uint)borderElements.Count };
            var fills = new Fills(fillElements) { Count = (uint)fillElements.Count };
            var fonts = new Fonts(fontElements) { Count = (uint)fontElements.Count, KnownFonts = true };

            var cellFormats = new CellFormats(cellFormatElements) { Count = (uint)cellFormatElements.Count };
            var differentialFormats = new DifferentialFormats { Count = 0U };
            var tableStyles = new TableStyles
                                  {
                                      Count = 0U,
                                      DefaultTableStyle = "TableStyleMedium2",
                                      DefaultPivotStyle = "PivotStyleLight16"
                                  };

            var cellStyleFormats1 = new CellStyleFormats { Count = 1U };
            var cellFormat1 = new CellFormat
                                  {
                                      NumberFormatId = 0U,
                                      FontId = 0U,
                                      FillId = 0U,
                                      BorderId = 0U
                                  };
            cellStyleFormats1.AppendChild(cellFormat1);

            var cellStyle1 = new CellStyle { Name = "Normal", FormatId = 0U, BuiltinId = 0U };
            var cellStyles = new CellStyles() { Count = 1 };
            cellStyles.AppendChild(cellStyle1);

            var stylesheet1 = new Stylesheet(
                fonts, fills, borders, cellFormats, cellStyles, differentialFormats, tableStyles)
                                  {
                                      MCAttributes =
                                          new MarkupCompatibilityAttributes
                                              {
                                                  Ignorable
                                                      =
                                                      "x14ac"
                                              }
                                  };

            if (numberingFormats.Count > 0)
            {
                stylesheet1.NumberingFormats = new NumberingFormats(numberingFormats);
            }

            stylesheet1.AddNamespaceDeclaration("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006");
            stylesheet1.AddNamespaceDeclaration("x14ac", "http://schemas.microsoft.com/office/spreadsheetml/2009/9/ac");
            return stylesheet1;
        }

        /// <summary>
        /// Exports the specified book to an OpenXML document.
        /// </summary>
        /// <param name="book">
        /// The book.
        /// </param>
        /// <param name="document">
        /// The document.
        /// </param>
        private static void Export(Spreadsheet.Workbook book, SpreadsheetDocument document)
        {
            var workbookPart = document.AddWorkbookPart();
            var workbook = new Workbook();
            workbookPart.Workbook = workbook;

            var sheets = workbook.AppendChild(new Sheets());

            var styles = new Dictionary<Spreadsheet.Style, uint>();
            uint styleIndex = 0;
            foreach (var style in book.Styles)
            {
                styles.Add(style, styleIndex);
            }

            var stylesPart = workbookPart.AddNewPart<WorkbookStylesPart>();

            stylesPart.Stylesheet = CreateStylesheet(styles.Keys);

            uint i = 1;
            foreach (var spreadSheet in book.Sheets)
            {
                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                var sheetData = new SheetData();
                var worksheet = new Worksheet();
                worksheetPart.Worksheet = worksheet;

                // Append a new worksheet and associate it with the workbook.
                var sheet = new Sheet
                                {
                                    Id = workbookPart.GetIdOfPart(worksheetPart),
                                    SheetId = i++,
                                    Name = spreadSheet.Name
                                };
                sheets.AppendChild(sheet);

                var rows = new Dictionary<int, Row>();
                foreach (var c in spreadSheet.Cells.OrderBy(cell => cell.Row))
                {
                    Row row;
                    if (!rows.TryGetValue(c.Row, out row))
                    {
                        row = new Row { RowIndex = (uint)c.Row + 1 };
                        rows.Add(c.Row, row);
                    }

                    var s = c.Style == null ? 0U : (uint)book.Styles.IndexOf(c.Style);
                    var cell = new Cell { CellReference = new StringValue(c.GetCellReferenceString()), StyleIndex = s };

                    if (c.Formula != null)
                    {
                        cell.AppendChild(new CellFormula(c.Formula));
                    }

                    SetCellValue(cell, c.Content);

                    row.AppendChild(cell);
                }

                foreach (var row in rows.Values.OrderBy(r => (uint)r.RowIndex))
                {
                    sheetData.AppendChild(row);
                }

                if (spreadSheet.Columns.Count > 0)
                {
                    var columns = new Columns();
                    uint j = 1;
                    foreach (var c in spreadSheet.Columns)
                    {
                        var actualWidth = c.Width;
                        if (double.IsNaN(actualWidth))
                        {
                            actualWidth = spreadSheet.FindMaximumColumnWidth(j - 1);
                        }

                        var column = new Column
                                         {
                                             Width = new DoubleValue(actualWidth),
                                             Min = j,
                                             Max = j,
                                             BestFit = true,
                                             CustomWidth = true
                                         };
                        j++;
                        columns.AppendChild(column);
                    }

                    worksheet.AppendChild(columns);
                }

                worksheet.AppendChild(sheetData);
                if (spreadSheet.MergeCells.Count > 0)
                {
                    var mergeCells =
                        spreadSheet.MergeCells.Select(m => new MergeCell { Reference = m.ToString() }).ToList();
                    worksheet.AppendChild(new MergeCells(mergeCells) { Count = (uint)mergeCells.Count });
                }
            }

            // var sharedStringTablePart = workbookPart.AddNewPart<SharedStringTablePart>();
            workbookPart.Workbook.Save();

            document.PackageProperties.Title = book.Title;
            document.PackageProperties.Keywords = book.Keywords;
            document.PackageProperties.Creator = book.Creator;
            document.PackageProperties.Created = DateTime.Now;
            document.PackageProperties.Modified = DateTime.Now;
            document.PackageProperties.LastModifiedBy = book.Creator;
        }

        /// <summary>
        /// Sets the value of a cell.
        /// </summary>
        /// <param name="cell">
        /// The cell.
        /// </param>
        /// <param name="content">
        /// The cell content.
        /// </param>
        private static void SetCellValue(Cell cell, object content)
        {
            if (content is double)
            {
                var d = (double)content;
                cell.CellValue = new CellValue(d.ToString(CultureInfo.InvariantCulture));
                cell.DataType = CellValues.Number;
                return;
            }

            if (content is int)
            {
                var d = (int)content;
                cell.CellValue = new CellValue(d.ToString(CultureInfo.InvariantCulture));
                cell.DataType = CellValues.Number;
                return;
            }

            if (content is bool)
            {
                var d = (bool)content;
                cell.CellValue = new CellValue(d ? "1" : "0");
                cell.DataType = CellValues.Boolean;
                return;
            }

            if (content is DateTime)
            {
                var d = (DateTime)content;
                var days = (d - ExcelDayZero).TotalDays;
                cell.CellValue = new CellValue(days.ToString(CultureInfo.InvariantCulture));

                // cell.DataType = CellValues.Date;
                return;
            }

            cell.CellValue = new CellValue(content != null ? content.ToString() : null);
            cell.DataType = CellValues.String;
        }
    }
}