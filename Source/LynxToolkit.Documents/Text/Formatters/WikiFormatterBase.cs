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
    using System.IO;

    public abstract class WikiFormatterBase : DocumentFormatter<TextWriter>
    {
        protected WikiFormatterBase()
        {
            this.LineLength = 72;
        }

        protected static char[] NewlineChars = new[] { '\r', '\n' };

        public string ToString(TextWriter context)
        {
            return context.ToString().Trim(NewlineChars);
        }

        protected void WriteItems(ListItemCollection items, TextWriter context, string prefix)
        {
            int i = 1;
            foreach (var item in items)
            {
                context.Write(prefix, i++);
                var innerWriter = new StringWriter();
                this.WriteInlines(item.Content, innerWriter);
                var text = innerWriter.ToString();
                text = Wrap(text, this.LineLength);
                this.WriteLines(context, text, Repeat(" ", prefix.Length), string.Empty);
            }
        }

        protected static string Wrap(string text, int maxlength)
        {
            var b = new StringBuilder();
            int i0 = 0;
            int i1 = maxlength;
            while (i1 < text.Length)
            {
                int i = i1;
                for (i = i1; i > i0; i--)
                {
                    if (string.IsNullOrEmpty(text[i].ToString()))
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

        protected void WriteLines(TextWriter context, string text, string prefix = null, string firstprefix = null)
        {
            bool first = true;
            foreach (var line in text.Split('\n'))
            {
                if (first && firstprefix != null)
                {
                    context.Write(firstprefix);
                    first = false;
                }
                else
                {
                    if (prefix != null)
                    {
                        context.Write(prefix);
                    }
                }

                context.WriteLine(line.Trim('\r'));
            }
        }

        public int LineLength { get; set; }

        protected override void Write(TableOfContents toc, TextWriter context)
        {
            context.WriteLine("@toc");
        }

        protected override void Write(Paragraph paragraph, TextWriter context)
        {
            var sb = new StringWriter();
            WriteInlines(paragraph.Content, sb);
            var text = Wrap(sb.ToString(), LineLength);
            context.Write(text);
            context.WriteLine();
            context.WriteLine();
        }

        protected override void Write(Section section, TextWriter context)
        {
            this.WriteBlocks(section.Blocks, context);
        }

        protected override void Write(Quote quote, TextWriter context)
        {
            var sb = new StringWriter();
            WriteInlines(quote.Content, sb);
            var text = Wrap(sb.ToString(), LineLength);
            context.WriteLine("///");
            WriteInlines(quote.Content, context);
            WriteLines(context, text);
            context.WriteLine("///");
            context.WriteLine();
        }

        protected override void Write(UnorderedList list, TextWriter context)
        {
            WriteItems(list.Items, context, "- ");
            context.WriteLine();
        }

        protected override void Write(OrderedList list, TextWriter context)
        {
            WriteItems(list.Items, context, "# ");
            context.WriteLine();
        }

        protected override void Write(Table table, TextWriter context)
        {
            foreach (var r in table.Rows)
            {
                foreach (var c in r.Cells)
                {
                    context.Write(c is TableHeaderCell ? "||" : "|");
                    WriteBlocks(c.Blocks, context);
                }
                context.WriteLine("|");
            }
            context.WriteLine();
        }

        protected override void Write(CodeBlock codeBlock, TextWriter context)
        {
            context.WriteLine("{{{");
            WriteLines(context, codeBlock.Text);
            context.WriteLine("}}}");
            context.WriteLine();
        }

        protected override void Write(HorizontalRuler ruler, TextWriter context)
        {
            context.WriteLine("----");
            context.WriteLine();
        }

        protected override void Write(NonBreakingSpace nbsp, TextWriter context)
        {
            context.Write(" ");
        }

        protected override void Write(Run run, TextWriter context)
        {
            var text = Encode(run.Text);
            context.Write(text);
        }

        protected override void Write(Symbol symbol, TextWriter context)
        {
            context.Write(symbol.Name);
        }

        protected override void Write(Span span, TextWriter context)
        {
            WriteInlines(span.Content, context);
        }

        protected override void Write(Strong strong, TextWriter context)
        {
            context.Write("**");
            WriteInlines(strong.Content, context);
            context.Write("**");
        }

        protected override void Write(Emphasized em, TextWriter context)
        {
            context.Write("//");
            WriteInlines(em.Content, context);
            context.Write("//");
        }

        protected override void Write(LineBreak linebreak, TextWriter context)
        {
            context.WriteLine(@"\\");
        }

        protected override void Write(InlineCode inlineCode, TextWriter context)
        {
            context.Write("{{" + inlineCode.Code + "}}");
        }

        protected override void Write(Equation equation, TextWriter context)
        {
        }
    }
}