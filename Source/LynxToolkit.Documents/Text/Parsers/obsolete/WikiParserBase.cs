// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiParserBase.cs" company="Lynx Toolkit">
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
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;

    public abstract class WikiParserBase : DocumentParser
    {
        private static readonly char[] newlineChars = new[] { '\r', '\n' };

        protected Regex BlocksExpression;

        protected Regex DirectivesExpression;

        protected Regex InlineExpression;

        protected Regex ListItemExpression;

        protected Regex TableRowExpression;

        private Regex NewlineWhitespaceExpression = new Regex(@"\s*\r?\n\r?\s*", RegexOptions.Compiled);

        public string BaseDirectory { get; set; }

        protected virtual void AddHeader(string text, int level)
        {
            var h = new Header { Level = level };
            this.ParseInlines(text, h.Content);
            this.doc.Blocks.Add(h);
        }

        protected virtual void AddParagraph(string text)
        {
            var pa = new Paragraph();
            this.ParseInlines(text, pa.Content);
            this.doc.Blocks.Add(pa);
        }

        protected void ParseCore(string text)
        {
            // CRLF => LF only
            text = text.Replace("\r\n", "\n");

            int index = 0;
            foreach (Match match in this.BlocksExpression.Matches(text))
            {
                if (match.Index > index)
                {
                    var s = text.Substring(index, match.Index - index);
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        this.AddParagraph(s);
                    }
                }

                index = match.Index + match.Length;

                var h1 = match.Groups["h1"];
                if (h1.Success)
                {
                    this.AddHeader(h1.Value, 1);
                    continue;
                }

                var h2 = match.Groups["h2"];
                if (h2.Success)
                {
                    this.AddHeader(h2.Value, 2);
                    continue;
                }

                var h = match.Groups["h"];
                if (h.Success)
                {
                    this.AddHeader(h.Value, match.Groups["hlevel"].Value.Length);
                    continue;
                }

                var table = match.Groups["table"];
                if (table.Success)
                {
                    this.AddTable(table.Value);
                    continue;
                }

                var quote = match.Groups["quote"];
                if (quote.Success)
                {
                    this.AddQuote(quote.Value);
                    continue;
                }

                var ul = match.Groups["ul"];
                if (ul.Success)
                {
                    this.AddList(ul.Value);
                    continue;
                }

                var ol = match.Groups["ol"];
                if (ol.Success)
                {
                    this.AddList(ol.Value);
                    continue;
                }

                var code = match.Groups["code"];
                if (code.Success)
                {
                    this.AddCodeBlock(code.Value, match.Groups["codelanguage"].Value);
                    continue;
                }

                if (match.Groups["hr"].Success)
                {
                    this.doc.Blocks.Add(new HorizontalRuler());
                    continue;
                }
            }

            if (index < text.Length)
            {
                var s = text.Substring(index);
                if (!string.IsNullOrWhiteSpace(s))
                {
                    this.AddParagraph(s);
                }
            }
        }

        protected virtual Language ParseLanguage(string language)
        {
            if (string.Equals(language, "cs", StringComparison.OrdinalIgnoreCase))
            {
                return Language.Cs;
            }

            if (string.Equals(language, "c#", StringComparison.OrdinalIgnoreCase))
            {
                return Language.Cs;
            }

            if (string.Equals(language, "js", StringComparison.OrdinalIgnoreCase))
            {
                return Language.Js;
            }

            if (string.Equals(language, "xml", StringComparison.OrdinalIgnoreCase))
            {
                return Language.Xml;
            }

            if (string.Equals(language, "xaml", StringComparison.OrdinalIgnoreCase))
            {
                return Language.Xml;
            }

            return Language.Unknown;
        }

        private void AddCodeBlock(string text, string language)
        {
            var codeBlock = new CodeBlock { Language = this.ParseLanguage(language), Text = text.Trim(newlineChars) };
            this.doc.Blocks.Add(codeBlock);
        }

        private void AddList(string value)
        {
            List list = null;
            var matches = this.ListItemExpression.Matches(value);
            foreach (Match match in matches)
            {
                var level = match.Groups[1] != null ? match.Groups[1].Length : 0;
                var unordered = match.Groups["unordered"].Success;

                int end = value.Length;
                var next = match.NextMatch();
                if (next.Success)
                {
                    end = next.Index;
                }

                var start = match.Index + match.Length;
                var item = value.Substring(start, end - start).Trim();
                var li = new ListItem();
                this.ParseInlines(item, li.Content);
                if (list == null)
                {
                    if (unordered)
                    {
                        list = new UnorderedList();
                    }
                    else
                    {
                        list = new OrderedList();
                    }

                    this.doc.Blocks.Add(list);
                }

                list.Items.Add(li);
            }
        }

        private void AddQuote(string text)
        {
            text = text.Replace("> ", string.Empty);
            var quote = new Quote();
            this.ParseInlines(text, quote.Content);
            this.doc.Blocks.Add(quote);
        }

        private void AddTable(string text)
        {
            var table = new Table();
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                var tr = new TableRow();
                foreach (Match match in this.TableRowExpression.Matches(line))
                {
                    TableCell td = match.Groups[1].Value == "|" ? new TableCell() : new TableHeaderCell();
                    var content = match.Groups[2].Value;

                    var right = content.StartsWith("  ");
                    var left = content.EndsWith("  ");
                    if (left && right)
                    {
                        td.HorizontalAlignment = HorizontalAlignment.Center;
                    }

                    if (right && !left)
                    {
                        td.HorizontalAlignment = HorizontalAlignment.Right;
                    }

                    // this.ParseInlines(content.Trim(), td.Content);
                    tr.Cells.Add(td);
                }

                table.Rows.Add(tr);
            }

            this.doc.Blocks.Add(table);
        }

        private string Decode(string input)
        {
            return CharacterReplacements.Decode(input);
        }

        private void ParseInlines(string text, IList<Inline> inlines)
        {
            text = text.Trim(newlineChars);
            var matches = this.InlineExpression.Matches(text);

            int index = 0;
            foreach (Match match in matches)
            {
                if (match.Index > index)
                {
                    var s = text.Substring(index, match.Index - index);
                    this.ParseRun(s, inlines);
                }

                index = match.Index + match.Length;

                var strongGroup = match.Groups["strong"];
                if (strongGroup.Success)
                {
                    var innertext = strongGroup.Value;
                    var strong = new Strong();
                    this.ParseInlines(innertext, strong.Content);
                    inlines.Add(strong);
                }

                var emphasizedGroup = match.Groups["em"];
                if (emphasizedGroup.Success)
                {
                    var innertext = emphasizedGroup.Value;
                    var em = new Emphasized();
                    this.ParseInlines(innertext, em.Content);
                    inlines.Add(em);
                }

                var smileyGroup = match.Groups["smiley"];
                if (smileyGroup.Success)
                {
                    var symbol = new Symbol { Name = smileyGroup.Value };
                    inlines.Add(symbol);
                }

                var symbolGroup = match.Groups["symbol"];
                if (symbolGroup.Success)
                {
                    var symbol = new Symbol { Name = symbolGroup.Value };
                    inlines.Add(symbol);
                }

                var codeGroup = match.Groups["code"];
                if (codeGroup.Success)
                {
                    var code = codeGroup.Value;
                    var a = new InlineCode { Code = code };
                    inlines.Add(a);
                }

                var anchorGroup = match.Groups["anchor"];
                if (anchorGroup.Success)
                {
                    var name = anchorGroup.Value;
                    var a = new Anchor { Name = name };
                    inlines.Add(a);
                }

                var linkGroup = match.Groups["ahref"];
                if (linkGroup.Success)
                {
                    var innertext = match.Groups["atext"].Value;
                    var url = match.Groups["ahref"].Value;
                    var title = match.Groups["atitle"].Value;
                    if (string.IsNullOrWhiteSpace(innertext))
                    {
                        innertext = url.Replace("http://", string.Empty);
                    }

                    var a = new Hyperlink { Url = url, Title = title };
                    this.ParseInlines(innertext, a.Content);
                    inlines.Add(a);
                }

                var imgGroup = match.Groups["imgsrc"];
                if (imgGroup.Success)
                {
                    var src = match.Groups["imgsrc"].Value;
                    var alt = match.Groups["imgalt"].Value;
                    var imglink = match.Groups["imglink"].Value;
                    var img = new Image
                                  {
                                      Source = src,
                                      AlternateText = alt,
                                      Link = imglink
                                  };
                    inlines.Add(img);
                }

                var linebreakGroup = match.Groups["br"];
                if (linebreakGroup.Success)
                {
                    inlines.Add(new LineBreak());
                }

                var newlineGroup = match.Groups["n"];
                if (newlineGroup.Success)
                {
                    inlines.Add(new Run(" "));
                }
            }

            if (index < text.Length)
            {
                var s = text.Substring(index);
                this.ParseRun(s, inlines);
            }
        }

        private void ParseRun(string text, IList<Inline> inlines)
        {
            var s = this.Decode(text);
            inlines.Add(new Run(s));
        }
    }
}