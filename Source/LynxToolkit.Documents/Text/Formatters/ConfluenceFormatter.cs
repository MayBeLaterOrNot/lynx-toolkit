// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConfluenceFormatter.cs" company="Lynx Toolkit">
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

    public class ConfluenceFormatter : WikiFormatterBase
    {
        public ConfluenceFormatter()
        {
            // https://confluence.atlassian.com/display/DOC/Confluence+Wiki+Markup
            this.EscapeCharacter = '\\';
            this.UnorderedListItemPrefix = "- ";
            this.OrderedListItemPrefix = "# ";
            this.HorizontalRulerText = "----";
            this.LineBreakText = @"\\";
            this.StrongWrapper = "*";
            this.EmphasizedWrapper = "_";
            this.HeaderPrefix = "#";
            this.InlineCodePrefix = "{{";
            this.InlineCodeSuffix = "}}";
            this.TableHeaderPrefix = "||";
            this.TableCellSeparator = "|";
        }

        protected override void Write(Header header, TextWriter context)
        {
            context.Write("h{0}.", header.Level);
            this.WriteInlines(header.Content, context);
            context.WriteLine();
            context.WriteLine();
        }

        protected override void Write(Quote quote, TextWriter context)
        {
            context.WriteLine("///");
            this.WriteInlines(quote.Content, context);
            context.WriteLine();
            context.WriteLine("///");
            context.WriteLine();
        }

        protected override void Write(CodeBlock codeBlock, TextWriter context)
        {
            context.Write("{code");
            if (codeBlock.Language != Language.Unknown)
            {
                context.Write(":{0}", codeBlock.Language);
            }

            context.WriteLine("}");
            foreach (var line in codeBlock.Text.Split('\n'))
            {
                context.WriteLine(line.Trim('\r'));
            }

            context.WriteLine("{code}");
            context.WriteLine();
        }

        protected override void Write(Hyperlink hyperlink, TextWriter context)
        {
            context.Write("[");
            if (hyperlink.Content.Count > 0)
            {
                this.WriteInlines(hyperlink.Content, context);
                context.Write("|");
            }

            context.Write(hyperlink.Url);
            context.Write("]");
        }

        protected override void Write(Image image, TextWriter context)
        {
            context.Write("!{0}", image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                context.Write("|alt={0}", image.AlternateText);
            }

            context.Write("!");
        }
    }
}