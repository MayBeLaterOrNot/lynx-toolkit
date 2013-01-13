// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SiteMap.cs" company="Lynx">
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

namespace HhcGen
{
    using System.Collections.Generic;
    using System.IO;
    using System.Text.RegularExpressions;

    public class SiteMap
    {
        public SiteMap(string title = null, string link = null)
        {
            this.Title = title;
            this.Link = link;
            this.Children = new List<SiteMap>();
        }

        public List<SiteMap> Children { get; private set; }

        public string Link { get; set; }

        public string Title { get; set; }

        public static SiteMap Parse(string filePath, string extension = ".html")
        {
            var text = File.ReadAllText(filePath);
            var site = new Stack<SiteMap>();
            site.Push(new SiteMap());
            var r = new Regex(
                @"^(?<level>\s*)(?:(\[(?<link>.+?)(?:\|(?<title>.+))?\])|(?<header>.+?))\s*$", RegexOptions.Multiline);
            foreach (Match match in r.Matches(text))
            {
                var level = match.Groups["level"].Value.Length / 2;
                var title = match.Groups["title"].Value;
                var header = match.Groups["header"].Value;
                var link = match.Groups["link"].Value;
                if (string.IsNullOrEmpty(header))
                {
                    if (string.IsNullOrEmpty(title))
                    {
                        title = link;
                    }
                }
                else
                {
                    title = header;
                }

                while (level < site.Count - 1)
                {
                    site.Pop();
                }

                var child = new SiteMap { Title = title, Link = string.IsNullOrEmpty(link) ? null : link + extension };
                var parent = site.Peek();
                parent.Children.Add(child);
                site.Push(child);
            }

            while (site.Count > 1)
            {
                site.Pop();
            }

            return site.Pop();
        }

        public void WriteContentsFile(string filePath)
        {
            using (var s = new StreamWriter(filePath))
            {
                s.WriteLine("<!DOCTYPE HTML PUBLIC \"-//IETF//DTD HTML//EN\">");
                s.WriteLine("<HTML>");
                s.WriteLine("<HEAD>");
                s.WriteLine("<meta name=\"GENERATOR\" content=\"Microsoft&reg; HTML Help Workshop 4.1\">");
                s.WriteLine("<!-- Sitemap 1.0 -->");
                s.WriteLine("</HEAD><BODY>");
                this.WriteChildren(s);
                s.WriteLine("</BODY></HTML>");
            }
        }

        private void WriteChildren(StreamWriter s)
        {
            if (this.Children.Count > 0)
            {
                s.WriteLine("<UL>");
                foreach (var c in this.Children)
                {
                    s.WriteLine("<LI> <OBJECT type=\"text/sitemap\">");
                    s.WriteLine("<param name=\"Name\" value=\"{0}\">", c.Title);
                    if (c.Link != null)
                    {
                        s.WriteLine("<param name=\"Local\" value=\"{0}\">", c.Link);
                    }

                    s.WriteLine("</OBJECT>");
                    c.WriteChildren(s);
                }

                s.WriteLine("</UL>");
            }
        }
    }
}