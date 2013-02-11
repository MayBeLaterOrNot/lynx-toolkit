// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Worksheet.cs" company="Lynx Toolkit">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   Represents a spreadsheet in a workbook.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents.Spreadsheet
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a spreadsheet in a workbook.
    /// </summary>
    public class Worksheet : IEnumerable<Cell>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Worksheet"/> class.
        /// </summary>
        /// <param name="name">
        /// The name.
        /// </param>
        public Worksheet(string name)
        {
            this.Name = name;
            this.Cells = new List<Cell>();
            this.Columns = new List<Column>();
            this.MergeCells = new List<CellRange>();
        }

        /// <summary>
        /// Gets the cells.
        /// </summary>
        public List<Cell> Cells { get; private set; }

        /// <summary>
        /// Gets the columns.
        /// </summary>
        public List<Column> Columns { get; private set; }

        /// <summary>
        /// Gets the merge cells.
        /// </summary>
        public List<CellRange> MergeCells { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the value at the specified row and column.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <returns>
        /// The value.
        /// </returns>
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

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="cellReference">
        /// The cell reference.
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
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

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="width">
        /// The width.
        /// </param>
        public void AddColumn(double width = double.NaN)
        {
            this.Columns.Add(new Column { Width = width });
        }

        /// <summary>
        /// The apply style.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <param name="style">
        /// The style.
        /// </param>
        public void ApplyStyle(int row, int column, Style style)
        {
            this.GetCell(row, column).Style = style;
        }

        /// <summary>
        /// The apply style.
        /// </summary>
        /// <param name="cellReference">
        /// The cell reference.
        /// </param>
        /// <param name="style">
        /// The style.
        /// </param>
        public void ApplyStyle(string cellReference, Style style)
        {
            foreach (var cell in this.GetCells(cellReference))
            {
                cell.Style = style;
            }
        }

        /// <summary>
        /// Finds the maximum column width.
        /// </summary>
        /// <param name="column">
        /// The column index (0-based).
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public double FindMaximumColumnWidth(uint column)
        {
            var maxLength = this.Cells.Where(c => c.Column == column).Max(c => c.FormatContent().Length);
            return maxLength + 2;
        }

        /// <summary>
        /// Gets the cell from the specified cell reference.
        /// </summary>
        /// <param name="cellReference">
        /// The cell reference.
        /// </param>
        /// <returns>
        /// The <see cref="Cell"/>.
        /// </returns>
        public Cell GetCell(string cellReference)
        {
            return this.GetCell(CellReference.Parse(cellReference));
        }

        /// <summary>
        /// Gets the cell from the specified cell reference.
        /// </summary>
        /// <param name="r">
        /// The reference (e.g. "A1").
        /// </param>
        /// <returns>
        /// The <see cref="Cell"/>.
        /// </returns>
        public Cell GetCell(CellReference r)
        {
            return this.GetCell(r.Row, r.Column);
        }

        /// <summary>
        /// Gets the cell from the specified row and column.
        /// </summary>
        /// <param name="row">
        /// The row (0-based).
        /// </param>
        /// <param name="column">
        /// The column (0-based).
        /// </param>
        /// <returns>
        /// The <see cref="Cell"/>.
        /// </returns>
        public Cell GetCell(int row, int column)
        {
            var cell = this.Cells.FirstOrDefault(c => c.Row == row && c.Column == column);
            if (cell == null)
            {
                cell = new Cell(row, column);
                this.Cells.Add(cell);
            }

            return cell;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>A sequence of <see cref="Cell" />.</returns>
        public IEnumerator<Cell> GetEnumerator()
        {
            return this.Cells.GetEnumerator();
        }

        /// <summary>
        /// Merges the cells in the specified range.
        /// </summary>
        /// <param name="range">
        /// The range (e.g. "A1:B1").
        /// </param>
        public void Merge(string range)
        {
            this.MergeCells.Add(CellRange.Parse(range));
        }

        /// <summary>
        /// Sets the formula in the specified cell.
        /// </summary>
        /// <param name="row">
        /// The row (0-based).
        /// </param>
        /// <param name="column">
        /// The column (0-based).
        /// </param>
        /// <param name="formula">
        /// The formula.
        /// </param>
        public void SetFormula(int row, int column, string formula)
        {
            this.GetCell(row, column).Formula = formula;
        }

        /// <summary>
        /// Sets the formula in the specified cell.
        /// </summary>
        /// <param name="cellReference">
        /// The cell reference.
        /// </param>
        /// <param name="formula">
        /// The formula.
        /// </param>
        public void SetFormula(string cellReference, string formula)
        {
            this.GetCell(cellReference).Formula = formula;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>A <see cref="System.Collections.IEnumerator" />.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets the cells in the specified range.
        /// </summary>
        /// <param name="rangeReference">
        /// The range (e.g. "A1:B2" or "A1").
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Cell"/>s in the specified range.
        /// </returns>
        private IEnumerable<Cell> GetCells(string rangeReference)
        {
            var range = CellRange.Parse(rangeReference);
            for (int i = range.Start.Row; i <= range.End.Row; i++)
            {
                for (int j = range.Start.Column; j <= range.End.Column; j++)
                {
                    yield return this.GetCell(i, j);
                }
            }
        }

        public void AutoSizeColumns()
        {
            var maxColumn = this.Cells.Max(c => c.Column);
            while (this.Columns.Count <= maxColumn)
            {
                this.AddColumn();
            }

            for (int i = 0; i < this.Columns.Count; i++)
            {
                var maxLength = this.Cells.Where(c => c.Column == i).Max(c => c.ToString().Length);
                this.Columns[i].Width = maxLength;
            }
        }
    }
}