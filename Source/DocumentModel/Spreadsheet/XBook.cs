// --------------------------------------------------------------------------------------------------------------------
// <copyright file="XBook.cs" company="Lynx">
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
    using System.Collections.Generic;

    /// <summary>
    /// Represents a spreadsheet workbook.
    /// </summary>
    public class XBook
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="XBook"/> class.
        /// </summary>
        public XBook()
        {
            this.Sheets = new List<XSheet>();
            this.Styles = new List<XStyle>();
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
        public List<XSheet> Sheets { get; private set; }

        /// <summary>
        /// Gets the styles.
        /// </summary>
        /// <value>The styles.</value>
        public List<XStyle> Styles { get; private set; }

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
        /// A XSheet.
        /// </returns>
        public XSheet AddSheet(string name)
        {
            var sheet = new XSheet(name);
            this.Sheets.Add(sheet);
            return sheet;
        }

        /// <summary>
        /// Adds a style.
        /// </summary>
        /// <param name="horizontalAlignment">
        /// The horizontal alignment.
        /// </param>
        /// <param name="verticalAlignment">
        /// The vertical alignment.
        /// </param>
        /// <param name="bold">
        /// if set to <c>true</c> [bold].
        /// </param>
        /// <param name="italic">
        /// if set to <c>true</c> [italic].
        /// </param>
        /// <param name="fontSize">
        /// Size of the font.
        /// </param>
        /// <param name="fontName">
        /// Name of the font.
        /// </param>
        /// <param name="foreground">
        /// The foreground.
        /// </param>
        /// <param name="background">
        /// The background.
        /// </param>
        /// <returns>
        /// A XStyle.
        /// </returns>
        public XStyle AddStyle(
            HorizontalAlignment horizontalAlignment = HorizontalAlignment.Auto, 
            VerticalAlignment verticalAlignment = VerticalAlignment.Auto, 
            bool bold = false, 
            bool italic = false, 
            double fontSize = 11, 
            string fontName = "Calibri", 
            uint? foreground = null, 
            uint? background = null)
        {
            var style = new XStyle
                            {
                                HorizontalAlignment = horizontalAlignment, 
                                VerticalAlignment = verticalAlignment, 
                                Bold = bold, 
                                Italic = italic, 
                                FontSize = fontSize, 
                                FontName = fontName, 
                                Foreground = foreground, 
                                Background = background
                            };
            this.Styles.Add(style);
            return style;
        }
    }
}