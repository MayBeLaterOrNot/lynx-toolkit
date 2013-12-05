namespace LynxToolkit.Documents
{
    using System.IO;

    public class XmlFormatter : IDocumentFormatter
    {
        private static readonly XmlSerializer Serializer = new XmlSerializer(
            typeof(Document),
            new[]
                {
                    typeof(BlockCollection), typeof(InlineCollection), typeof(Header), typeof(Paragraph),
                    typeof(CodeBlock), typeof(Quote), typeof(Section), typeof(HorizontalRuler), typeof(UnorderedList),
                    typeof(OrderedList), typeof(ListItem), typeof(ListItemCollection), typeof(Table),
                    typeof(TableRowCollection), typeof(TableRow), typeof(TableCellCollection), typeof(TableCell),
                    typeof(TableHeaderCell), typeof(LineBreak), typeof(Run), typeof(Emphasized), typeof(Strong),
                    typeof(Symbol), typeof(InlineCode), typeof(Anchor), typeof(Hyperlink), typeof(Image)
                });

        public void Format(Document doc, Stream s)
        {
            Serializer.Serialize(s, doc);
        }
    }
}