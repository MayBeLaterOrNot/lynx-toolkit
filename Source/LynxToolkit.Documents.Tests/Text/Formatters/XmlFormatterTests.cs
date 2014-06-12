namespace LynxToolkit.Documents.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class XmlFormatterTests : FormatterTests
    {
        [Test]
        public void Format()
        {
            var doc = LoadExample();
            //var xmlFormatter = new XmlFormatter();
            //var xml = xmlFormatter.Format(doc);
        }
    }
}