namespace LynxToolkit.Documents
{
    public class OWikiFormatter : MarkdownFormatter
    {
        protected OWikiFormatter(Document doc)
            : base(doc)
        {
        }

        public static new string Format(Document doc)
        {
            var wf = new OWikiFormatter(doc);
            wf.FormatCore();
            return wf.ToString();
        }

        protected override void Write(HorizontalRuler ruler)
        {
            WriteLine("- - -");
            WriteLine();
        }
        
        protected override void Write(Hyperlink hyperlink)
        {
            Write("[");
            Write(hyperlink.Url);
            bool isDefaultTitle = false;
            if (hyperlink.Content.Count == 1)
            {
                var defaultTitle = hyperlink.Url != null ? hyperlink.Url.Replace("http://", "") : null;
                var run1 = hyperlink.Content[0] as Run;
                isDefaultTitle = run1 != null && string.Equals(run1.Text, defaultTitle);
            }

            if (hyperlink.Content.Count > 0 && !isDefaultTitle)
            {
                Write("|");
                WriteInlines(hyperlink.Content);
            }

            if (!string.IsNullOrWhiteSpace(hyperlink.Title))
            {
                Write("|");
                Write(hyperlink.Title);
            }
            Write("]");
        }

        protected override void Write(Image image)
        {
            Write("{", image.Source);
            if (!string.IsNullOrWhiteSpace(image.AlternateText))
            {
                Write("|", image.AlternateText);
            }
            if (!string.IsNullOrWhiteSpace(image.Link))
            {
                Write("|", image.Link);
            }
            Write("}");
        }
    }
}