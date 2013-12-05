// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentModel.cs" company="Lynx Toolkit">
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
namespace LynxToolkit.Documents
{
    using System;
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Document
    {
        public Document()
        {
            this.Blocks = new BlockCollection();
        }

        [XmlArrayItem("Paragraph", typeof(Paragraph))]
        [XmlArrayItem("Header", typeof(Header))]
        [XmlArrayItem("OrderedList", typeof(OrderedList))]
        [XmlArrayItem("UnorderedList", typeof(UnorderedList))]
        [XmlArrayItem("DefinitionList", typeof(DefinitionList))]
        [XmlArrayItem("Table", typeof(Table))]
        [XmlArrayItem("Quote", typeof(Quote))]
        [XmlArrayItem("CodeBlock", typeof(CodeBlock))]
        [XmlArrayItem("HorizontalRuler", typeof(HorizontalRuler))]
        [XmlArrayItem("Section", typeof(Section))]
        public BlockCollection Blocks { get; private set; }

        public string Category { get; set; }

        public string Creator { get; set; }

        public string Date { get; set; }

        public string Description { get; set; }

        public string Keywords { get; set; }

        public string Revision { get; set; }

        public string Subject { get; set; }

        public string Title { get; set; }

        public string Subtitle { get; set; }

        public string Version { get; set; }

        public void Append(Document doc)
        {
            foreach (var block in doc.Blocks)
            {
                this.Blocks.Add(block);
            }
        }
    }

    public class StyleSheet
    {
        public StyleSheet()
        {
            this.HeaderStyles = new Style[5];
            this.HeaderStyles[0] = new Style { FontFamily = "Arial", FontSize = 28, FontWeight = FontWeight.Bold, PageBreakBefore = true };
            this.HeaderStyles[1] = new Style { FontFamily = "Arial", FontSize = 20, FontWeight = FontWeight.Bold };
            this.HeaderStyles[2] = new Style { FontFamily = "Arial", FontWeight = FontWeight.Bold };
            this.ParagraphStyle = null;
            this.CodeStyle = new Style { FontFamily = "Consolas" };
            this.InlineCodeStyle = new Style { FontFamily = "Consolas" };
            this.QuoteStyle = new Style
                                  {
                                      FontStyle = FontStyle.Italic,
                                      Border = Colors.Blue,
                                      BorderThickness = new Thickness { Left = 1 }
                                  };
            this.UnorderedListStyle = null;
            this.OrderedListStyle = null;
            this.TableStyle = null;
            this.TableHeaderStyle = new Style { FontWeight = FontWeight.Bold };
        }

        public Style CodeStyle { get; set; }

        public Style[] HeaderStyles { get; set; }

        public Style HyperlinkStyle { get; set; }

        public Style ImageStyle { get; set; }

        public Style InlineCodeStyle { get; set; }

        public Style OrderedListStyle { get; set; }

        public Style ParagraphStyle { get; set; }

        public Style QuoteStyle { get; set; }

        public Style TableStyle { get; set; }

        public Style TableHeaderStyle { get; set; }

        public Style UnorderedListStyle { get; set; }
    }

    public abstract class Element
    {
        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <remarks>
        /// For HTML output, this will be written to the class attribute.
        /// For Word output, this will override the style of the element.
        /// </remarks>
        public string Class { get; set; }

        /// <summary>
        /// Gets or sets the ID.
        /// </summary>
        /// <remarks>
        /// The ID can be used to create references. 
        /// For HTML output, this will be written to the id attribute.
        /// </remarks>
        public string ID { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }
    }

    public abstract class Block : Element
    {
    }

    public class BlockCollection : List<Block>
    {
    }

    public class ContentBlock : Block
    {
        public ContentBlock()
        {
            this.Content = new InlineCollection();
        }

        [XmlArrayItem("Run", typeof(Run))]
        [XmlArrayItem("Emphasized", typeof(Emphasized))]
        [XmlArrayItem("Strong", typeof(Strong))]
        [XmlArrayItem("LineBreak", typeof(LineBreak))]
        [XmlArrayItem("InlineCode", typeof(InlineCode))]
        [XmlArrayItem("Hyperlink", typeof(Hyperlink))]
        [XmlArrayItem("Image", typeof(Image))]
        [XmlArrayItem("Anchor", typeof(Anchor))]
        [XmlArrayItem("Symbol", typeof(Symbol))]
        [XmlArrayItem("Span", typeof(Span))]
        [XmlArrayItem("Equation", typeof(Equation))]
        [XmlArrayItem("NonBreakingSpace", typeof(NonBreakingSpace))]
        public InlineCollection Content { get; private set; }
    }

    public class HorizontalRuler : Block
    {
    }

    public class NonBreakingSpace : Inline
    {
    }

    public class Header : ContentBlock
    {
        public Header()
        {
            this.Level = 1;
        }

        public Header(int level, params Inline[] inlines)
        {
            this.Level = level;
            this.Content.AddRange(inlines);
        }

        public int Level { get; set; }
    }

    public class Paragraph : ContentBlock
    {
        public Paragraph()
        {
        }

        public Paragraph(params Inline[] inlines)
        {
            this.Content.AddRange(inlines);
        }
    }

    public enum Language
    {
        /// <summary>
        /// C# language
        /// </summary>
        Cs = 1,

        /// <summary>
        /// JavaScript language
        /// </summary>
        Js = 2,

        /// <summary>
        /// XML/XAML language
        /// </summary>
        Xml = 3,

        /// <summary>
        /// C language
        /// </summary>
        C = 4,

        /// <summary>
        /// C++ language
        /// </summary>
        Cpp = 5,

        /// <summary>
        /// Fortran language
        /// </summary>
        Fortran = 5,

        /// <summary>
        /// Unknown language
        /// </summary>
        Unknown = 0
    }

    public class CodeBlock : Block
    {
        public CodeBlock()
        {
        }

        public CodeBlock(string language, string code)
        {
            if (!string.IsNullOrEmpty(language))
            {
                Language l;
                if (Enum.TryParse(language, true, out l))
                {
                    this.Language = l;
                }
            }

            this.Text = code;
        }

        public Language Language { get; set; }

        public string Text { get; set; }
    }

    public class Quote : ContentBlock
    {
    }

    public class TableOfContents : Block
    {
        public int Levels { get; set; }

        public TableOfContents()
        {
            this.Levels = 3;
        }
    }

    public class Index : Block
    {
    }

    public class Section : Block
    {
        public Section()
        {
            this.Blocks = new BlockCollection();
        }

        public BlockCollection Blocks { get; private set; }
    }

    public class ListItemCollection : List<ListItem>
    {
    }

    public abstract class List : Block
    {
        protected List()
        {
            this.Items = new ListItemCollection();
        }

        public ListItemCollection Items { get; private set; }
    }

    public abstract class Item
    {
        public Item()
        {
            this.Content = new InlineCollection();
        }

        public InlineCollection Content { get; private set; }
    }

    public class ListItem : Item
    {
        public List NestedList { get; set; }
    }

    public class OrderedList : List
    {
    }

    public class UnorderedList : List
    {
    }

    public class DefinitionList : Block
    {
        public DefinitionList()
        {
            this.Items = new List<DefinitionItem>();
        }

        public List<DefinitionItem> Items { get; private set; }
    }

    public abstract class DefinitionItem : Item
    {
    }

    public class DefinitionTerm : DefinitionItem
    {
    }

    public class DefinitionDescription : DefinitionItem
    {
    }

    public class TableRowCollection : List<TableRow>
    {
    }

    public class Table : Block
    {
        public Table()
        {
            this.Rows = new TableRowCollection();
        }

        public TableRowCollection Rows { get; private set; }
    }

    public class TableCellCollection : List<TableCell>
    {
    }

    public class TableRow
    {
        public TableRow()
        {
            this.Cells = new TableCellCollection();
        }

        public TableCellCollection Cells { get; private set; }
    }

    public class TableCell
    {
        public TableCell()
        {
            this.Blocks = new BlockCollection();
            this.RowSpan = 1;
            this.ColumnSpan = 1;
        }

        public TableCell(string content)
            : this()
        {
            this.Blocks.Add(new Paragraph(new Run(content)));
        }

        public BlockCollection Blocks { get; private set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public int ColumnSpan { get; set; }
        public int RowSpan { get; set; }
    }

    public class TableHeaderCell : TableCell
    {
        public TableHeaderCell()
        {
        }

        public TableHeaderCell(string content)
        {
            this.Blocks.Add(new Paragraph(new Run(content)));
        }
    }

    public enum HorizontalAlignment
    {
        Left,

        Center,

        Right,

        Justify
    }

    public enum VerticalAlignment
    {
        Top,

        Middle,

        Bottom
    }

    public enum FontWeight
    {
        Normal,

        Bold
    }

    public enum FontStyle
    {
        Normal,

        Italic
    }

    public struct Thickness
    {
        public double Bottom { get; set; }

        public double Left { get; set; }

        public double Right { get; set; }

        public double Top { get; set; }
    }

    public struct Color
    {
        public byte Alpha { get; set; }

        public byte Blue { get; set; }

        public byte Green { get; set; }

        public byte Red { get; set; }

        public static Color FromARGB(byte a, byte r, byte g, byte b)
        {
            return new Color { Alpha = a, Red = r, Green = g, Blue = b };
        }
    }

    public static class Colors
    {
        private static readonly Color blue = Color.FromARGB(0xFF, 0x00, 0x00, 0xFF);

        public static Color Blue
        {
            get
            {
                return blue;
            }
        }
    }

    public class Style
    {
        public Color? Background { get; set; }

        public Color? Border { get; set; }

        public Thickness? BorderThickness { get; set; }

        public string FontFamily { get; set; }

        public double? FontSize { get; set; }

        public FontStyle FontStyle { get; set; }

        public FontWeight FontWeight { get; set; }

        public Color? Foreground { get; set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public Thickness? Margin { get; set; }

        public Thickness? Padding { get; set; }

        public VerticalAlignment VerticalAlignment { get; set; }

        public bool? PageBreakBefore { get; set; }
    }

    public abstract class Inline : Element
    {
    }

    public class InlineCollection : List<Inline>
    {
    }

    public class LineBreak : Inline
    {
    }

    public class Anchor : Inline
    {
        public string Name { get; set; }
    }

    public class Run : Inline
    {
        public Run()
        {
        }

        public Run(string text)
        {
            this.Text = text;
        }

        public string Text { get; set; }

        public override string ToString()
        {
            return Text;
        }
    }

    public class Equation : Inline
    {
        public Equation()
        {
        }

        public Equation(string content)
        {
            this.Content = content;
        }

        public string Content { get; set; }
    }

    public abstract class InlineContent : Inline
    {
        protected InlineContent(params Inline[] inlines)
        {
            this.Content = new InlineCollection();
            this.Content.AddRange(inlines);
        }

        public InlineCollection Content { get; private set; }

        public InlineContent Add(params Inline[] inlines)
        {
            foreach (var i in inlines)
            {
                this.Content.Add(i);
            }

            return this;
        }

        public void Add(string text)
        {
            this.Add(new Run(text));
        }
    }

    public class Emphasized : InlineContent
    {
        public Emphasized()
        {
        }

        public Emphasized(string text)
        {
            this.Content.Add(new Run(text));
        }
    }

    public class Span : InlineContent
    {
    }

    public class Strong : InlineContent
    {
        public Strong()
        {
        }

        public Strong(string text)
        {
            this.Content.Add(new Run(text));
        }
    }

    public class Symbol : Inline
    {
        public string Name { get; set; }
    }

    public class InlineCode : Inline
    {
        public InlineCode()
        {
        }

        public InlineCode(string code, Language language = Language.Cs)
        {
            this.Code = code;
            this.Language = language;
        }

        public Language Language { get; set; }

        public string Code { get; set; }
    }

    public class Hyperlink : InlineContent
    {
        public Hyperlink()
        {
        }

        public Hyperlink(string url, params Inline[] inlines)
            : base(inlines)
        {
            this.Url = url;
        }

        public string Url { get; set; }
    }

    public class Image : Inline
    {
        public Image()
        {
        }

        public Image(string source, string alt = null, string link = null)
        {
            this.Source = source;
            this.AlternateText = alt;
            this.Link = link;
        }

        public string AlternateText { get; set; }

        public string Link { get; set; }

        public string Source { get; set; }
    }
}