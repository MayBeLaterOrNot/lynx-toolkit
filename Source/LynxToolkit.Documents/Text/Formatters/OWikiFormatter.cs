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
    using System.IO;

    public class OWikiFormatter : WikiFormatterBase
    {
        protected override void Write(Hyperlink hyperlink, TextWriter context)
        {
            context.Write("[");
            context.Write(hyperlink.Url);
            bool isDefaultTitle = false;
            if (hyperlink.Content.Count == 1)
            {
                var defaultTitle = hyperlink.Url != null ? hyperlink.Url.Replace("http://", "") : null;
                var run1 = hyperlink.Content[0] as Run;
                isDefaultTitle = run1 != null && string.Equals(run1.Text, defaultTitle);
            }

            if (hyperlink.Content.Count > 0 && !isDefaultTitle)
            {
                context.Write("|");
                this.WriteInlines(hyperlink.Content, context);
            }

            if (!string.IsNullOrEmpty(hyperlink.Title))
            {
                context.Write("|");
                context.Write(hyperlink.Title);
            }

            context.Write("]");
        }

        protected override void Write(Image image, TextWriter context)
        {
            context.Write("{" + image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                context.Write("|" + image.AlternateText);
            }

            if (!string.IsNullOrEmpty(image.Link))
            {
                context.Write("|" + image.Link);
            }

            context.Write("}");
        }
    }
}