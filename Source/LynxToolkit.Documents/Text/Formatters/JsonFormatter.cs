namespace LynxToolkit.Documents
{
    using System.IO;

    public class JsonFormatter : IDocumentFormatter
    {
        private static JsonSerializer js;

        static JsonFormatter()
        {
            js = new JsonSerializer();
        }

        public void Format(Document doc, Stream stream)
        {
            using (var tw = new StreamWriter(stream))
            {
                js.Serialize(tw, doc);
            }
        }
    }
}