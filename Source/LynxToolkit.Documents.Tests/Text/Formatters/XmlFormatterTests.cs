namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class XmlFormatterTests
    {
        [Test]
        public void Format()
        {
            var parser = new WikiParser(File.OpenRead);
            var doc = parser.ParseFile(@"Input/Example.wiki");
            //var xmlFormatter = new XmlFormatter();
            //var xml = xmlFormatter.Format(doc);
        }
    }
}