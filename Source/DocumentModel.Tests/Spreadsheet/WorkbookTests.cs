
namespace LynxToolkit.Documents.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class WorkbookTests
    {
        [Test]
        public void Library_CellReferences()
        {
            var workbook = WorkbookLibrary.CellReferences;
        }

        [Test]
        public void Library_HelloWorld()
        {
            var workbook = WorkbookLibrary.HelloWorld;
        }

        [Test]
        public void Library_DataTypes()
        {
            var workbook = WorkbookLibrary.HelloWorld;
        }

        [Test]
        public void Library_MergedCells()
        {
            var workbook = WorkbookLibrary.MergedCells;
        }

        [Test]
        public void Library_AllWorkbookds()
        {
            var workbooks = WorkbookLibrary.GetWorkbooks();
        }
    }
}
