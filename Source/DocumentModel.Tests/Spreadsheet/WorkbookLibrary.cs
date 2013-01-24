namespace LynxToolkit.Documents.Tests
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;

    using LynxToolkit.Documents.Spreadsheet;

    public static class WorkbookLibrary
    {
        public static Dictionary<string, Workbook> GetWorkbooks()
        {
            var d = new Dictionary<string, Workbook>();
            foreach (var pi in typeof(WorkbookLibrary).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                var wb = pi.GetValue(null, null) as Workbook;
                if (wb != null)
                {
                    d.Add(pi.Name, wb);
                }
            }

            return d;
        }

        public static Workbook HelloWorld
        {
            get
            {
                var wb = new Workbook();
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Hello world";
                return wb;
            }
        }

        public static Workbook CellReferences
        {
            get
            {
                var wb = new Workbook();
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "A1";
                sheet1[0, 1] = "B1";
                return wb;
            }
        }

        public static Workbook MergedCells
        {
            get
            {
                var wb = new Workbook();
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "A1";
                sheet1["B1"] = "B1";
                sheet1.Merge("A1:A2");
                sheet1.Merge("B1:C1");
                return wb;
            }
        }

        public static Workbook DataTypes
        {
            get
            {
                var wb = new Workbook();
                var sheet1 = wb.AddSheet("DataTypes");
                sheet1["A1"] = "Text";
                sheet1["A2"] = 1;
                sheet1["A3"] = Math.PI;
                sheet1["A4"] = true;
                sheet1["A5"] = new DateTime(2013, 1, 24);
                return wb;
            }
        }
    }
}