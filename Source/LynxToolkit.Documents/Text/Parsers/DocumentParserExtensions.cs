// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentParserExtensions.cs" company="Lynx Toolkit">
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
//   Provides extension methods for the <see cref="IDocumentParser" />s.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LynxToolkit.Documents
{
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides extension methods for the <see cref="IDocumentParser"/>s.
    /// </summary>
    public static class DocumentParserExtensions
    {
        /// <summary>
        /// Parses the specified string to a <see cref="Document"/>.
        /// </summary>
        /// <param name="parser">The parser.</param>
        /// <param name="input">The input string.</param>
        /// <returns>A <see cref="Document"/>.</returns>
        public static Document Parse(this IDocumentParser parser, string input)
        {
            using (var s = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                return parser.Parse(s);
            }
        }
    }
}