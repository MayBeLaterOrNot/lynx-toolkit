namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class JsonFormatterTests
    {
        [Test, ExpectedException]
        public void Format()
        {
            var parser = new WikiParser(File.OpenRead);
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var ms = new MemoryStream();
            var jf = new JsonFormatter();
            var json = jf.Format(doc);
        }
    }
}