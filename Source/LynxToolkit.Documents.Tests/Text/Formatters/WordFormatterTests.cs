namespace LynxToolkit.Documents.OpenXml.Tests
{
    using System.IO;

    using LynxToolkit.Documents.OpenXml;

    using NUnit.Framework;

    [TestFixture]
    public class WordFormatterTests
    {
        [Test]
        public void Format()
        {
            var parser = new WikiParser(File.OpenRead);
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var formatter = new WordFormatter();
            formatter.SymbolDirectory = @"Input\Images";
            // options.Template=
            using (var stream = File.Create("Example.docx"))
            {
                formatter.Format(doc, stream);
            }
        }
    }
}