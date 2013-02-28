namespace LynxToolkit.Documents.OpenXml.Tests
{
    using LynxToolkit.Documents.OpenXml;

    using NUnit.Framework;

    [TestFixture]
    public class WordFormatterTests
    {
        [Test]
        public void Format()
        {
            var parser = new WikiParser();
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var formatter = new WordFormatter();
            formatter.SymbolDirectory = @"Input\Images";
            // options.Template=
            formatter.Format(doc, "Example.docx");
        }
    }
}