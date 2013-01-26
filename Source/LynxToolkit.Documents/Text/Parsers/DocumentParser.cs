// --------------------------------------------------------------------------------------------------------------------
// <copyright file="DocumentParser.cs" company="Lynx Toolkit">
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
//   Creates a regular expression.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents
{
    using System.Text.RegularExpressions;

    public abstract class DocumentParser
    {
        protected Document doc;

        public DocumentParser()
        {
            doc = new Document();
        }

        public Document Document
        {
            get { return doc; }
        }

        /// <summary>
        /// Creates a regular expression.
        /// </summary>
        /// <param name="s">The expression.</param>
        /// <param name="multiline">Multiline mode. Changes the meaning of ^ and$ so they match at the beginning and end, respectively, of any line, and not just the beginning and end of the entire string.</param>
        /// <param name="singleline">Specifies single-line mode. Changes the meaning of the dot (.) so it matches every character (instead of every character except \n).</param>
        /// <returns>The compiled regular expression.</returns>
        public static Regex CreateRegex(string s, bool multiline = true, bool singleline = true)
        {
            var o = RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled | RegexOptions.CultureInvariant;
            if (multiline)
                o |= RegexOptions.Multiline;
            if (singleline)
                o |= RegexOptions.Singleline;
            return new Regex(s, o);
        }

    }
}