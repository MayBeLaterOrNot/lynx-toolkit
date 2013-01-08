namespace LynxToolkit.Documents
{
    using System.IO;
    using System.Text;
    using System.Xml.Serialization;

    public class XmlFormatter
    {
        static XmlSerializer serializer = new XmlSerializer(
            typeof(Document),
            new[] {
                          typeof(BlockCollection), typeof(InlineCollection),
                          typeof(Header),typeof(Paragraph),typeof(CodeBlock), typeof(Quote),typeof(Section),typeof(HorizontalRuler),
                          typeof(UnorderedList),typeof(OrderedList),typeof(ListItem),typeof(ListItemCollection),
                          typeof(Table),typeof(TableRowCollection), typeof(TableRow),typeof(TableCellCollection), typeof(TableCell),typeof(TableHeaderCell),
                          typeof(LineBreak),typeof(Run),typeof(Emphasized),typeof(Strong),typeof(Symbol),typeof(InlineCode),typeof(Anchor),typeof(Hyperlink),typeof(Image)});

        public static string Format(Document doc)
        {

            var ms = new MemoryStream();
            serializer.Serialize(ms, doc);
            return Encoding.UTF8.GetString(ms.ToArray());
        }

        public static Document Parse(Stream s)
        {
            return serializer.Deserialize(s) as Document;
        }

        public static Document Parse(string xml)
        {
            var ms = new MemoryStream(Encoding.UTF8.GetBytes(xml));
            return serializer.Deserialize(ms) as Document;
        }
    }
}