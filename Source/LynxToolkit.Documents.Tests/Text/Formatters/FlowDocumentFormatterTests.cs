namespace LynxToolkit.Documents.Wpf.Tests
{
    using System.IO;

    using LynxToolkit.Documents.Tests;
    using LynxToolkit.Documents.Wpf;

    using NUnit.Framework;

    [TestFixture]
    public class FlowDocumentFormatterTests : FormatterTests
    {
        [Test, RequiresSTA]
        public void Format()
        {
            var doc = LoadExample();
            var formatter = new FlowDocumentFormatter { SymbolDirectory = @"Input\Images" };
            using (var stream = File.Create("Example.xps"))
            {
                formatter.Format(doc, stream);
            }
        }
    }
}