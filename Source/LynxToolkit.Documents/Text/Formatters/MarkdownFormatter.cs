// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MarkdownFormatter.cs" company="Lynx Toolkit">
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
    using System.Text;

    public class MarkdownFormatter : WikiFormatterBase
    {
        protected MarkdownFormatter(Document doc)
            : base(doc)
        {
        }

        public static string Format(Document doc)
        {
            var wf = new MarkdownFormatter(doc);
            wf.Format();
            return wf.ToString();
        }

        protected override void Write(Header header)
        {
            if (header.Level > 2)
            {
                Write(Repeat("#", header.Level), " ");
            }
            int l0 = sb.Length;
            WriteInlines(header.Content);
            int l1 = sb.Length - l0;
            WriteLine();
            if (header.Level == 1)
            {
                WriteLine(Repeat("=", l1));
                WriteLine();
            }
            if (header.Level == 2)
            {
                WriteLine(Repeat("-", l1));
                WriteLine();
            }
        }

        protected override void Write(UnorderedList list)
        {
            WriteItems(list.Items, "- ");
            WriteLine();
        }

        protected override void Write(OrderedList list)
        {
            WriteItems(list.Items, "{0}. ");
            WriteLine();
        }

        protected override void Write(Anchor anchor, object parent)
        {
            Write("#", anchor.Name, " ");
        }

        protected override void Write(Table table)
        {
            foreach (var r in table.Rows)
            {
                foreach (var c in r.Cells)
                {
                    Write(c is TableHeaderCell ? "||" : "|");
                    this.Write(c.HorizontalAlignment != HorizontalAlignment.Left ? "  " : " ");
                    WriteInlines(c.Content);
                    this.Write(c.HorizontalAlignment == HorizontalAlignment.Center ? "  " : " ");
                }
                WriteLine("|");
            }
            WriteLine();
        }

        bool firstInline;
        string prefix = string.Empty;

        protected override void WriteInline(Inline inline, object parent)
        {
            if (firstInline && prefix != null)
            {
                Write(prefix);
                firstInline = false;
            }

            base.WriteInline(inline, parent);
            if (inline is LineBreak) firstInline = true;
        }

        protected override void Write(Quote quote)
        {
            var tmp = sb;
            sb = new StringBuilder();
            WriteInlines(quote.Content);
            var text = Wrap(sb.ToString(), LineLength);
            sb = tmp;

            WriteLines(text, "> ");
            WriteLine();
        }

        protected override void Write(CodeBlock codeBlock)
        {
            WriteLine("```", codeBlock.Language.ToString().ToLower());
            foreach (var line in codeBlock.Text.Split('\n'))
            {
                WriteLine(line.Trim('\r'));
            }
            WriteLine("```");
            WriteLine();
        }

        protected override void Write(HorizontalRuler ruler)
        {
            WriteLine();
            WriteLine("----");
            WriteLine();
        }

        protected override void Write(Run run, object parent)
        {
            Write(Encode(run.Text));
        }

        protected override void Write(Strong strong, object parent)
        {
            Write("**");
            WriteInlines(strong.Content);
            Write("**");
        }

        protected override void Write(Emphasized em, object parent)
        {
            Write("*");
            WriteInlines(em.Content);
            Write("*");
        }

        protected override void Write(LineBreak linebreak, object parent)
        {
            WriteLine(@"  ");
        }

        protected override void Write(InlineCode inlineCode, object parent)
        {
            Write("`", inlineCode.Code, "`");
        }

        protected override void Write(Hyperlink hyperlink, object parent)
        {
            Write("[");
            WriteInlines(hyperlink.Content);
            Write("]");
            Write("(", hyperlink.Url);
            if (hyperlink.Title != null)
                Write(" \"", hyperlink.Title, "\"");
            Write(")");
        }

        protected override void Write(Image image, object parent)
        {
            Write("![", image.Source, "]");
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("(", image.AlternateText, ")");
            }
        }
    }
}