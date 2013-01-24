namespace LynxToolkit.Documents.OpenXml.Tests
{
    using LynxToolkit.Documents.Tests;

    using NUnit.Framework;

    [TestFixture]
    public class ExcelWriterTests
    {
        [Test]
        public void Export_AllWorkbooksInLibrary()
        {
            foreach (var kvp in WorkbookLibrary.GetWorkbooks())
            {
                var fileName = kvp.Key + ".xlsx";
                ExcelWriter.Export(kvp.Value, fileName);
            }
        }
    }
}
