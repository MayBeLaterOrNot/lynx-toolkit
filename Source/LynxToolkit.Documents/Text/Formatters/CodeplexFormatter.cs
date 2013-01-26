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
    public class CodeplexFormatter : WikiFormatterBase
    {
        public static string Format(Document doc)
        {
            var wf = new CodeplexFormatter(doc);
            wf.Format();
            return wf.ToString();
        }

        protected CodeplexFormatter(Document doc)
            : base(doc)
        {

        }

        protected override void Write(Header header)
        {
            Write(Repeat("!", header.Level), " ");
            WriteInlines(header.Content);
            WriteLine();
            WriteLine();
        }

        protected override void Write(Strong strong, object parent)
        {
            Write("*");
            WriteInlines(strong.Content);
            Write("*");
        }

        protected override void Write(Emphasized em, object parent)
        {
            Write("_");
            WriteInlines(em.Content);
            Write("_");
        }

        protected override void Write(LineBreak linebreak, object parent)
        {
            WriteLine(@"//");
        }

        protected override void Write(InlineCode inlineCode, object parent)
        {
            Write("{{", inlineCode.Code, "}}");
        }

        protected override void Write(Hyperlink hyperlink, object parent)
        {
            Write("[url:");
            Write(hyperlink.Url);
            if (hyperlink.Content.Count > 0)
            {
                Write("|");
                WriteInlines(hyperlink.Content);
            }
            Write("]");
        }

        protected override void Write(Image image, object parent)
        {
            Write("[image:", image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("|", image.AlternateText);
            }
            Write("]");
        }

        protected override void Write(Anchor anchor, object parent)
        {
        }
    }
}