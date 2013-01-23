namespace LynxToolkit.Documents.Spreadsheet
{
    /// <summary>
    /// Represents a cell format in a spreadsheet.
    /// </summary>
    public class XStyle
    {
        public string NumberFormat { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public double FontSize { get; set; }
        public string FontName { get; set; }
        public uint? Foreground { get; set; }
        public uint? Background { get; set; }
        public BorderStyle LeftBorderStyle { get; set; }
        public BorderStyle RightBorderStyle { get; set; }
        public BorderStyle TopBorderStyle { get; set; }
        public BorderStyle BottomBorderStyle { get; set; }
        public uint LeftBorderColor { get; set; }
        public uint RightBorderColor { get; set; }
        public uint TopBorderColor { get; set; }
        public uint BottomBorderColor { get; set; }
    }
}