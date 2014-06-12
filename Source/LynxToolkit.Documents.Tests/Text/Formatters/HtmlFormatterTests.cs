namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    using LynxToolkit.Documents.Html;

    using NUnit.Framework;

    [TestFixture]
    public class HtmlFormatterTests : FormatterTests
    {
        [Test]
        public void Format()
        {
            var doc = LoadExample();
            var formatter = new HtmlFormatter { SymbolDirectory = @"Input\Images" };
            using (var stream = File.Create("Example.html"))
            {
                formatter.Format(doc, stream);
            }

            var html = formatter.Format(doc);
        }
    }
}