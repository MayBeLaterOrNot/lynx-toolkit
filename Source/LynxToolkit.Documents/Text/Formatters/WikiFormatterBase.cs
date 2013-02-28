// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WikiFormatterBase.cs" company="Lynx Toolkit">
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
using System.Text;

namespace LynxToolkit.Documents
{
    public abstract class WikiFormatterBase : DocumentFormatter
    {
        protected StringBuilder sb;

        protected WikiFormatterBase(Document doc)
            : base(doc)
        {
            this.sb = new StringBuilder();
            this.LineLength = 72;
        }

        private static char[] newlineChars = new[] { '\r', '\n' };

        public override string ToString()
        {
            return this.sb.ToString().Trim(newlineChars);
        }

        protected void WriteItems(ListItemCollection items, object parent, string prefix)
        {
            int i = 1;
            foreach (var item in items)
            {
                this.sb.AppendFormat(prefix, i++);
                var tmp = this.sb;
                this.sb = new StringBuilder();
                this.WriteInlines(item.Content, parent);
                var text = this.sb.ToString();
                this.sb = tmp;
                text = this.Wrap(text, this.LineLength);
                this.WriteLines(text, Repeat(" ", prefix.Length), string.Empty);
            }
        }

        protected string Wrap(string text, int maxlength)
        {
            var b = new StringBuilder();
            int i0 = 0;
            int i1 = maxlength;
            while (i1 < text.Length)
            {
                int i = i1;
                for (i = i1; i > i0; i--)
                {
                    if (string.IsNullOrWhiteSpace(text[i].ToString()))
                    {
                        break;
                    }
                }

                if (i > i0)
                {
                    var line = text.Substring(i0, i - i0);
                    b.AppendLine(line);
                    i0 = i + 1;
                    i1 = i0 + maxlength;
                }
                else
                {
                    i1 += maxlength;
                }
            }

            var line2 = text.Substring(i0);
            b.Append(line2);
            return b.ToString();
        }

        protected string Encode(string text)
        {
            return CharacterReplacements.Encode(text);
        }

        protected static string Repeat(string input, int repetitions)
        {
            var sb = new StringBuilder(input.Length * repetitions);
            for (int i = 0; i < repetitions; i++)
            {
                sb.Append(input);
            }

            return sb.ToString();
        }

        protected void Write(params object[] args)
        {
            foreach (var a in args)
            {
                if (a != null)
                {
                    sb.Append(a);
                }
            }
        }

        protected void WriteLines(string text, string prefix = null, string firstprefix = null)
        {
            bool first = true;
            foreach (var line in text.Split('\n'))
            {
                if (first && firstprefix != null)
                {
                    sb.Append(firstprefix);
                    first = false;
                }
                else
                {
                    if (prefix != null)
                    {
                        sb.Append(prefix);
                    }
                }

                sb.AppendLine(line.Trim('\r'));
            }
        }

        protected void WriteLine(params object[] args)
        {
            Write(args);
            sb.AppendLine();
        }

        public int LineLength { get; set; }

        protected override void Write(TableOfContents toc, object parent)
        {
            this.WriteLine("@toc");
        }

        protected override void Write(Paragraph paragraph, object parent)
        {
            var tmp = sb;
            sb = new StringBuilder();
            WriteInlines(paragraph.Content, parent);
            var text = Wrap(sb.ToString(), LineLength);
            sb = tmp;
            Write(text);
            WriteLine();
            WriteLine();
        }

        protected override void Write(Section section, object parent)
        {
            this.WriteBlocks(section.Blocks, parent);
        }

        protected override void Write(Quote quote, object parent)
        {
            var tmp = sb;
            sb = new StringBuilder();
            WriteInlines(quote.Content, parent);
            var text = Wrap(sb.ToString(), LineLength);
            sb = tmp;
            WriteLine("///");
            WriteInlines(quote.Content, parent);
            WriteLines(text);
            WriteLine("///");
            WriteLine();
        }

        protected override void Write(UnorderedList list, object parent)
        {
            WriteItems(list.Items, parent, "- ");
            WriteLine();
        }

        protected override void Write(OrderedList list, object parent)
        {
            WriteItems(list.Items, parent, "# ");
            WriteLine();
        }

        protected override void Write(Table table, object parent)
        {
            foreach (var r in table.Rows)
            {
                foreach (var c in r.Cells)
                {
                    Write(c is TableHeaderCell ? "||" : "|");
                    WriteBlocks(c.Blocks, parent);
                }
                WriteLine("|");
            }
            WriteLine();
        }

        protected override void Write(CodeBlock codeBlock, object parent)
        {
            WriteLine("{{{");
            WriteLines(codeBlock.Text);
            WriteLine("}}}");
            WriteLine();
        }

        protected override void Write(HorizontalRuler ruler, object parent)
        {
            WriteLine("----");
            WriteLine();
        }

        protected override void Write(NonBreakingSpace nbsp, object parent)
        {
            Write(" ");
        }

        protected override void Write(Run run, object parent)
        {
            var text = Encode(run.Text);
            Write(text);
        }

        protected override void Write(Symbol symbol, object parent)
        {
            Write(symbol.Name);
        }

        protected override void Write(Span span, object parent)
        {
            WriteInlines(span.Content, parent);
        }

        protected override void Write(Strong strong, object parent)
        {
            Write("**");
            WriteInlines(strong.Content, parent);
            Write("**");
        }

        protected override void Write(Emphasized em, object parent)
        {
            Write("//");
            WriteInlines(em.Content, parent);
            Write("//");
        }

        protected override void Write(LineBreak linebreak, object parent)
        {
            WriteLine(@"\\");
        }

        protected override void Write(InlineCode inlineCode, object parent)
        {
            Write("{{", inlineCode.Code, "}}");
        }

        protected override void Write(Equation equation, object parent)
        {
        }
    }
}