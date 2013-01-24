using System.Text;

namespace LynxToolkit.Documents
{
    public class CreoleFormatter : WikiFormatterBase
    {
        protected CreoleFormatter(Document doc)
            : base(doc)
        {
        }

        public static string Format(Document doc)
        {
            var wf = new CreoleFormatter(doc);
            wf.Format();
            return wf.ToString();
        }

        protected override void Write(Header header)
        {
            Write(Repeat("=", header.Level), " ");
            WriteInlines(header.Content);
            WriteLine(" ", Repeat("=", header.Level));
            if (header.Level < 3)
            {
                WriteLine();
            }
        }


        protected override void Write(Anchor anchor, object parent)
        {
            Write("¤", anchor.Name, "¤");
        }

        protected override void Write(Hyperlink hyperlink, object parent)
        {
            Write("[[", hyperlink.Url);
            if (hyperlink.Content.Count > 0)
            {
                Write("|");
                WriteInlines(hyperlink.Content);
            }
            Write("]]");
        }

        protected override void Write(Image image, object parent)
        {
            Write("{{", image.Source);
            if (!string.IsNullOrEmpty(image.AlternateText))
            {
                Write("|", image.AlternateText);
            }
            Write("}}");
        }
    }
}