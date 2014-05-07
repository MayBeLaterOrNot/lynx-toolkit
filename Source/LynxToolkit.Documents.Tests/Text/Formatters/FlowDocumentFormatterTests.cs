namespace LynxToolkit.Documents.Wpf.Tests
{
    using System.IO;

    using LynxToolkit.Documents.Wpf;

    using NUnit.Framework;

    [TestFixture]
    public class FlowDocumentFormatterTests
    {
        [Test]
        public void Format()
        {
            var parser = new WikiParser(File.OpenRead);
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var formatter = new FlowDocumentFormatter { SymbolDirectory = @"Input\Images" };
            using (var stream = File.Create("Example.xps"))
            {
                formatter.Format(doc, stream);
            }
        }
    }
}