using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LynxToolkit.Documents
{
    public abstract class WikiParserBase : DocumentParser
    {
        protected Regex TableRowExpression;
        protected Regex ListItemExpression;
        protected Regex DirectivesExpression;
        protected Regex BlocksExpression;
        protected Regex InlineExpression;

        protected void ParseCore(string text)
        {
            // CRLF => LF only
            text = text.Replace("\r\n", "\n");

            int index = 0;
            foreach (Match match in BlocksExpression.Matches(text))
            {
                if (match.Index > index)
                {
                    var s = text.Substring(index, match.Index - index);
                    if (!string.IsNullOrWhiteSpace(s))
                        AddParagraph(s);
                }
                index = match.Index + match.Length;

                var h1 = match.Groups["h1"];
                if (h1.Success)
                {
                    AddHeader(h1.Value, 1);
                    continue;
                }
                var h2 = match.Groups["h2"];
                if (h2.Success)
                {
                    AddHeader(h2.Value, 2);
                    continue;
                }
                var h = match.Groups["h"];
                if (h.Success)
                {
                    AddHeader(h.Value, match.Groups["hlevel"].Value.Length);
                    continue;
                }
                var table = match.Groups["table"];
                if (table.Success)
                {
                    AddTable(table.Value);
                    continue;
                }
                var quote = match.Groups["quote"];
                if (quote.Success)
                {
                    AddQuote(quote.Value);
                    continue;
                }
                var ul = match.Groups["ul"];
                if (ul.Success)
                {
                    AddList(ul.Value);
                    continue;
                }
                var ol = match.Groups["ol"];
                if (ol.Success)
                {
                    AddList(ol.Value);
                    continue;
                }
                var code = match.Groups["code"];
                if (code.Success)
                {
                    AddCodeBlock(code.Value, match.Groups["codelanguage"].Value);
                    continue;
                }
                if (match.Groups["hr"].Success)
                {
                    doc.Blocks.Add(new HorizontalRuler());
                    continue;
                }
            }

            if (index < text.Length)
            {
                var s = text.Substring(index);
                if (!string.IsNullOrWhiteSpace(s))
                    AddParagraph(s);
            }
        }

        protected virtual Language ParseLanguage(string language)
        {
            if (string.Equals(language, "cs", StringComparison.OrdinalIgnoreCase)) return Language.Cs;
            if (string.Equals(language, "c#", StringComparison.OrdinalIgnoreCase)) return Language.Cs;
            if (string.Equals(language, "js", StringComparison.OrdinalIgnoreCase)) return Language.Js;
            if (string.Equals(language, "xml", StringComparison.OrdinalIgnoreCase)) return Language.Xml;
            if (string.Equals(language, "xaml", StringComparison.OrdinalIgnoreCase)) return Language.Xml;
            return Language.Unknown;
        }

        private void AddCodeBlock(string text, string language)
        {
            var codeBlock = new CodeBlock() { Language = ParseLanguage(language), Text = text.Trim(newlineChars) };
            doc.Blocks.Add(codeBlock);
        }

        private Regex NewlineWhitespaceExpression = new Regex(@"\s*\r?\n\r?\s*", RegexOptions.Compiled);

        private void AddList(string value)
        {
            List list = null;
            var matches = ListItemExpression.Matches(value);
            foreach (Match match in matches)
            {
                var level = match.Groups[1] != null ? match.Groups[1].Length : 0;
                var unordered = match.Groups["unordered"].Success;

                int end = value.Length;
                var next = match.NextMatch();
                if (next.Success)
                    end = next.Index;
                var start = match.Index + match.Length;
                var item = value.Substring(start, end - start).Trim();
                var li = new ListItem();
                ParseInlines(item, li.Content);
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

                    doc.Blocks.Add(list);
                }
                list.Items.Add(li);
            }
        }

        private void AddQuote(string text)
        {
            text = text.Replace("> ", "");
            var quote = new Quote();
            ParseInlines(text, quote.Content);
            doc.Blocks.Add(quote);
        }
        private void AddTable(string text)
        {
            var table = new Table();
            var lines = text.Split('\n');
            foreach (var line in lines)
            {
                var tr = new TableRow();
                foreach (Match match in TableRowExpression.Matches(line))
                {
                    TableCell td = match.Groups[1].Value == "|" ? new TableCell() : new TableHeaderCell();
                    var content = match.Groups[2].Value;

                    var right = content.StartsWith("  ");
                    var left = content.EndsWith("  ");
                    if (left && right)
                        td.HorizontalAlignment = HorizontalAlignment.Center;
                    if (right && !left)
                        td.HorizontalAlignment = HorizontalAlignment.Right;

                    ParseInlines(content.Trim(), td.Content);
                    tr.Cells.Add(td);
                }
                table.Rows.Add(tr);
            }
            doc.Blocks.Add(table);
        }

        protected virtual void AddHeader(string text, int level)
        {
            var h = new Header { Level = level };
            ParseInlines(text, h.Content);
            doc.Blocks.Add(h);
        }

        protected virtual void AddParagraph(string text)
        {
            var pa = new Paragraph();
            ParseInlines(text, pa.Content);
            doc.Blocks.Add(pa);
        }

        private static char[] newlineChars = new char[] { '\r', '\n' };

        private void ParseInlines(string text, IList<Inline> inlines)
        {
            text = text.Trim(newlineChars);
            var matches = InlineExpression.Matches(text);

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
                    ParseInlines(innertext, strong.Content);
                    inlines.Add(strong);
                }

                var emGroup = match.Groups["em"];
                if (emGroup.Success)
                {
                    var innertext = emGroup.Value;
                    var em = new Emphasized();
                    ParseInlines(innertext, em.Content);
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
                    var a = new InlineCode { Text = code };
                    inlines.Add(a);
                }

                var anchorGroup = match.Groups["anchor"];
                if (anchorGroup.Success)
                {
                    var name = anchorGroup.Value;
                    var a = new Anchor { Name = name };
                    inlines.Add(a);
                }

                var aGroup = match.Groups["ahref"];
                if (aGroup.Success)
                {
                    var innertext = match.Groups["atext"].Value;
                    var url = match.Groups["ahref"].Value;
                    var title = match.Groups["atitle"].Value;
                    if (string.IsNullOrWhiteSpace(innertext))
                        innertext = url.Replace("http://", "");

                    var a = new Hyperlink { Url = url, Title = title };
                    ParseInlines(innertext, a.Content);
                    inlines.Add(a);
                }

                var imgGroup = match.Groups["imgsrc"];
                if (imgGroup.Success)
                {
                    var src = match.Groups["imgsrc"].Value;
                    var alt = match.Groups["imgalt"].Value;
                    var imglink = match.Groups["imglink"].Value;
                    var img = new Image { Source = src, AlternateText = alt, Link = imglink };
                    inlines.Add(img);
                }

                var brGroup = match.Groups["br"];
                if (brGroup.Success)
                {
                    inlines.Add(new LineBreak());
                }

                var nGroup = match.Groups["n"];
                if (nGroup.Success)
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
            var s = this.Decode(text.Trim());
            inlines.Add(new Run(s));
        }

        private string Decode(string input)
        {
            return CharacterReplacements.Decode(input);
        }
    }
}