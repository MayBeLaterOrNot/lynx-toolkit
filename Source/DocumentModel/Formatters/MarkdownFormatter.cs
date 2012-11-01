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
            wf.FormatCore();
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

        protected override void Write(List list)
        {
            WriteItems(list.Items, "- ");
            WriteLine();
        }

        protected override void Write(OrderedList list)
        {
            WriteItems(list.Items, "{0}. ");
            WriteLine();
        }

        protected override void Write(Anchor anchor)
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
        string prefix;

        protected override void WriteInline(Inline inline)
        {
            if (firstInline && prefix != null)
            {
                Write(prefix);
                firstInline = false;
            }
            base.WriteInline(inline);
            if (inline is LineBreak) firstInline = true;
        }

        protected override void Write(Quote quote)
        {
            var tmp = sb;
            sb = new StringBuilder();
            WriteInlines(quote.Content);
            var text = Wrap(sb.ToString(), LineLength);
            sb = tmp;

            WriteLines(text,"> ");
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

        protected override void Write(Run run)
        {
            Write(Encode(run.Text));
        }

        protected override void Write(Strong strong)
        {
            Write("**");
            WriteInlines(strong.Content);
            Write("**");
        }

        protected override void Write(Emphasized em)
        {
            Write("*");
            WriteInlines(em.Content);
            Write("*");
        }

        protected override void Write(LineBreak linebreak)
        {
            WriteLine(@"  ");
        }

        protected override void Write(InlineCode inlineCode)
        {
            Write("`", inlineCode.Text, "`");
        }

        protected override void Write(Hyperlink hyperlink)
        {
            Write("[");
            WriteInlines(hyperlink.Content);
            Write("]");
            Write("(", hyperlink.Url);
            if (hyperlink.Title != null)
                Write(" \"", hyperlink.Title, "\"");
            Write(")");
        }

        protected override void Write(Image image)
        {
            Write("![", image.Source, "]");
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("(", image.AlternateText, ")");
            }
        }
    }
}