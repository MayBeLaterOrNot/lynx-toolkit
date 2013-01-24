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

        public static Workbook Foreground
        {
            get
            {
                var wb = new Workbook();
                var style1 = wb.AddStyle(foreground: 0xFF0000);
                var style2 = wb.AddStyle(foreground: 0x0000FF);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "Red";
                sheet1["A3"] = "Blue";
                sheet1.ApplyStyle("A2", style1);
                sheet1.ApplyStyle("A3", style2);
                return wb;
            }
        }

        public static Workbook Background
        {
            get
            {
                var wb = new Workbook();
                var style1 = wb.AddStyle(background: 0xFF8080);
                var style2 = wb.AddStyle(background: 0x8080FF);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "On red";
                sheet1["A3"] = "On blue";
                sheet1.ApplyStyle("A2", style1);
                sheet1.ApplyStyle("A3", style2);
                return wb;
            }
        }

        public static Workbook Bold
        {
            get
            {
                var wb = new Workbook();
                var style1 = wb.AddStyle(bold: true);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "Bold";
                sheet1.ApplyStyle("A2", style1);
                return wb;
            }
        }

        public static Workbook Italic
        {
            get
            {
                var wb = new Workbook();
                var style1 = wb.AddStyle(italic: true);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "Italic";
                sheet1.ApplyStyle("A2", style1);
                return wb;
            }
        }

        public static Workbook Font
        {
            get
            {
                var wb = new Workbook();
                var style1 = wb.AddStyle(fontName: "Courier New", fontSize: 24);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "Courier New, 24pt";
                sheet1.ApplyStyle("A2", style1);
                return wb;
            }
        }

        public static Workbook HorizontalAlignment
        {
            get
            {
                var wb = new Workbook();
                var left = wb.AddStyle(horizontalAlignment: Spreadsheet.HorizontalAlignment.Left);
                var center = wb.AddStyle(horizontalAlignment: Spreadsheet.HorizontalAlignment.Center);
                var right = wb.AddStyle(horizontalAlignment: Spreadsheet.HorizontalAlignment.Right);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "Left";
                sheet1["A3"] = "Center";
                sheet1["A4"] = "Right";
                sheet1["B1"] = 3.14;
                sheet1["B2"] = 3.14;
                sheet1["B3"] = 3.14;
                sheet1["B4"] = 3.14;
                sheet1.ApplyStyle("A2:B2", left);
                sheet1.ApplyStyle("A3:B3", center);
                sheet1.ApplyStyle("A4:B4", right);
                return wb;
            }
        }

        public static Workbook VerticalAlignment
        {
            get
            {
                var wb = new Workbook();
                var top = wb.AddStyle(verticalAlignment: Spreadsheet.VerticalAlignment.Top);
                var middle = wb.AddStyle(verticalAlignment: Spreadsheet.VerticalAlignment.Middle);
                var bottom = wb.AddStyle(verticalAlignment: Spreadsheet.VerticalAlignment.Bottom);
                var sheet1 = wb.AddSheet();
                sheet1["A1"] = "Default";
                sheet1["A2"] = "Top";
                sheet1["A3"] = "Middle";
                sheet1["A4"] = "Bottom";
                sheet1.ApplyStyle("A2:B2", top);
                sheet1.ApplyStyle("A3:B3", middle);
                sheet1.ApplyStyle("A4:B4", bottom);
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

        public static Workbook Formulas
        {
            get
            {
                var wb = new Workbook();
                var sheet1 = wb.AddSheet("Formulas");
                sheet1["A1"] = 2;
                sheet1["A2"] = 3;
                sheet1["A3"] = 5;
                sheet1.SetFormula("A4", "+SUM(A1:A3)");
                sheet1.SetFormula("A5", "+A4*A4");
                return wb;
            }
        }
    }
}