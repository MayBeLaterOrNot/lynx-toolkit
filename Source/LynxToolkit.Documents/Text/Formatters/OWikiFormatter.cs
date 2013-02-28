// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OWikiFormatter.cs" company="Lynx Toolkit">
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
    public class OWikiFormatter : MarkdownFormatter
    {
        protected OWikiFormatter(Document doc)
            : base(doc)
        {
        }

        public static new string Format(Document doc)
        {
            var wf = new OWikiFormatter(doc);
            wf.Format();
            return wf.ToString();
        }

        protected override void Write(HorizontalRuler ruler, object parent)
        {
            WriteLine("- - -");
            WriteLine();
        }

        protected override void Write(Hyperlink hyperlink, object parent)
        {
            Write("[");
            Write(hyperlink.Url);
            bool isDefaultTitle = false;
            if (hyperlink.Content.Count == 1)
            {
                var defaultTitle = hyperlink.Url != null ? hyperlink.Url.Replace("http://", "") : null;
                var run1 = hyperlink.Content[0] as Run;
                isDefaultTitle = run1 != null && string.Equals(run1.Text, defaultTitle);
            }

            if (hyperlink.Content.Count > 0 && !isDefaultTitle)
            {
                Write("|");
                WriteInlines(hyperlink.Content, parent);
            }

            if (!string.IsNullOrWhiteSpace(hyperlink.Title))
            {
                Write("|");
                Write(hyperlink.Title);
            }
            Write("]");
        }

        protected override void Write(Image image, object parent)
        {
            Write("{", image.Source);
            if (!string.IsNullOrWhiteSpace(image.AlternateText))
            {
                Write("|", image.AlternateText);
            }
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                Write("|", image.Link);
            }
            Write("}");
        }
    }
}