namespace LynxToolkit.Documents
{
    public class CodeplexFormatter : WikiFormatterBase
    {
        public static string Format(Document doc)
        {
            var wf = new CodeplexFormatter(doc);
            wf.FormatCore();
            return wf.ToString();
        }

        protected CodeplexFormatter(Document doc)
            : base(doc)
        {

        }

        protected override void Write(Header header)
        {
            Write(Repeat("!", header.Level), " ");
            WriteInlines(header.Content);
            WriteLine();
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
            Write("[url:");
            Write(hyperlink.Url);
            if (hyperlink.Content.Count > 0)
            {
                Write("|");
                WriteInlines(hyperlink.Content);
            }
            Write("]");
        }

        protected override void Write(Image image)
        {
            Write("[image:", image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("|", image.AlternateText);
            }
            Write("]");
        }

        protected override void Write(Anchor anchor)
        {
        }
    }
}