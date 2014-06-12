namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    public class FormatterTests
    {
        public Document Load(string fileName)
        {
            var parser = new WikiParser(File.OpenRead, Path.GetDirectoryName(fileName));
            using (var stream = File.OpenRead(fileName))
            {
                return parser.Parse(stream);
            }
        }

        public Document LoadExample()
        {
            return this.Load(@"Input/Example.wiki");
        }
    }
}