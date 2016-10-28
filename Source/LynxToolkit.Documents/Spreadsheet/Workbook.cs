// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Workbook.cs" company="Lynx Toolkit">
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
//   Represents a spreadsheet workbook.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace LynxToolkit.Documents.Spreadsheet
{
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a spreadsheet workbook.
    /// </summary>
    public class Workbook : IEnumerable<Worksheet>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Workbook"/> class.
        /// </summary>
        public Workbook()
        {
            this.Sheets = new List<Worksheet>();
            this.Styles = new List<Style>();
            this.AddStyle();
        }

        /// <summary>
        /// Gets or sets the creator.
        /// </summary>
        /// <value>The creator.</value>
        public string Creator { get; set; }

        /// <summary>
        /// Gets or sets the keywords.
        /// </summary>
        /// <value>The keywords.</value>
        public string Keywords { get; set; }

        /// <summary>
        /// Gets the sheets.
        /// </summary>
        /// <value>The sheets.</value>
        public List<Worksheet> Sheets { get; }

        /// <summary>
        /// Gets the styles.
        /// </summary>
        /// <value>The styles.</value>
        public List<Style> Styles { get; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>The title.</value>
        public string Title { get; set; }

        /// <summary>
        /// Adds a sheet.
        /// </summary>
        /// <param name="name">
        /// The name of the sheet.
        /// </param>
        /// <returns>
        /// A <see cref="Worksheet"/>.
        /// </returns>
        public Worksheet AddSheet(string name = null)
        {
            var sheet = new Worksheet(name ?? string.Format("Sheet{0}", this.Sheets.Count + 1));
            this.Sheets.Add(sheet);
            return sheet;
        }

        /// <summary>
        /// Adds a style.
        /// </summary>
        /// <param name="horizontalAlignment">The horizontal alignment.</param>
        /// <param name="verticalAlignment">The vertical alignment.</param>
        /// <param name="bold">if set to <c>true</c> [bold].</param>
        /// <param name="italic">if set to <c>true</c> [italic].</param>
        /// <param name="fontSize">Size of the font.</param>
        /// <param name="fontName">Name of the font.</param>
        /// <param name="foreground">The foreground.</param>
        /// <param name="background">The background.</param>
        /// <param name="textRotation">The text rotation in degrees.</param>
        /// <returns>
        /// A XStyle.
        /// </returns>
        public Style AddStyle(
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Auto,
            VerticalAlignment verticalAlignment = VerticalAlignment.Auto,
            bool bold = false,
            bool italic = false,
            double fontSize = 11,
            string fontName = "Calibri",
            uint? foreground = null,
            uint? background = null,
            int textRotation = 0)
        {
            var style = new Style
            {
                HorizontalAlignment = horizontalAlignment,
                VerticalAlignment = verticalAlignment,
                Bold = bold,
                Italic = italic,
                FontSize = fontSize,
                FontName = fontName,
                Foreground = foreground,
                Background = background,
                TextRotation = textRotation
            };
            this.Styles.Add(style);
            return style;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>An enumerator of <see cref="Worksheet"/>. </returns>
        public IEnumerator<Worksheet> GetEnumerator()
        {
            return this.Sheets.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>A <see cref="System.Collections.IEnumerator"/>.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}