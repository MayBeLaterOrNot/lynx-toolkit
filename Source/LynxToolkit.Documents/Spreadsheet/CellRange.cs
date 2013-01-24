// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CellRange.cs" company="Lynx">
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
//   Represents a spreadsheet cell range.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents.Spreadsheet
{
    /// <summary>
    /// Represents a spreadsheet cell range.
    /// </summary>
    public struct CellRange
    {
        /// <summary>
        /// Gets or sets the end cell.
        /// </summary>
        /// <value>The end.</value>
        public CellReference End { get; set; }

        /// <summary>
        /// Gets or sets the start cell.
        /// </summary>
        /// <value>The start.</value>
        public CellReference Start { get; set; }

        /// <summary>
        /// Parses the specified range.
        /// </summary>
        /// <param name="range">
        /// The range.
        /// </param>
        /// <returns>
        /// A <see cref="CellRange"/>.
        /// </returns>
        public static CellRange Parse(string range)
        {
            if (range.Contains(":"))
            {

                var f = range.Split(':');
                return new CellRange { Start = CellReference.Parse(f[0]), End = CellReference.Parse(f[1]) };
            }

            var cell = CellReference.Parse(range);
            return new CellRange { Start = cell, End = cell };
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("{0}:{1}", this.Start, this.End);
        }
    }
}