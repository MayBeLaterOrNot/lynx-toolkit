namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    using NUnit.Framework;

    /// <summary>
    /// The markdown formatter tests.
    /// </summary>
    [TestFixture]
    public class WikiFormatterTests : FormatterTests
    {
        [Test]
        public void Format_OWiki()
        {
            var doc = LoadExample();
            var formatter = new OWikiFormatter();
            var output = formatter.Format(doc);
            File.WriteAllText("Example.wiki", output);
        }

        [Test]
        public void Format_Markdown()
        {
            var doc = LoadExample();
            var formatter = new MarkdownFormatter();
            var output = formatter.Format(doc);
            File.WriteAllText("Example.md", output);
        }

        [Test]
        public void Format_Creole()
        {
            var doc = LoadExample();
            var formatter = new CreoleFormatter();
            var output = formatter.Format(doc);
            File.WriteAllText("Example.creole", output);
        }

        [Test]
        public void Format_Codeplex()
        {
            var doc = LoadExample();
            var formatter = new CodeplexFormatter();
            var output = formatter.Format(doc);
            File.WriteAllText("Example.codeplex", output);
        }

        [Test]
        public void Format_Confluence()
        {
            var doc = LoadExample();
            var formatter = new ConfluenceFormatter();
            var output = formatter.Format(doc);
            File.WriteAllText("Example.confluence", output);
        }
    }
}