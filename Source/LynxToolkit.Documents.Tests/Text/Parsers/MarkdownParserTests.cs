namespace LynxToolkit.Documents.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class MarkdownParserTests
    {
        [Test, Ignore]
        public void Header1()
        {
            var o2 = new MarkdownParser();
            var model = o2.ParseText("# Header 1");
            Assert.AreEqual(1, model.Blocks.Count);
            var h1 = (Header)model.Blocks[0];
            var r1 = (Run)h1.Content[0];
            Assert.AreEqual("Header 1", r1.Text);
            Assert.AreEqual(1, h1.Level);
        }
    }
}