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
    /// Implements a simple wiki parser.  
    /// </summary>
    public class WikiParser
    {
        private Func<string, Stream> OpenRead { get; set; }

        private Document document;

        private const char EscapeCharacter = '~';

        private const char DirectivePrefix = '@';

        private const char HeaderPrefix = '=';

        private const char OrderedListPrefix = '#';

        private const char UnorderedListPrefix = '-';

        private const char NewLineCharacter = '\n';

        private const string IncludeKeyword = "@include";

        private const string CodeBlockPrefix = "```";

        private const string HorizontalRulerSymbol = "---";

        private const string VariablePrefix = "$";

        private const string SectionPrefix = "[[";

        private const char QuotePrefix = '>';

        private const char TablePrefix = '|';

        private const string UrlPrefix = "http://";

        private const string SecureUrlPrefix = "https://";

        private const string NewLine = "\n";

        private const string ParagraphSuffix = "\n\n";

        private int i;

        private int n;

        private string text;

        public WikiParser(Func<string, Stream> openRead = null)
            : this(new string[] { }, new Dictionary<string, string>(), openRead)
        {
        }

        public WikiParser(
            IEnumerable<string> defines,
            Dictionary<string, string> variables,
            Func<string, Stream> openRead)
        {
            this.OpenRead = openRead;
            this.ImportPaths = new List<string>();
            this.IncludePaths = new List<string>();
            this.Defines = new List<string>(defines);
            this.Variables = new Dictionary<string, string>(variables);
            this.CurrentDirectory = string.Empty;
        }

        public string Syntax { get; set; }

        public string CurrentDirectory { get; set; }

        public string IncludeDefaultExtension { get; set; }

        public List<string> ImportPaths { get; private set; }

        public List<string> IncludePaths { get; private set; }

        public List<string> Defines { get; private set; }

        public Dictionary<string, string> Variables { get; private set; }

        /// <summary>
        /// Parses the specified text.
        /// </summary>
        /// <param name="input">The text to parse.</param>
        /// <returns>the parsed <see cref="Document"/>.</returns>
        public Document Parse(string input)
        {
            this.text = input.Replace("\r", string.Empty);
            this.n = this.text.Length;
            this.i = 0;
            this.document = new Document();
            this.ParseBlocks(this.document.Blocks, null);
            return this.document;
        }

        private string ReadAllText(string fileName)
        {
            using (var r = new StreamReader(this.OpenRead(fileName)))
            {
                return r.ReadToEnd();
            }
        }

        public Document ParseFile(string fileName)
        {
            this.CurrentDirectory = PathUtilities.GetDirectoryName(fileName);
            var content = this.ReadAllText(fileName);
            return this.Parse(content);
        }

        private void ParseCodeBlock(BlockCollection blocks)
        {
            string language = this.ReadTo(NewLineCharacter);
            this.i++;
            var b = new StringBuilder();

            if (this.Match(IncludeKeyword))
            {
                var include = this.ReadArg();
                var path = PathUtilities.Combine(this.CurrentDirectory, include);
                var content = this.ReadAllText(path);
                b.Append(content);
            }

            while (this.i < this.n)
            {
                if (this.Match(CodeBlockPrefix))
                {
                    break;
                }

                var c = this.text[i++];
                if (c == NewLineCharacter)
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
                if (this.Match(VariablePrefix + kvp.Key))
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

                if (this.text[this.i] == DirectivePrefix)
                {
                    this.ParseDirective(b);
                    continue;
                }

                if (this.text[this.i] == HeaderPrefix)
                {
                    b.Add(this.ParseHeader());
                    continue;
                }

                if (this.text[this.i] == CodeBlockPrefix[0])
                {
                    if (this.Match(CodeBlockPrefix))
                    {
                        this.ParseCodeBlock(b);
                        continue;
                    }
                }

                if (this.text[this.i] == HorizontalRulerSymbol[0])
                {
                    if (this.Match(HorizontalRulerSymbol))
                    {
                        this.Skip('-');
                        b.Add(new HorizontalRuler());
                        continue;
                    }
                }

                if (this.text[this.i] == OrderedListPrefix || this.text[this.i] == UnorderedListPrefix)
                {
                    b.Add(this.ParseList(terminator));
                    continue;
                }

                if (this.text[this.i] == SectionPrefix[0])
                {
                    if (this.text[this.i + 1] == SectionPrefix[1])
                    {
                        this.i += 2;
                        b.Add(this.ParseSection());
                        continue;
                    }
                }

                if (this.text[this.i] == QuotePrefix)
                {
                    b.Add(this.ParseQuote(terminator));
                    continue;
                }

                if (this.text[this.i] == TablePrefix)
                {
                    b.Add(this.ParseTable());
                    continue;
                }

                b.Add(this.ParseParagraph(terminator));
            }
        }

        private bool Exists(string filename)
        {
            try
            {
                using (this.OpenRead(filename)) { }
                return true;
            }
            catch
            {
                return false;
            }
        }

        private string ResolveIncludeFile(string filename, IEnumerable<string> paths)
        {
            if (this.IncludeDefaultExtension != null && string.IsNullOrEmpty(PathUtilities.GetExtension(filename)))
            {
                filename = PathUtilities.ChangeExtension(filename, this.IncludeDefaultExtension);
            }

            var resolvedPath = PathUtilities.Combine(this.CurrentDirectory, filename);
            if (this.Exists(resolvedPath))
            {
                return resolvedPath;
            }

            foreach (var path in paths)
            {

                resolvedPath = PathUtilities.Simplify(PathUtilities.Combine(PathUtilities.Combine(this.CurrentDirectory, path), filename));
                if (this.Exists(resolvedPath))
                {
                    return resolvedPath;
                }
            }

            throw new FileNotFoundException(filename + " not found.");
        }

        private void ParseDirective(BlockCollection b)
        {
            this.i++;

            if (this.Match("importpath"))
            {
                this.ImportPaths.Add(this.ReadArg());
                return;
            }

            if (this.Match("includepath"))
            {
                this.IncludePaths.Add(this.ReadArg());
                return;
            }

            if (this.Match("include"))
            {
                var s = this.ReadArg();

                var content = this.ReadAllText(this.ResolveIncludeFile(s, this.IncludePaths));
                this.text = this.text.Insert(this.i, content);
                this.n = this.text.Length;
                return;
            }

            if (this.Match("import"))
            {
                var s = this.ReadArg();
                var parser = new WikiParser(this.Defines, this.Variables, this.OpenRead) { IncludeDefaultExtension = this.IncludeDefaultExtension };
                var importedDocument = parser.ParseFile(this.ResolveIncludeFile(s, this.ImportPaths));
                b.AddRange(importedDocument.Blocks);

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

            if (this.Match("subtitle"))
            {
                this.document.Subtitle = this.ReadArg();
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
                // Note: boolean expressions are not supported
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

            var directive = this.ReadTo(' ');
            throw new FormatException("Unknown directive: " + directive);
        }

        private int ParseInlines(InlineCollection c, params string[] terminators)
        {
            var runContent = new StringBuilder();
            Action addRun = () =>
                {
                    if (runContent.Length > 0)
                    {
                        c.Add(new Run(runContent.ToString()));
                        runContent = new StringBuilder();
                    }
                };

            int nt = terminators.Length;
            int t = 0;

            while (this.i < this.n)
            {
                if (this.text[this.i] == EscapeCharacter)
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

                var ci = this.text[this.i];
                var ci2 = this.i + 1 < this.n ? this.text[this.i + 1] : '\0';

                if (ci == '$')
                {
                    if (ci2 == '$')
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
                }

                if (ci == NewLineCharacter)
                {
                    this.i++;
                    addRun();
                    continue;
                }

                if (ci == ' ' && this.Match("  \n"))
                {
                    addRun();
                    c.Add(new LineBreak());
                    continue;
                }

                if (ci == '\\' && ci2 == '\\')
                {
                    this.i += 2;
                    addRun();
                    c.Add(new LineBreak());
                    continue;
                }

                if (ci == '*' && ci2 == '*')
                {
                    addRun();
                    this.i += 2;
                    var strong = new Strong();
                    this.ParseInlines(strong.Content, "**");
                    this.i += 2;
                    c.Add(strong);
                    continue;
                }

                if (ci == '/' && ci2 == '/')
                {
                    addRun();
                    this.i += 2;

                    var em = new Emphasized();
                    this.ParseInlines(em.Content, "//");
                    this.i += 2;
                    c.Add(em);
                    continue;
                }

                if (ci == '`')
                {
                    addRun();
                    this.i++;
                    var code = new InlineCode { Code = this.ReadTo("`", false) };
                    this.i++;
                    c.Add(code);
                    continue;
                }

                if (ci == '[')
                {
                    addRun();
                    this.i++;
                    var link = new Hyperlink { Url = this.ReadToAny('|', ']') };
                    if (i < n && this.text[this.i] == '|')
                    {
                        this.i++;
                        this.ParseInlines(link.Content, "]");
                    }
                    else
                    {
                        link.Content.Add(new Run(this.SimplifyUrl(link.Url)));
                    }

                    this.i++;

                    c.Add(link);
                    continue;
                }

                if (ci == '{')
                {
                    addRun();
                    this.i++;

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
                    var fullPath = source;
                    if (!source.StartsWith("http") && !PathUtilities.IsPathRooted(source))
                    {
                        fullPath = PathUtilities.Simplify(PathUtilities.Combine(this.CurrentDirectory, source));
                    }

                    var img = new Image { Source = fullPath, OriginalSource = source };

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
                }

                if (ci == '(' || ci == ':' || ci == ';')
                {
                    string symbolName;
                    if (this.MatchSymbol(out symbolName))
                    {
                        var s = new Symbol { Name = symbolName };
                        c.Add(s);
                        continue;
                    }
                }

                runContent.Append(ci);
                this.i++;
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

            return url.Replace(UrlPrefix, string.Empty).Replace(SecureUrlPrefix, string.Empty);
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
                    if (prefix[0] == OrderedListPrefix)
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
                    this.ParseInlines(li.Content, NewLine);
                }
                else
                {
                    this.ParseInlines(li.Content, NewLine, terminator);
                }

                this.i++;
                list.Items.Add(li);
                if (this.i < this.n && this.text[this.i] != UnorderedListPrefix && this.text[this.i] != OrderedListPrefix)
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
                this.ParseInlines(p.Content, ParagraphSuffix);
            }
            else
            {
                this.ParseInlines(p.Content, ParagraphSuffix, terminator);
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
                    this.ParseInlines(quote.Content, NewLine);
                }
                else
                {
                    this.ParseInlines(quote.Content, NewLine, terminator);
                }

                this.i++;
                if (this.i < this.n && this.text[this.i] != QuotePrefix)
                {
                    break;
                }
            }

            return quote;
        }

        private const char SectionDelimiter = ':';
        private const string SectionSuffix = "]]";

        private Section ParseSection()
        {
            var className = this.ReadTo(SectionDelimiter);
            this.i++;
            var section = new Section { Class = className };
            this.ParseBlocks(section.Blocks, SectionSuffix);
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

                if (c == EscapeCharacter)
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
                if (handleEscapeCharacters && c == EscapeCharacter)
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

                if (c == EscapeCharacter)
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
                if (ch != ' ' && ch != NewLineCharacter)
                {
                    break;
                }

                this.i++;
            }
        }
    }
}