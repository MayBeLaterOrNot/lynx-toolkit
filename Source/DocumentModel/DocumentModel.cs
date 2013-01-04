using System.Xml.Serialization;

namespace LynxToolkit.Documents
{
    using System.Collections.Generic;

    public class Document
    {
        public string Creator { get; set; }
        public string Subject { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string Date { get; set; }
        public string Revision { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public StyleSheet StyleSheet { get; set; }

        [XmlArrayItem("Paragraph", typeof(Paragraph))]
        [XmlArrayItem("Header", typeof(Header))]
        [XmlArrayItem("OrderedList", typeof(OrderedList))]
        [XmlArrayItem("UnorderedList", typeof(UnorderedList))]
        [XmlArrayItem("DefinitionList", typeof(DefinitionList))]
        [XmlArrayItem("Table", typeof(Table))]
        [XmlArrayItem("Quote", typeof(Quote))]
        [XmlArrayItem("CodeBlock", typeof(CodeBlock))]
        [XmlArrayItem("HorizontalRuler", typeof(HorizontalRuler))]
        public BlockCollection Blocks { get; private set; }

        public Document()
        {
            this.Blocks = new BlockCollection();
            this.StyleSheet = new StyleSheet();
        }

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
        public Style Header1Style { get; set; }
        public Style Header2Style { get; set; }
        public Style Header3Style { get; set; }
        public Style Header4Style { get; set; }
        public Style Header5Style { get; set; }
        public Style ParagraphStyle { get; set; }
        public Style CodeStyle { get; set; }
        public Style InlineCodeStyle { get; set; }
        public Style QuoteStyle { get; set; }
        public Style UnorderedListStyle { get; set; }
        public Style OrderedListStyle { get; set; }
        public Style TableStyle { get; set; }
        public Style HyperlinkStyle { get; set; }
        public Style ImageStyle { get; set; }

        public StyleSheet()
        {
            this.Header1Style = new Style { FontFamily = "Arial", FontSize = 28, FontWeight = FontWeight.Bold };
            this.Header2Style = new Style { FontFamily = "Arial", FontSize = 20, FontWeight = FontWeight.Bold };
            this.Header3Style = new Style { FontFamily = "Arial", FontWeight = FontWeight.Bold };
            this.ParagraphStyle = null;
            this.CodeStyle = new Style { FontFamily = "Consolas" };
            this.InlineCodeStyle = new Style { FontFamily = "Consolas" };
            this.QuoteStyle = new Style { FontStyle = FontStyle.Italic, Border = Colors.Blue, BorderThickness = new Thickness { Left = 1 } };
            this.UnorderedListStyle = null;
            this.OrderedListStyle = null;
            this.TableStyle = null;
        }
    }

    public abstract class Element
    {
        public string ID { get; set; }
        public string Class { get; set; }
        public string Title { get; set; }
    }

    public abstract class Block : Element { }

    public class BlockCollection : List<Block> { }

    public class ContentBlock : Block
    {
        [XmlArrayItem("Run", typeof(Run))]
        [XmlArrayItem("Emphasized", typeof(Emphasized))]
        [XmlArrayItem("Strong", typeof(Strong))]
        [XmlArrayItem("LineBreak", typeof(LineBreak))]
        [XmlArrayItem("InlineCode", typeof(InlineCode))]
        [XmlArrayItem("Hyperlink", typeof(Hyperlink))]
        [XmlArrayItem("Image", typeof(Image))]
        [XmlArrayItem("Anchor", typeof(Anchor))]
        [XmlArrayItem("Symbol", typeof(Symbol))]
        public InlineCollection Content { get; private set; }

        public ContentBlock()
        {
            this.Content = new InlineCollection();
        }
    }

    public class HorizontalRuler : Block { }

    public class Header : ContentBlock
    {
        public int Level { get; set; }
    }

    public class Paragraph : ContentBlock { }

    public enum Language { Cs = 1, Js = 2, Xml = 3, Unknown = 0 }

    public class CodeBlock : Block
    {
        public string Text { get; set; }
        public Language Language { get; set; }
    }

    public class Quote : ContentBlock { }
    
    public class TableOfContents : Block {}
    
    public class Index : Block { }

    public class Section : Block
    {
        public BlockCollection Blocks { get; private set; }

        public Section()
        {
            this.Blocks = new BlockCollection();
        }
    }

    public class ListItemCollection : List<ListItem> { }

    public abstract class List : Block
    {
        public ListItemCollection Items { get; private set; }

        protected List()
        {
            this.Items = new ListItemCollection();
        }
    }

    public abstract class Item
    {
        public InlineCollection Content { get; private set; }

        public Item()
        {
            this.Content = new InlineCollection();
        }
    }

    public class ListItem : Item
    {
        public List NestedList { get; set; }
    }

    public class OrderedList : List { }

    public class UnorderedList : List { }

    public class DefinitionList : Block
    {
        public List<DefinitionItem> Items { get; private set; }

        public DefinitionList()
        {
            this.Items = new List<DefinitionItem>();
        }
    }

    public abstract class DefinitionItem : Item { }

    public class DefinitionTerm : DefinitionItem { }

    public class DefinitionDescription : DefinitionItem { }

    public class TableRowCollection : List<TableRow> { }

    public class Table : Block
    {
        public TableRowCollection Rows { get; private set; }

        public Table()
        {
            this.Rows = new TableRowCollection();
        }
    }

    public class TableCellCollection : List<TableCell> { }

    public class TableRow
    {
        public TableCellCollection Cells { get; private set; }

        public TableRow()
        {
            this.Cells = new TableCellCollection();
        }
    }

    public class TableCell
    {
        public InlineCollection Content { get; private set; }

        public HorizontalAlignment HorizontalAlignment { get; set; }

        public TableCell()
        {
            this.Content = new InlineCollection();
        }

        public TableCell(string content)
            : this()
        {
            this.Content.Add(new Run(content));
        }
    }

    public class TableHeaderCell : TableCell
    {
        public TableHeaderCell() { }

        public TableHeaderCell(string content)
        {
            this.Content.Add(new Run(content));
        }
    }

    public enum HorizontalAlignment { Left, Center, Right, Justify }
    public enum VerticalAlignment { Top, Middle, Bottom }
    public enum FontWeight { Normal, Bold }
    public enum FontStyle { Normal, Italic }

    public struct Thickness
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Right { get; set; }
        public double Bottom { get; set; }
    }

    public struct Color
    {
        public byte Alpha { get; set; }
        public byte Red { get; set; }
        public byte Green { get; set; }
        public byte Blue { get; set; }

        public static Color FromARGB(byte a, byte r, byte g, byte b)
        {
            return new Color() { Alpha = a, Red = r, Green = g, Blue = b };
        }
    }

    public static class Colors
    {
        private static Color blue = Color.FromARGB(255, 0, 0, 255);
        public static Color Blue { get { return blue; } }
    }

    public class Style
    {
        public Color? Foreground { get; set; }
        public Color? Background { get; set; }
        public Color? Border { get; set; }
        public Thickness? BorderThickness { get; set; }
        public string FontFamily { get; set; }
        public double? FontSize { get; set; }
        public FontWeight FontWeight { get; set; }
        public FontStyle FontStyle { get; set; }
        public Thickness? Margin { get; set; }
        public Thickness? Padding { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
    }

    public abstract class Inline : Element { }

    public class InlineCollection : List<Inline> { }

    public class LineBreak : Inline { }

    public class Anchor : Inline
    {
        public string Name { get; set; }
    }

    public class Run : Inline
    {
        public string Text { get; set; }

        public Run()
        {
        }

        public Run(string text)
        {
            this.Text = text;
        }
    }

    public abstract class InlineContent : Inline
    {
        public InlineCollection Content { get; private set; }

        protected InlineContent()
        {
            this.Content = new InlineCollection();
        }

        public InlineContent Add(params Inline[] inlines)
        {
            foreach (var i in inlines)
            {
                this.Content.Add(i);
            }
            return this;
        }
    }

    public class Emphasized : InlineContent { }

    public class Strong : InlineContent { }

    public class Symbol : Inline
    {
        public string Name { get; set; }
    }

    public class InlineCode : Inline
    {
        public string Text { get; set; }
        public string Language { get; set; }
    }

    public class Hyperlink : InlineContent
    {
        public string Url { get; set; }
    }

    public class Image : Inline
    {
        public string Source { get; set; }
        public string AlternateText { get; set; }
        public string Link { get; set; }
    }
}