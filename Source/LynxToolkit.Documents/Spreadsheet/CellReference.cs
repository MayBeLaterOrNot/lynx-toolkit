// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CellReference.cs" company="Lynx">
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
//   Represents a spreadsheet cell reference.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents.Spreadsheet
{
    /// <summary>
    /// Represents a spreadsheet cell reference.
    /// </summary>
    public struct CellReference
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellReference"/> struct.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <param name="column">
        /// The column.
        /// </param>
        public CellReference(int row, int column)
            : this()
        {
            this.Row = row;
            this.Column = column;
        }

        /// <summary>
        /// Gets the column.
        /// </summary>
        /// <value>The column.</value>
        public int Column { get; private set; }

        /// <summary>
        /// Gets the row.
        /// </summary>
        /// <value>The row.</value>
        public int Row { get; private set; }

        /// <summary>
        /// Formats the specified row and column to a cell reference string.
        /// </summary>
        /// <param name="row">
        /// The row.
        /// </param>
        /// <param name="column">
        /// The column.
        /// </param>
        /// <returns>
        /// A System.String.
        /// </returns>
        public static string Format(int row, int column)
        {
            var c0 = column / 26;
            var c1 = column % 26;
            var b0 = (char)((byte)'A' + c0 - 1);
            var b1 = (char)((byte)'A' + c1);
            return column >= 26 ? string.Format("{0}{1}{2}", b0, b1, row + 1) : string.Format("{0}{1}", b1, row + 1);
        }

        /// <summary>
        /// Parses the specified cell reference.
        /// </summary>
        /// <param name="cellReference">
        /// The cell reference.
        /// </param>
        /// <returns>
        /// A <see cref="CellReference"/>.
        /// </returns>
        public static CellReference Parse(string cellReference)
        {
            if (char.IsDigit(cellReference[1]))
            {
                int column = cellReference[0] - 'A';
                int row = int.Parse(cellReference.Substring(1)) - 1;
                return new CellReference(row, column);
            }
            else
            {
                int c0 = cellReference[0] - 'A';
                int c1 = cellReference[1] - 'A';
                int column = ((c0 + 1) * 26) + c1;
                int row = int.Parse(cellReference.Substring(2)) - 1;
                return new CellReference(row, column);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Format(this.Row, this.Column);
        }
    }
}