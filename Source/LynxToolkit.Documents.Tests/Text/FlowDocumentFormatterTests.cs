namespace LynxToolkit.Documents.Wpf.Tests
{
    using LynxToolkit.Documents.OpenXml;
    using LynxToolkit.Documents.Wpf;

    using NUnit.Framework;

    [TestFixture]
    public class FlowDocumentFormatterTests
    {
        [Test]
        public void Format()
        {
            var parser = new WikiParser();
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var formatter = new FlowDocumentFormatter();
            formatter.SymbolDirectory = @"Input\Images";
            formatter.Format(doc, @"Example.xps");
        }
    }
}