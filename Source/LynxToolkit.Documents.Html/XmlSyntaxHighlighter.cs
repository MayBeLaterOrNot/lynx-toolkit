// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XmlSyntaxHighlighter.cs" company="Lynx Toolkit">
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
//   Provides a simple Xml syntax highlighter.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LynxToolkit.Documents.Html
{
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Provides a simple Xml syntax highlighter.
    /// </summary>
    public static class XmlSyntaxHighlighter
    {
        /// <summary>
        /// The expression
        /// </summary>
        private static readonly Regex Expression;

        /// <summary>
        /// Initializes static members of the <see cref="XmlSyntaxHighlighter"/> class. 
        /// </summary>
        static XmlSyntaxHighlighter()
        {
            var b = new StringBuilder();
            b.Append("(?:");
            b.Append("(?<starttag> &lt;/? )");
            b.Append("(?<element>[\\w.:]+ )");
            b.Append("(?<attribute>\\s+[\\w.:]+=\"[^\"]*\" )*");
            b.Append("(?<endtag> \\s* /? &gt; )");
            b.Append(")");
            Expression = new Regex(
                b.ToString(),
                RegexOptions.IgnorePatternWhitespace | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.CultureInvariant);
        }

        /// <summary>
        /// Applies Xml syntax highlighting to the specified text.
        /// </summary>
        /// <param name="text">
        /// The encoded html.
        /// </param>
        /// <returns>
        /// Html with highlight elements.
        /// </returns>
        public static string Highlight(string text)
        {
            return Expression.Replace(text, ReplaceMatch);
        }

        /// <summary>
        /// Replaces a match.
        /// </summary>
        /// <param name="match">
        /// The match.
        /// </param>
        /// <returns>
        /// The replacement string.
        /// </returns>
        private static string ReplaceMatch(Match match)
        {
            var b = new StringBuilder();
            var startTagGroup = match.Groups["starttag"];
            if (startTagGroup.Success)
            {
                b.Append("<span class=\"tag\">" + startTagGroup.Value + "</span>");
            }

            var elementNameGroup = match.Groups["element"];
            if (elementNameGroup.Success)
            {
                b.Append("<span class=\"name\">" + elementNameGroup.Value + "</span>");
            }

            var attributeGroup = match.Groups["attribute"];
            foreach (Capture attribute in attributeGroup.Captures)
            {
                var i = attribute.Value.IndexOf('=');
                var key = attribute.Value.Substring(0, i);
                var value = attribute.Value.Substring(i);
                b.Append("<span class=\"key\">" + key + "</span>");
                b.Append("<span class=\"value\">" + value + "</span>");
            }

            var endTagGroup = match.Groups["endtag"];
            if (endTagGroup.Success)
            {
                b.Append("<span class=\"tag\">" + endTagGroup.Value + "</span>");
            }

            return b.ToString();
        }
    }
}