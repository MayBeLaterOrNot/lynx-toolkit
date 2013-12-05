// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CodeplexFormatter.cs" company="Lynx Toolkit">
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

    public class CodeplexFormatter : WikiFormatterBase
    {
        public CodeplexFormatter()
        {
            // https://codeplex.codeplex.com/wikipage?title=CodePlex%20Wiki%20Markup%20Guide
            this.EscapeCharacter = '\\';
            this.UnorderedListItemPrefix = "- ";
            this.OrderedListItemPrefix = "i. ";
            this.HorizontalRulerText = "- - -";
            this.LineBreakText = "//";
            this.StrongWrapper = "*";
            this.EmphasizedWrapper = "_";
            this.HeaderPrefix = "!";
            this.InlineCodePrefix = "{{";
            this.InlineCodeSuffix = "}}";
        }

        protected override void Write(Hyperlink hyperlink, TextWriter context)
        {
            context.Write("[url:");
            context.Write(hyperlink.Url);
            if (hyperlink.Content.Count > 0)
            {
                context.Write("|");
                this.WriteInlines(hyperlink.Content, context);
            }

            context.Write("]");
        }

        protected override void Write(Image image, TextWriter context)
        {
            context.Write("[image:");
            context.Write(image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                context.Write("|");
                context.Write(image.AlternateText);
            }

            context.Write("]");
        }
    }
}