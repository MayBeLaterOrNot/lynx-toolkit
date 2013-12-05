// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarkdownFormatter.cs" company="Lynx Toolkit">
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
//   Provides an <see cref="IDocumentFormatter" /> with Markdown syntax.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LynxToolkit.Documents
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// Provides an <see cref="IDocumentFormatter"/> with Markdown syntax.
    /// </summary>
    public class MarkdownFormatter : WikiFormatterBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MarkdownFormatter"/> class.
        /// </summary>
        public MarkdownFormatter()
        {
            this.EscapeCharacter = '\\';

            // http://daringfireball.net/projects/markdown/syntax#list
            this.UnorderedListItemPrefix = "- ";
            this.OrderedListItemPrefix = "i. ";

            // http://daringfireball.net/projects/markdown/syntax#hr
            this.HorizontalRulerText = "- - -";

            // http://daringfireball.net/projects/markdown/syntax#p
            // https://help.github.com/articles/github-flavored-markdown
            this.LineBreakText = "  ";

            // http://daringfireball.net/projects/markdown/syntax#em
            this.StrongWrapper = "**";
            this.EmphasizedWrapper = "*";

            // http://daringfireball.net/projects/markdown/syntax#header
            this.HeaderPrefix = "#";

            // http://daringfireball.net/projects/markdown/syntax#code
            this.InlineCodePrefix = this.InlineCodeSuffix = "`";
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1126:PrefixCallsCorrectly", Justification = "Reviewed. Suppression is OK here.")]
        protected override void Write(Header header, TextWriter context)
        {
            // http://daringfireball.net/projects/markdown/syntax#header
            if (header.Level > 2)
            {
                base.Write(header, context);
                return;
            }

            var sw = context as StreamWriter;
            if (sw != null)
            {
                sw.Flush();
            }

            var p0 = sw != null ? sw.BaseStream.Position : 0;
            this.WriteInlines(header.Content, context);
            if (sw != null)
            {
                sw.Flush();
            }

            var headerLength = (int)(sw != null ? sw.BaseStream.Position - p0 : 20);
            context.WriteLine();

            if (header.Level == 1)
            {
                context.WriteLine(Repeat("=", headerLength));
                context.WriteLine();
            }

            if (header.Level == 2)
            {
                context.WriteLine(Repeat("-", headerLength));
                context.WriteLine();
            }

            context.WriteLine();
        }

        protected override void Write(Hyperlink hyperlink, TextWriter context)
        {
            // http://daringfireball.net/projects/markdown/syntax#link
            context.Write("[");
            this.WriteInlines(hyperlink.Content, context);
            context.Write("]");
            context.Write("(");
            context.Write(hyperlink.Url);
            if (hyperlink.Title != null)
            {
                context.Write(" \"{0}\"", hyperlink.Title);
            }

            context.Write(")");
        }

        protected override void Write(Image image, TextWriter context)
        {
            // http://daringfireball.net/projects/markdown/syntax#img
            context.Write("!");
            context.Write("[{0}]", image.AlternateText);
            context.Write("(");
            context.Write(image.Source);
            if (image.Title != null)
            {
                context.Write(" \"{0}\"", image.Title);
            }

            context.Write(")");
        }

        protected override void Write(CodeBlock codeBlock, TextWriter context)
        {
            // http://daringfireball.net/projects/markdown/syntax#precode
            this.WriteLines(context, codeBlock.Text, "    ");
            context.WriteLine();
        }

        protected override void Write(Quote quote, TextWriter context)
        {
            // http://daringfireball.net/projects/markdown/syntax#blockquote
            var sb = new StringWriter();
            this.WriteInlines(quote.Content, sb);
            var text = Wrap(sb.ToString(), LineLength);

            context.WriteLine();
            this.WriteLines(context, text, "> ");
            context.WriteLine();
        }

        protected override void Write(Table table, TextWriter context)
        {
            // https://github.com/adam-p/markdown-here/wiki/Markdown-Cheatsheet
            // https://github.com/fletcher/MultiMarkdown/wiki/MultiMarkdown-Syntax-Guide
            // http://fletcherpenney.net/multimarkdown/
            base.Write(table, context);
        }
    }
}