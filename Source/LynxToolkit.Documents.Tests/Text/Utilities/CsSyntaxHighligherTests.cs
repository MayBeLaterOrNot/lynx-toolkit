// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiTests.cs" company="Lynx Toolkit">
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
namespace LynxToolkit.Documents.Tests
{
    using LynxToolkit.Documents.Html;

    using NUnit.Framework;

    [TestFixture]
    public class CsSyntaxHighlighterTests
    {
        [Test]
        public void Strings()
        {
            Assert.AreEqual("s = <span class=\"string\">\"abc\"</span>;", CsSyntaxHighlighter.Highlight("s = \"abc\";"));
            Assert.AreEqual("s = <span class=\"string\">@\"abc\"</span>;", CsSyntaxHighlighter.Highlight("s = @\"abc\";"));
            Assert.AreEqual("s = <span class=\"string\">@\"ab\"\"c\"</span>;", CsSyntaxHighlighter.Highlight("s = @\"ab\"\"c\";"));
            Assert.AreEqual("s = <span class=\"string\">\"ab\\\"c\"</span>;", CsSyntaxHighlighter.Highlight("s = \"ab\\\"c\";"));
        }

        [Test]
        public void Keywords()
        {
            Assert.AreEqual("<span class=\"keyword\">string</span> s;", CsSyntaxHighlighter.Highlight("string s;"));
        }

        [Test]
        public void Comments()
        {
            Assert.AreEqual("<span class=\"comment\">/* comment */</span>", CsSyntaxHighlighter.Highlight("/* comment */"));
            Assert.AreEqual("<span class=\"comment\">// comment</span>", CsSyntaxHighlighter.Highlight("// comment"));
        }

        [Test]
        public void Documentation()
        {
            Assert.AreEqual("<span class=\"doc\">/// text</span>", CsSyntaxHighlighter.Highlight("/// text"));
            Assert.AreEqual("<span class=\"doc\">/// &lt;summary&gt;text&lt;/summary&gt;</span>", CsSyntaxHighlighter.Highlight("/// &lt;summary&gt;text&lt;/summary&gt;"));
        }
    }
}