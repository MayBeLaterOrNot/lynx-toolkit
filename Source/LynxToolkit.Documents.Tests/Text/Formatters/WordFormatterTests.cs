namespace LynxToolkit.Documents.OpenXml.Tests
{
    using System.IO;

    using LynxToolkit.Documents.OpenXml;
    using LynxToolkit.Documents.Tests;

    using NUnit.Framework;

    [TestFixture]
    public class WordFormatterTests : FormatterTests
    {
        [Test]
        public void Format()
        {
            var doc = LoadExample();
            var formatter = new WordFormatter { SymbolDirectory = @"Input\Images" };
            using (var stream = File.Create("Example.docx"))
            {
                formatter.Format(doc, stream);
            }
        }

        [Test]
        public void Format_WithTemplate()
        {
            var doc = LoadExample();
            var formatter = new WordFormatter { SymbolDirectory = @"Input\Images", Template = @"Input\Template.docx" };
            using (var stream = File.Create("Example_WithTemplate.docx"))
            {
                formatter.Format(doc, stream);
            }
        }
    }
}