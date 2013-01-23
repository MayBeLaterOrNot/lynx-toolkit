namespace LynxToolkit.Documents.Spreadsheet
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a sheet in a spreadsheet workbook.
    /// </summary>
    public class XSheet
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XSheet"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public XSheet(string name)
        {
            this.Name = name;
            this.Cells = new List<XCell>();
            this.Columns = new List<XColumn>();
            this.MergeCells = new List<XCellRange>();
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="width">The width.</param>
        public void AddColumn(double width = double.NaN)
        {
            this.Columns.Add(new XColumn { Width = width });
        }

        /// <summary>
        /// Gets or sets the value at the specified row and column.
        /// </summary>
        /// <param name="row">The row.</param>
        /// <param name="column">The column.</param>
        /// <returns>The value.</returns>
        public object this[int row, int column]
        {
            get
            {
                var cell = this.GetCell(row, column);
                return cell.Content;
            }

            set
            {
                var cell = this.GetCell(row, column);
                cell.Content = value;
            }
        }

        public object this[string cellReference]
        {
            get
            {
                var cell = this.GetCell(cellReference);
                return cell.Content;
            }

            set
            {
                var cell = this.GetCell(cellReference);
                cell.Content = value;
            }
        }

        public void ApplyStyle(int row, int column, XStyle style)
        {
            this.GetCell(row, column).Style = style;
        }

        public void SetFormula(int row, int column, string formula)
        {
            this.GetCell(row, column).Formula = formula;
        }

        public void SetFormula(string cellReference, string formula)
        {
            this.GetCell(cellReference).Formula = formula;
        }

        public void ApplyStyle(string cellReference, XStyle style)
        {
            foreach (var cell in this.GetCells(cellReference))
            {
                cell.Style = style;
            }
        }

        private IEnumerable<XCell> GetCells(string cellReference)
        {
            if (cellReference.Contains(":"))
            {
                var range = XCellRange.Parse(cellReference);
                for (int i = range.Start.Row; i <= range.End.Row; i++)
                {
                    for (int j = range.Start.Column; j <= range.End.Column; j++)
                    {
                        yield return this.GetCell(i, j);
                    }
                }
            }
            else
            {
                yield return this.GetCell(cellReference);
            }
        }

        public XCell GetCell(string cellReference)
        {
            return this.GetCell(XCellReference.Parse(cellReference));
        }

        public XCell GetCell(XCellReference r)
        {
            return this.GetCell(r.Row, r.Column);
        }

        public XCell GetCell(int row, int column)
        {
            var cell = this.Cells.FirstOrDefault(c => c.Row == row && c.Column == column);
            if (cell == null)
            {
                cell = new XCell(row, column);
                this.Cells.Add(cell);
            }

            return cell;
        }

        public List<XCell> Cells { get; private set; }
        public List<XColumn> Columns { get; private set; }
        public List<XCellRange> MergeCells { get; private set; }
        public string Name { get; set; }

        public void Merge(string range)
        {
            this.MergeCells.Add(XCellRange.Parse(range));
        }

        public double FindColumnWidth(uint column)
        {
            var maxLength = this.Cells.Where(c => c.Column == column).Max(c => c.FormatContent().Length);
            return maxLength + 2;
        }
    }
}