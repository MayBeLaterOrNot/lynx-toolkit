namespace LynxToolkit.Documents.Tests
{
    using System.IO;

    using NUnit.Framework;

    [TestFixture]
    public class JsonFormatterTests : FormatterTests
    {
        [Test, ExpectedException]
        public void Format()
        {
            var doc = LoadExample();
            var ms = new MemoryStream();
            var jf = new JsonFormatter();
            var json = jf.Format(doc);
        }
    }
}