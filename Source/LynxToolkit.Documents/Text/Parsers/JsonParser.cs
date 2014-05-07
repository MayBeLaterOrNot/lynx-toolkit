namespace LynxToolkit.Documents
{
    using System.IO;

    public class JsonParser :IDocumentParser
    {
        private static JsonSerializer js;

        static JsonParser()
        {
            js = new JsonSerializer();
        }

        public Document Parse(Stream stream)
        {
            var tr = new StreamReader(stream);
            return js.Deserialize<Document>(tr);
        }

        public static Document Parse(string json)
        {
            var tr = new StringReader(json);
            return js.Deserialize<Document>(tr);
        }
    }
}