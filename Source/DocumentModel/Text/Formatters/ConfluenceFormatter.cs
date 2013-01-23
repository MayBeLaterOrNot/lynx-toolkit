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
            wf.Format();
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

        protected override void Write(Strong strong, object parent)
        {
            Write("*");
            WriteInlines(strong.Content);
            Write("*");
        }

        protected override void Write(Emphasized em, object parent)
        {
            Write("_");
            WriteInlines(em.Content);
            Write("_");
        }

        protected override void Write(LineBreak linebreak, object parent)
        {
            WriteLine(@"//");
        }

        protected override void Write(InlineCode inlineCode, object parent)
        {
            Write("{{", inlineCode.Code, "}}");
        }

        protected override void Write(Hyperlink hyperlink, object parent)
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

        protected override void Write(Image image, object parent)
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