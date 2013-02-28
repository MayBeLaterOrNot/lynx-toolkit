// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiParser.cs" company="Lynx Toolkit">
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
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Provides a simple wiki parser.  
    /// </summary>
    public class WikiParser
    {
        private Document document;

        private char escapeCharacter = '~';

        private int i;

        private int n;

        private char newLineCharacter = '\n';

        private string text;

        public WikiParser()
        {
            this.Defines = new HashSet<string>();
            this.Variables = new Dictionary<string, string>();
            this.CurrentDirectory = string.Empty;
        }

        public WikiParser(HashSet<string> defines, Dictionary<string, string> variables)
        {
            this.Defines = new HashSet<string>(defines);
            this.Variables = new Dictionary<string, string>(variables);
            this.CurrentDirectory = string.Empty;
        }

        public string Syntax { get; set; }

        public string CurrentDirectory { get; set; }

        public string IncludeDefaultExtension { get; set; }

        public HashSet<string> Defines { get; private set; }

        public Dictionary<string, string> Variables { get; private set; }

        public Document Parse(string text)
        {
            this.text = text.Replace("\r", string.Empty);
            this.n = this.text.Length;
            this.i = 0;
            this.document = new Document();
            this.ParseBlocks(this.document.Blocks, null);
            return this.document;
        }

        public Document ParseFile(string fileName)
        {
            this.CurrentDirectory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            var content = File.ReadAllText(fileName);
            return this.Parse(content);
        }

        private void ParseCodeBlock(BlockCollection blocks)
        {
            string language = this.ReadTo(this.newLineCharacter);
            this.i++;
            var b = new StringBuilder();

            if (this.Match("@include"))
            {
                var include = this.ReadArg();
                var path = Path.GetFullPath(Path.Combine(this.CurrentDirectory, include));
                var content = File.ReadAllText(path);
                b.Append(content);
            }

            while (this.i < this.n)
            {
                if (this.Match("```"))
                {
                    break;
                }

                var c = text[i++];
                if (c == this.newLineCharacter)
                {
                    b.AppendLine();
                }
                else
                {
                    b.Append(c);
                }
            }

            var code = b.ToString().Trim();
            blocks.Add(new CodeBlock(language, code));
        }

        private Header ParseHeader()
        {
            int level = 1;
            this.i++;
            while (this.i < this.n)
            {
                if (this.text[this.i] != '=')
                {
                    break;
                }

                level++;
                this.i++;
            }

            this.Skip(' ');

            var header = new Header { Level = level };
            this.ParseInlines(header.Content, "\n");
            this.i++;
            return header;
        }

        private bool Match(string m)
        {
            int mn = m.Length;
            int j = this.i;
            int k = 0;
            while (j < this.n && k < mn)
            {
                if (this.text[j] != m[k])
                {
                    return false;
                }

                j++;
                k++;
            }

            if (k == mn)
            {
                this.i += mn;
                return true;
            }

            return false;
        }

        private bool MatchSymbol(out string symbolName)
        {
            foreach (var symbol in SymbolResolver.GetSymbolNames())
            {
                if (Match(symbol))
                {
                    symbolName = symbol;
                    return true;
                }
            }

            symbolName = default(string);
            return false;
        }

        private int MatchTerminator(string terminator)
        {
            int tn = terminator.Length;
            int k = 0;
            while (k < tn && this.i + k < this.n)
            {
                if (this.text[this.i + k] != terminator[k])
                {
                    break;
                }

                k++;
            }

            if (k == tn)
            {
                return tn;
            }

            return 0;
        }

        private int MatchTerminator(params string[] terminators)
        {
            foreach (var terminator in terminators)
            {
                var r = this.MatchTerminator(terminator);
                if (r > 0)
                {
                    return r;
                }
            }

            return 0;
        }

        private bool MatchVariable(out string value)
        {
            foreach (var kvp in this.Variables)
            {
                if (this.Match("$" + kvp.Key))
                {
                    value = kvp.Value;
                    return true;
                }
            }

            value = default(string);
            return false;
        }

        private void ParseBlocks(BlockCollection b, string terminator)
        {
            while (this.i < this.n)
            {
                this.SkipWhitespace();
                if (this.i == this.n)
                {
                    break;
                }

                if (terminator != null && this.MatchTerminator(terminator) > 0)
                {
                    break;
                }

                switch (this.text[this.i])
                {
                    case '@':
                        this.ParseDirective(b);
                        continue;
                    case '=':
                        b.Add(this.ParseHeader());
                        continue;

                    case '`':
                        if (this.Match("```"))
                        {
                            this.ParseCodeBlock(b);
                            continue;
                        }

                        break;

                    case '-':
                        if (this.Match("---"))
                        {
                            this.Skip('-');
                            b.Add(new HorizontalRuler());
                            continue;
                        }

                        break;

                    case '*':
                    case '#':
                        b.Add(this.ParseList(terminator));
                        continue;

                    case '[':
                        if (this.text[this.i + 1] == '[')
                        {
                            this.i += 2;
                            b.Add(this.ParseSection());
                            continue;
                        }

                        break;

                    case '>':
                        b.Add(this.ParseQuote(terminator));
                        continue;

                    case '|':
                        b.Add(this.ParseTable());
                        continue;
                }

                b.Add(this.ParseParagraph(terminator));
            }
        }

        private string ResolveIncludeFile(string filename)
        {
            var include = Path.GetFullPath(Path.Combine(this.CurrentDirectory, filename));
            if (this.IncludeDefaultExtension != null && string.IsNullOrEmpty(Path.GetExtension(include)))
            {
                include = Path.ChangeExtension(include, this.IncludeDefaultExtension);
            }

            return include;
        }

        private void ParseDirective(BlockCollection b)
        {
            this.i++;
            if (this.Match("include"))
            {
                var s = this.ReadArg();

                var content = File.ReadAllText(this.ResolveIncludeFile(s));
                this.text = this.text.Insert(this.i, content);
                this.n = this.text.Length;
                return;
            }

            if (this.Match("import"))
            {
                var s = this.ReadArg();
                var parser = new WikiParser(this.Defines, this.Variables);
                parser.IncludeDefaultExtension = this.IncludeDefaultExtension;
                var importedDocument = parser.ParseFile(this.ResolveIncludeFile(s));
                foreach (var block in importedDocument.Blocks)
                {
                    b.Add(block);
                }

                return;
            }

            if (this.Match("toc"))
            {
                var s = this.ReadArg();
                b.Add(new TableOfContents { Levels = int.Parse(s) });
                return;
            }

            if (this.Match("title"))
            {
                this.document.Title = this.ReadArg();
                return;
            }

            if (this.Match("syntax"))
            {
                this.Syntax = this.ReadArg();
                return;
            }

            if (this.Match("keywords"))
            {
                this.document.Keywords = this.ReadArg();
                return;
            }

            if (this.Match("description"))
            {
                this.document.Description = this.ReadArg();
                return;
            }

            if (this.Match("category"))
            {
                this.document.Category = this.ReadArg();
                return;
            }

            if (this.Match("creator"))
            {
                this.document.Creator = this.ReadArg();
                return;
            }

            if (this.Match("revision"))
            {
                this.document.Revision = this.ReadArg();
                return;
            }

            if (this.Match("version"))
            {
                this.document.Version = this.ReadArg();
                return;
            }

            if (this.Match("date"))
            {
                this.document.Date = this.ReadArg();
                return;
            }

            if (this.Match("subject"))
            {
                this.document.Subject = this.ReadArg();
                return;
            }

            if (this.Match("if"))
            {
                var condition = this.ReadArg();
                if (!this.Defines.Contains(condition))
                {
                    this.ReadTo("@endif");
                    this.i += 6;
                }

                return;
            }

            if (this.Match("endif"))
            {
                return;
            }

            throw new Exception("Unknown directive.");
        }

        private int ParseInlines(InlineCollection c, params string[] terminators)
        {
            var runContent = new StringBuilder();
            Action addRun = () =>
                {
                    if (runContent.Length > 0)
                    {
                        c.Add(new Run(runContent.ToString()));
                        runContent.Clear();
                    }
                };

            int nt = terminators.Length;
            int t = 0;

            while (this.i < this.n)
            {
                if (this.text[this.i] == this.escapeCharacter)
                {
                    this.i++;
                    var ch = this.text[i++];
                    runContent.Append(ch);
                    continue;
                }

                switch (nt)
                {
                    case 1:
                        t = this.MatchTerminator(terminators[0]);
                        break;
                    case 0:
                        break;
                    default:
                        t = this.MatchTerminator(terminators);
                        break;
                }

                if (t > 0)
                {
                    break;
                }

                switch (this.text[this.i])
                {
                    case '$':
                        if (this.i + 1 < this.n && this.text[this.i + 1] == '$')
                        {
                            this.i += 2;
                            addRun();
                            var eq = new Equation { Content = this.ReadTo("$$", false) };
                            this.i += 2;
                            c.Add(eq);
                            continue;
                        }

                        string variableValue;
                        if (this.MatchVariable(out variableValue))
                        {
                            runContent.Append(variableValue);
                            continue;
                        }

                        break;

                    case '\n':
                        this.i++;
                        addRun();
                        continue;

                    case '\\':
                        if (this.i + 1 < this.n && this.text[this.i + 1] == '\\')
                        {
                            this.i += 2;
                            addRun();
                            c.Add(new LineBreak());
                            continue;
                        }

                        break;

                    case '*':
                        if (this.i + 1 < this.n && this.text[this.i + 1] == '*')
                        {
                            addRun();
                            this.i += 2;
                            var strong = new Strong();
                            this.ParseInlines(strong.Content, "**");
                            this.i += 2;
                            c.Add(strong);
                            continue;
                        }

                        this.i++;
                        addRun();

                        var em = new Emphasized();
                        this.ParseInlines(em.Content, "*");
                        this.i++;
                        c.Add(em);
                        continue;

                    case '`':
                        addRun();
                        this.i++;
                        var code = new InlineCode { Code = this.ReadTo("`", false) };
                        this.i++;
                        c.Add(code);
                        continue;

                    case '[':
                        addRun();
                        this.i++;
                        var link = new Hyperlink { Url = this.ReadToAny('|', ']') };
                        if (this.text[this.i] == '|')
                        {
                            this.i++;
                            this.ParseInlines(link.Content, "]");
                        }
                        else
                        {
                            link.Content.Add(new Run(SimplifyUrl(link.Url)));
                        }

                        this.i++;

                        c.Add(link);
                        continue;

                    case '{':
                        this.i++;
                        addRun();

                        // Span
                        if (this.text[this.i] == '{')
                        {
                            this.i++;
                            var className = this.ReadTo(':');
                            this.i++;
                            var span = new Span { Class = className };
                            this.ParseInlines(span.Content, "}}");
                            this.i += 2;
                            c.Add(span);
                            continue;
                        }

                        // Anchor
                        if (this.text[this.i] == '#')
                        {
                            this.i++;
                            var anchorName = this.ReadTo('}');
                            this.i++;
                            var anchor = new Anchor { Name = anchorName };
                            c.Add(anchor);
                            continue;
                        }

                        // Image
                        var source = this.ReadToAny('|', '}');
                        if (!source.StartsWith("http") && !Path.IsPathRooted(source))
                        {
                            source = Path.GetFullPath(Path.Combine(this.CurrentDirectory, source));
                        }

                        var img = new Image { Source = source };

                        if (this.text[this.i] == '|')
                        {
                            this.i++;
                            img.AlternateText = this.ReadToAny('|', '}');
                        }

                        if (this.text[this.i] == '|')
                        {
                            this.i++;
                            img.Link = this.ReadTo('}');
                        }

                        this.i++;
                        c.Add(img);
                        continue;

                    case '(':
                    case ':':
                    case ';':
                        string symbolName;
                        if (this.MatchSymbol(out symbolName))
                        {
                            var s = new Symbol { Name = symbolName };
                            c.Add(s);
                            continue;
                        }

                        break;
                }

                runContent.Append(this.text[this.i++]);
            }

            addRun();
            return t;
        }

        private string SimplifyUrl(string url)
        {
            if (url == null)
            {
                return null;
            }

            return url.Replace("http://", string.Empty).Replace("https://", string.Empty);
        }

        private List ParseList(string terminator)
        {
            List list = null;
            while (this.i < this.n)
            {
                var prefix = this.ReadTo(' ');
                this.i++;
                if (list == null)
                {
                    if (prefix[0] == '#')
                    {
                        list = new OrderedList();
                    }
                    else
                    {
                        list = new UnorderedList();
                    }
                }

                var li = new ListItem();
                if (terminator == null)
                {
                    this.ParseInlines(li.Content, "\n");
                }
                else
                {
                    this.ParseInlines(li.Content, "\n", terminator);
                }

                this.i++;
                list.Items.Add(li);
                if (this.i < this.n && this.text[this.i] != '*' && this.text[this.i] != '#')
                {
                    break;
                }
            }

            return list;
        }

        private Block ParseParagraph(string terminator)
        {
            var p = new Paragraph();
            if (terminator == null)
            {
                this.ParseInlines(p.Content, "\n\n");
            }
            else
            {
                this.ParseInlines(p.Content, "\n\n", terminator);
            }

            return p;
        }

        private Block ParseQuote(string terminator)
        {
            var quote = new Quote();
            while (this.i < this.n)
            {
                this.i++;
                this.Skip(' ');
                if (terminator == null)
                {
                    this.ParseInlines(quote.Content, "\n");
                }
                else
                {
                    this.ParseInlines(quote.Content, "\n", terminator);
                }

                this.i++;
                if (this.i < this.n && this.text[this.i] != '>')
                {
                    break;
                }
            }

            return quote;
        }

        private Section ParseSection()
        {
            var className = this.ReadTo(':');
            this.i++;
            var section = new Section { Class = className };
            this.ParseBlocks(section.Blocks, "]]");
            this.i += 2;
            return section;
        }

        private Table ParseTable()
        {
            var table = new Table();
            while (this.i < this.n)
            {
                this.i++;
                var row = new TableRow();
                table.Rows.Add(row);
                while (this.i < this.n)
                {
                    bool isHeader = this.text[this.i] == '|';
                    var cell = isHeader ? new TableHeaderCell() : new TableCell();
                    if (isHeader)
                    {
                        i++;
                    }

                    switch (this.text[this.i])
                    {
                        case '^':
                            row.Cells.Last().ColumnSpan++;
                            this.ReadTo('|');
                            this.i++;
                            break;
                        case '¨':
                            var previousRow = table.Rows[table.Rows.Count - 2];
                            previousRow.Cells[row.Cells.Count].RowSpan++;
                            this.ReadTo('|');
                            this.i++;
                            break;
                        default:
                            row.Cells.Add(cell);
                            this.ParseBlocks(cell.Blocks, "|");
                            this.i++;
                            break;
                    }

                    if (this.i < this.n && this.text[this.i] == '\n')
                    {
                        this.i++;
                        break;
                    }
                }

                if (this.i >= this.n || this.text[this.i] != '|')
                {
                    break;
                }
            }

            return table;
        }

        private string ReadArg()
        {
            this.Skip(' ');
            return this.ReadTo('\n');
        }

        private string ReadTo(char endchar)
        {
            var b = new StringBuilder();
            while (this.i < this.n)
            {
                var c = this.text[this.i];

                if (c == '\\')
                {
                    this.i++;
                    b.Append(this.text[this.i++]);
                    continue;
                }

                if (c == endchar)
                {
                    break;
                }

                if (c == '$')
                {
                    string variableContent;
                    if (this.MatchVariable(out variableContent))
                    {
                        b.Append(variableContent);
                        continue;
                    }
                }

                b.Append(c);
                this.i++;
            }

            return b.ToString();
        }

        private string ReadTo(string terminator, bool handleEscapeCharacters = true)
        {
            var b = new StringBuilder();
            while (this.i < this.n)
            {
                if (this.MatchTerminator(terminator) > 0)
                {
                    break;
                }

                var c = this.text[this.i];
                if (handleEscapeCharacters && c == this.escapeCharacter)
                {
                    this.i++;
                    b.Append(this.text[this.i++]);
                    continue;
                }

                if (c == '$')
                {
                    string variableContent;
                    if (this.MatchVariable(out variableContent))
                    {
                        b.Append(variableContent);
                        continue;
                    }
                }

                b.Append(c);
                this.i++;
            }

            return b.ToString();
        }

        private string ReadToAny(char endchar1, char endchar2)
        {
            var b = new StringBuilder();
            while (this.i < this.n)
            {
                var c = this.text[this.i];

                if (c == this.escapeCharacter)
                {
                    this.i++;
                    b.Append(this.text[this.i++]);
                    continue;
                }

                if (c == endchar1 || c == endchar2)
                {
                    break;
                }

                if (c == '$')
                {
                    string variableContent;
                    if (this.MatchVariable(out variableContent))
                    {
                        b.Append(variableContent);
                        continue;
                    }
                }

                b.Append(c);
                this.i++;
            }

            return b.ToString();
        }

        private void Skip(char x)
        {
            while (this.i < this.n)
            {
                if (this.text[this.i] != x)
                {
                    break;
                }

                this.i++;
            }
        }

        private void SkipWhitespace()
        {
            while (this.i < this.n)
            {
                var ch = this.text[this.i];
                if (ch != ' ' && ch != this.newLineCharacter)
                {
                    break;
                }

                this.i++;
            }
        }
    }
}