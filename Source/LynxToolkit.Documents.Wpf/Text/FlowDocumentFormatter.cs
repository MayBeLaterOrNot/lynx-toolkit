// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlowDocumentFormatter.cs" company="Lynx Toolkit">
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
namespace LynxToolkit.Documents.Wpf
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Shapes;

    using LynxToolkit.Documents;

    using Block = System.Windows.Documents.Block;
    using Color = System.Windows.Media.Color;
    using Hyperlink = System.Windows.Documents.Hyperlink;
    using Image = System.Windows.Controls.Image;
    using InlineCollection = System.Windows.Documents.InlineCollection;
    using LineBreak = System.Windows.Documents.LineBreak;
    using List = System.Windows.Documents.List;
    using ListItem = System.Windows.Documents.ListItem;
    using Paragraph = System.Windows.Documents.Paragraph;
    using Run = System.Windows.Documents.Run;
    using Span = LynxToolkit.Documents.Span;
    using Style = System.Windows.Style;
    using Table = System.Windows.Documents.Table;
    using TableCell = System.Windows.Documents.TableCell;
    using TableRow = System.Windows.Documents.TableRow;
    using Thickness = System.Windows.Thickness;

    public class FlowDocumentFormatter : DocumentFormatter<object>
    {
        public FlowDocument Document { get; private set; }

        private Dictionary<LynxToolkit.Documents.Style, Style> styles = new Dictionary<LynxToolkit.Documents.Style, Style>();

        public FlowDocumentFormatter()
        {
        }

        private Style Format(LynxToolkit.Documents.Style style)
        {
            if (style == null)
            {
                return null;
            }

            Style s;
            if (this.styles.TryGetValue(style, out s))
            {
                return s;
            }

            s = CreateStyle(style);
            this.styles[style] = s;
            return s;
        }

        private static Style CreateStyle(LynxToolkit.Documents.Style style)
        {
            var s = new Style();
            if (style.FontFamily != null)
            {
                s.Setters.Add(new Setter(TextElement.FontFamilyProperty, new FontFamily(style.FontFamily)));
            }

            if (style.FontSize != null)
            {
                s.Setters.Add(new Setter(TextElement.FontSizeProperty, style.FontSize.Value));
            }

            if (style.Foreground != null)
            {
                s.Setters.Add(new Setter(TextElement.ForegroundProperty, ToBrush(style.Foreground.Value)));
            }

            if (style.Background != null)
            {
                s.Setters.Add(new Setter(TextElement.BackgroundProperty, ToBrush(style.Background.Value)));
            }

            if (style.FontStyle == LynxToolkit.Documents.FontStyle.Italic)
            {
                s.Setters.Add(new Setter(TextElement.FontStyleProperty, FontStyles.Italic));
            }

            if (style.FontWeight == LynxToolkit.Documents.FontWeight.Bold)
            {
                s.Setters.Add(new Setter(TextElement.FontWeightProperty, FontWeights.Bold));
            }

            if (style.Margin != null)
            {
                s.Setters.Add(new Setter(Block.MarginProperty, ToThickness(style.Margin.Value)));
            }

            return s;
        }

        private static Thickness ToThickness(LynxToolkit.Documents.Thickness t)
        {
            return new Thickness(t.Left, t.Top, t.Right, t.Bottom);
        }

        private static Brush ToBrush(LynxToolkit.Documents.Color c)
        {
            return new SolidColorBrush(Color.FromArgb(c.Alpha, c.Red, c.Green, c.Blue));
        }

        public override void Format(LynxToolkit.Documents.Document doc, Stream stream)
        {
            this.Source = doc;
            this.Document = new FlowDocument();
            if (this.StyleSheet == null) this.StyleSheet = new StyleSheet();
            this.WriteBlocks(doc.Blocks, null);

            // TODO...
        }

        protected override void Write(LynxToolkit.Documents.Header header, object parent)
        {
            var p = new Paragraph();
            WriteInlines(header.Content, p.Inlines);
            p.Style = this.Format(GetStyle(header));
            Document.Blocks.Add(p);
        }

        protected override void Write(LynxToolkit.Documents.Section section, object parent)
        {
            var p = new System.Windows.Documents.Section();
            WriteBlocks(section.Blocks, p.Blocks);
            // p.Style = this.Format(GetStyle(header));
            Document.Blocks.Add(p);
        }

        protected override void Write(TableOfContents toc, object parent)
        {
        }

        private Stack<InlineCollection> outputStack = new Stack<InlineCollection>();

        //InlineCollection Current { get { return outputStack.Peek(); } }

        //private void Push(InlineCollection collection)
        //{
        //    outputStack.Push(collection);
        //}

        //private void Pop()
        //{
        //    outputStack.Pop();
        //}

        protected override void Write(LynxToolkit.Documents.Paragraph paragraph, object parent)
        {
            var p = new Paragraph();
            WriteInlines(paragraph.Content, p.Inlines);
            Document.Blocks.Add(p);
            p.Style = this.Format(GetStyle(paragraph));
        }

        protected override void Write(LynxToolkit.Documents.UnorderedList list, object parent)
        {
            var p = new List();
            foreach (var item in list.Items)
                Write(item, p);
            Document.Blocks.Add(p);
            p.Style = this.Format(GetStyle(list));
        }

        protected override void Write(LynxToolkit.Documents.OrderedList list, object parent)
        {
            var p = new List();
            p.MarkerStyle = TextMarkerStyle.Decimal;
            foreach (var item in list.Items)
                Write(item, p);
            Document.Blocks.Add(p);
            p.Style = this.Format(GetStyle(list));
        }

        protected override void Write(LynxToolkit.Documents.ListItem item, object parent)
        {
            var li = new ListItem();
            var p = new Paragraph();
            li.Blocks.Add(p);
            WriteInlines(item.Content, p.Inlines);
            // Write(item.NestedList);

            ((List)parent).ListItems.Add(li);
        }

        protected override void Write(LynxToolkit.Documents.Symbol symbol, object parent)
        {
            var path = this.ResolveSymbolPath(symbol);
            var img = new LynxToolkit.Documents.Image { Source = path };
            Write(img, parent);
        }

        protected override void Write(LynxToolkit.Documents.Anchor anchor, object parent)
        {
            ((InlineCollection)parent).Add(new Run { Name = anchor.Name });
        }

        protected override void Write(LynxToolkit.Documents.Table table, object parent)
        {
            var t = new Table();
            t.CellSpacing = 0;
            var trg = new TableRowGroup();
            t.Columns.Add(new TableColumn() { Width = new GridLength(80) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(80) });
            t.Columns.Add(new TableColumn() { Width = new GridLength(80) });
            t.RowGroups.Add(trg);
            int i = 0;
            foreach (var row in table.Rows)
            {
                i++;
                var tr = new TableRow();
                trg.Rows.Add(tr);
                int j = 0;
                foreach (var cell in row.Cells)
                {
                    j++;
                    var td = new TableCell();
                    td.BorderThickness = new Thickness(1, 1, j == row.Cells.Count ? 1 : 0, i == table.Rows.Count ? 1 : 0);
                    td.BorderBrush = Brushes.Gray;
                    td.Background = cell is LynxToolkit.Documents.TableHeaderCell ? Brushes.LightGray : null;

                    WriteBlocks(cell.Blocks, td.Blocks);

                    if (cell.HorizontalAlignment == LynxToolkit.Documents.HorizontalAlignment.Center)
                        td.TextAlignment = TextAlignment.Center;
                    if (cell.HorizontalAlignment == LynxToolkit.Documents.HorizontalAlignment.Right)
                        td.TextAlignment = TextAlignment.Right;
                    tr.Cells.Add(td);
                }
            }
            Document.Blocks.Add(t);
            t.Style = this.Format(GetStyle(table));

        }

        protected override void Write(LynxToolkit.Documents.Quote quote, object parent)
        {
            var p = new Paragraph();
            WriteInlines(quote.Content, p.Inlines);
            Document.Blocks.Add(p);
            p.Style = this.Format(GetStyle(quote));
        }

        protected override void Write(LynxToolkit.Documents.CodeBlock codeBlock, object parent)
        {
            var p = new Paragraph();
            Write(new LynxToolkit.Documents.Run(codeBlock.Text), p.Inlines);
            Document.Blocks.Add(p);
            p.Style = this.Format(GetStyle(codeBlock));
        }

        protected override void Write(LynxToolkit.Documents.HorizontalRuler ruler, object parent)
        {
            var line = new Rectangle() { Fill = Brushes.Black, Height = 1, Margin = new Thickness(4), SnapsToDevicePixels = true };
            Document.Blocks.Add(new BlockUIContainer(line));
        }

        protected override void Write(NonBreakingSpace nbsp, object parent)
        {
            ((InlineCollection)parent).Add(new Run(" "));
        }

        protected override void Write(LynxToolkit.Documents.Run run, object parent)
        {
            ((InlineCollection)parent).Add(new Run(run.Text));
        }

        protected override void Write(LynxToolkit.Documents.Span span, object parent)
        {
            var element = new System.Windows.Documents.Span();
            this.SetElementProperties(span, element);
            this.WriteInlines(span.Content, element.Inlines);
            ((InlineCollection)parent).Add(element);
        }

        private void SetElementProperties(Element x, FrameworkContentElement element)
        {
            element.Name = x.ID;
            element.ToolTip = x.Title;
            element.Style = this.GetElementStyle(x.Class);
        }

        private Style GetElementStyle(string name)
        {
            // TODO
            return null;
        }

        protected override void Write(LynxToolkit.Documents.Strong strong, object parent)
        {
            var element = new Bold();
            this.SetElementProperties(strong, element);
            WriteInlines(strong.Content, element.Inlines);
            ((InlineCollection)parent).Add(element);
        }

        protected override void Write(LynxToolkit.Documents.Emphasized em, object parent)
        {
            var element = new Italic();
            this.SetElementProperties(em, element);
            WriteInlines(em.Content, element.Inlines);
            ((InlineCollection)parent).Add(element);
        }

        protected override void Write(LynxToolkit.Documents.LineBreak linebreak, object parent)
        {
            var br = new LineBreak();
            ((InlineCollection)parent).Add(br);
        }

        protected override void Write(LynxToolkit.Documents.InlineCode inlineCode, object parent)
        {
            var run = new Run(inlineCode.Code);
            run.Style = this.Format(GetStyle(inlineCode));
            ((InlineCollection)parent).Add(run);
        }

        protected override void Write(LynxToolkit.Documents.Hyperlink hyperlink, object parent)
        {
            var a = new Hyperlink();
            a.Style = this.Format(GetStyle(hyperlink));
            if (!string.IsNullOrWhiteSpace(hyperlink.Title)) a.ToolTip = hyperlink.Title;
            a.Click += (s, e) => MessageBox.Show(hyperlink.Url);
            WriteInlines(hyperlink.Content, a.Inlines);
            ((InlineCollection)parent).Add(a);
        }

        protected override void Write(Documents.Equation equation, object parent)
        {
        }


        protected override void Write(LynxToolkit.Documents.Image image, object parent)
        {
            var src = new BitmapImage();

            src.BeginInit();
            src.UriSource = new Uri(image.Source, image.Source.StartsWith("http") ? UriKind.Absolute : UriKind.Relative);
            src.EndInit();

            var img = new Image
                          {
                              Source = src,
                              Stretch = Stretch.None,
                              Width = src.Width,
                              Height = src.Height,
                              Style = this.Format(GetStyle(image))
                          };

            if (!string.IsNullOrWhiteSpace(image.AlternateText))
                img.ToolTip = image.AlternateText;

            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                var a = new Hyperlink();
                a.Inlines.Add(img);
                ((InlineCollection)parent).Add(a);
            }
            else
            {
                ((InlineCollection)parent).Add(img);
            }
        }
    }
}