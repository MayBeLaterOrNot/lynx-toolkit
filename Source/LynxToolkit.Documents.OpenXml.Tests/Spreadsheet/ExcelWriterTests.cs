namespace LynxToolkit.Documents.OpenXml.Tests
{
    using LynxToolkit.Documents.Spreadsheet;
    using LynxToolkit.Documents.Tests;

    using NUnit.Framework;

    [TestFixture]
    public class ExcelWriterTests
    {
        [Test]
        public void HelloExcel()
        {
            var workbook = new Workbook();
            var sheet = workbook.AddSheet();
            sheet["A1"] = "Hello Excel";
            ExcelWriter.Export(workbook, "HelloExcel.xlsx");
        }

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
