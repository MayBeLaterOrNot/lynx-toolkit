// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlFormatter.cs" company="Lynx Toolkit">
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