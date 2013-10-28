namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class OWikiFormatterTests
    {
        [Test, ExpectedException]
        public void Format()
        {
            var source = File.ReadAllText(@"Input/Example.wiki");

            var parser = new WikiParser(File.OpenRead);
            var doc = parser.ParseFile(@"Input/Example.wiki");
            var wf = new OWikiFormatter();
            var owiki = wf.Format(doc);
        }
    }
}