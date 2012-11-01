namespace LynxToolkit.Documents
{
    public class ConfluenceFormatter : CreoleFormatter
    {
        protected ConfluenceFormatter(Document doc)
            : base(doc)
        {

        }
        public static new string Format(Document doc)
        {
            var wf = new ConfluenceFormatter(doc);
            wf.FormatCore();
            return wf.ToString();
        }

        protected override void Write(Header header)
        {
            Write("h", header.Level, ". ");
            WriteInlines(header.Content);
            WriteLine();
            WriteLine();
        }

        protected override void Write(Quote quote)
        {
            WriteLine("///");
            WriteInlines(quote.Content);
            WriteLine();
            WriteLine("///");
            WriteLine();
        }

        protected override void Write(CodeBlock codeBlock)
        {
            WriteLine("{code", codeBlock.Language != Language.Unknown ? ":" + codeBlock.Language : "", "}");
            foreach (var line in codeBlock.Text.Split('\n'))
            {
                WriteLine(line.Trim('\r'));
            }
            WriteLine("{code}");
            WriteLine();
        }

        protected override void Write(Strong strong)
        {
            Write("*");
            WriteInlines(strong.Content);
            Write("*");
        }

        protected override void Write(Emphasized em)
        {
            Write("_");
            WriteInlines(em.Content);
            Write("_");
        }

        protected override void Write(LineBreak linebreak)
        {
            WriteLine(@"//");
        }

        protected override void Write(InlineCode inlineCode)
        {
            Write("{{", inlineCode.Text, "}}");
        }

        protected override void Write(Hyperlink hyperlink)
        {
            Write("[");
            if (hyperlink.Content.Count > 0)
            {
                WriteInlines(hyperlink.Content);
                Write("|");
            }
            Write(hyperlink.Url);
            Write("]");
        }

        protected override void Write(Image image)
        {
            Write("!", image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("|alt=", image.AlternateText);
            }
            Write("!");
        }
    }
}