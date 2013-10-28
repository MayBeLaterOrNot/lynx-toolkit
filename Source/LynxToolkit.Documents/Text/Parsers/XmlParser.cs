// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlParser.cs" company="Lynx Toolkit">
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
    using System.Xml.Serialization;

    public class XmlParser : IDocumentParser
    {
        /// <summary>
        /// The serializer.
        /// </summary>
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

        /// <summary>
        /// Parses a <see cref="Document" /> from the specified <see cref="Stream" />.
        /// </summary>
        /// <param name="stream">The input <see cref="Stream" />.</param>
        /// <returns>
        /// A <see cref="Document" />
        /// </returns>
        public Document Parse(Stream stream)
        {
            return Serializer.Deserialize(stream) as Document;
        }
    }
}