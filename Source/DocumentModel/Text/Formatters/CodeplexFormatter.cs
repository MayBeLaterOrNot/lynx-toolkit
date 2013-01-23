namespace LynxToolkit.Documents
{
    public class CodeplexFormatter : WikiFormatterBase
    {
        public static string Format(Document doc)
        {
            var wf = new CodeplexFormatter(doc);
            wf.Format();
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
            Write("[url:");
            Write(hyperlink.Url);
            if (hyperlink.Content.Count > 0)
            {
                Write("|");
                WriteInlines(hyperlink.Content);
            }
            Write("]");
        }

        protected override void Write(Image image, object parent)
        {
            Write("[image:", image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("|", image.AlternateText);
            }
            Write("]");
        }

        protected override void Write(Anchor anchor, object parent)
        {
        }
    }
}