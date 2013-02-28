namespace LynxToolkit.Documents.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class HtmlFormatterTests
    {
        [Test]
        public void Format()
        {
            var parser = new WikiParser();
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var formatter = new HtmlFormatter();
            formatter.SymbolDirectory = @"Input\Images";
            formatter.Format(doc, "Example.html");
        }
    }
}