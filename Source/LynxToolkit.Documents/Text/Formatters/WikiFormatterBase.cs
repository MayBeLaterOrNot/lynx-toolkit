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

namespace LynxToolkit.Documents
{
    using System.IO;
    using System.Text;

    public abstract class WikiFormatterBase : DocumentFormatter<TextWriter>
    {
        protected WikiFormatterBase()
        {
            this.EscapeCharacter = '\\';
            this.LineLength = 72;
            this.UnorderedListItemPrefix = "- ";
            this.OrderedListItemPrefix = "# ";

            this.HorizontalRulerText = "----";
            this.LineBreakText = @"\\";
            this.HeaderPrefix = "=";

            this.StrongWrapper = "**";
            this.EmphasizedWrapper = "//";
            this.InlineCodePrefix = "{{";
            this.InlineCodeSuffix = "}}";

            this.TableHeaderPrefix = "|=";
            this.TableCellSeparator = "|";
        }

        public int LineLength { get; set; }

        public char EscapeCharacter { get; protected set; }

        public string UnorderedListItemPrefix { get; set; }

        public string OrderedListItemPrefix { get; set; }

        public string HorizontalRulerText { get; set; }

        public string LineBreakText { get; set; }

        public string HeaderPrefix { get; set; }

        public string StrongWrapper { get; set; }

        public string EmphasizedWrapper { get; set; }

        public string InlineCodePrefix { get; set; }

        public string InlineCodeSuffix { get; set; }

        public string TableCellSeparator { get; set; }

        public string TableHeaderPrefix { get; set; }

        public override void Format(Document doc, Stream stream)
        {
            var w = new StreamWriter(stream);
            this.WriteBlocks(doc.Blocks, w);
        }

        protected void WriteItems(ListItemCollection items, TextWriter context, string prefix)
        {
            int i = 1;
            foreach (var item in items)
            {
                var actualPrefix = prefix.Replace("i", i.ToString());
                context.Write(actualPrefix, i++);
                var innerWriter = new StringWriter();
                this.WriteInlines(item.Content, innerWriter);
                var text = innerWriter.ToString();
                text = Wrap(text, this.LineLength);
                this.WriteLines(context, text, Repeat(" ", actualPrefix.Length), string.Empty);
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

        protected override void Write(TableOfContents toc, TextWriter context)
        {
            context.WriteLine("@toc");
        }

        protected override void Write(Header header, TextWriter context)
        {
            context.Write(Repeat(this.HeaderPrefix, header.Level));
            context.Write(" ");
            this.WriteInlines(header.Content, context);
            context.WriteLine();

            // Empty line
            context.WriteLine();
        }


        protected override void Write(Paragraph paragraph, TextWriter context)
        {
            var sb = new StringWriter();
            this.WriteInlines(paragraph.Content, sb);
            var text = Wrap(sb.ToString(), this.LineLength);
            context.WriteLine(text);
            context.WriteLine();
        }

        protected override void Write(Section section, TextWriter context)
        {
            this.WriteBlocks(section.Blocks, context);
        }

        protected override void Write(Quote quote, TextWriter context)
        {
            var sb = new StringWriter();
            this.WriteInlines(quote.Content, sb);
            var text = Wrap(sb.ToString(), this.LineLength);
            context.WriteLine("///");
            this.WriteInlines(quote.Content, context);
            this.WriteLines(context, text);
            context.WriteLine("///");
            context.WriteLine();
        }

        protected override void Write(UnorderedList list, TextWriter context)
        {
            this.WriteItems(list.Items, context, this.UnorderedListItemPrefix);
            context.WriteLine();
        }

        protected override void Write(OrderedList list, TextWriter context)
        {
            this.WriteItems(list.Items, context, this.OrderedListItemPrefix);
            context.WriteLine();
        }

        protected override void Write(Table table, TextWriter context)
        {
            foreach (var r in table.Rows)
            {
                foreach (var c in r.Cells)
                {
                    context.Write(c is TableHeaderCell ? this.TableHeaderPrefix : this.TableCellSeparator);
                    this.WriteBlocks(c.Blocks, context);
                }

                context.WriteLine(this.TableCellSeparator);
            }

            context.WriteLine();
        }

        protected override void Write(CodeBlock codeBlock, TextWriter context)
        {
            context.WriteLine("{{{");
            this.WriteLines(context, codeBlock.Text);
            context.WriteLine("}}}");
            context.WriteLine();
        }

        protected override void Write(HorizontalRuler ruler, TextWriter context)
        {
            context.WriteLine();
            context.WriteLine(HorizontalRulerText);
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
            context.Write(this.StrongWrapper);
            WriteInlines(strong.Content, context);
            context.Write(this.StrongWrapper);
        }

        protected override void Write(Emphasized em, TextWriter context)
        {
            context.Write(this.EmphasizedWrapper);
            WriteInlines(em.Content, context);
            context.Write(this.EmphasizedWrapper);
        }

        protected override void Write(LineBreak linebreak, TextWriter context)
        {
            context.WriteLine(this.LineBreakText);
        }

        protected override void Write(InlineCode inlineCode, TextWriter context)
        {
            context.Write(this.InlineCodePrefix);
            context.Write(inlineCode.Code);
            context.Write(this.InlineCodeSuffix);
        }


        protected override void Write(Equation equation, TextWriter context)
        {
            context.Write(equation.Content);
        }

        protected override void Write(Anchor anchor, TextWriter context)
        {
            context.Write(anchor.Name);
        }
    }
}