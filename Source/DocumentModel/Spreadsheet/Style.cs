// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Style.cs" company="Lynx">
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
//   Represents a cell format in a spreadsheet.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace LynxToolkit.Documents.Spreadsheet
{
    /// <summary>
    /// Represents a cell format in a spreadsheet.
    /// </summary>
    public class Style
    {
        /// <summary>
        /// Gets or sets the background.
        /// </summary>
        /// <value>The background.</value>
        public uint? Background { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the text should be strong.
        /// </summary>
        /// <value>The bold.</value>
        public bool Bold { get; set; }

        /// <summary>
        /// Gets or sets the color of the bottom border.
        /// </summary>
        /// <value>The color of the bottom border.</value>
        public uint BottomBorderColor { get; set; }

        /// <summary>
        /// Gets or sets the bottom border style.
        /// </summary>
        /// <value>The bottom border style.</value>
        public BorderStyle BottomBorderStyle { get; set; }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        /// <value>The name of the font.</value>
        public string FontName { get; set; }

        /// <summary>
        /// Gets or sets the size of the font.
        /// </summary>
        /// <value>The size of the font.</value>
        public double FontSize { get; set; }

        /// <summary>
        /// Gets or sets the foreground.
        /// </summary>
        /// <value>The foreground.</value>
        public uint? Foreground { get; set; }

        /// <summary>
        /// Gets or sets the horizontal alignment.
        /// </summary>
        /// <value>The horizontal alignment.</value>
        public HorizontalAlignment HorizontalAlignment { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the text should be emphasized.
        /// </summary>
        /// <value>The italic.</value>
        public bool Italic { get; set; }

        /// <summary>
        /// Gets or sets the color of the left border.
        /// </summary>
        /// <value>The color of the left border.</value>
        public uint LeftBorderColor { get; set; }

        /// <summary>
        /// Gets or sets the left border style.
        /// </summary>
        /// <value>The left border style.</value>
        public BorderStyle LeftBorderStyle { get; set; }

        /// <summary>
        /// Gets or sets the number format.
        /// </summary>
        /// <value>The number format.</value>
        public string NumberFormat { get; set; }

        /// <summary>
        /// Gets or sets the color of the right border.
        /// </summary>
        /// <value>The color of the right border.</value>
        public uint RightBorderColor { get; set; }

        /// <summary>
        /// Gets or sets the right border style.
        /// </summary>
        /// <value>The right border style.</value>
        public BorderStyle RightBorderStyle { get; set; }

        /// <summary>
        /// Gets or sets the color of the top border.
        /// </summary>
        /// <value>The color of the top border.</value>
        public uint TopBorderColor { get; set; }

        /// <summary>
        /// Gets or sets the top border style.
        /// </summary>
        /// <value>The top border style.</value>
        public BorderStyle TopBorderStyle { get; set; }

        /// <summary>
        /// Gets or sets the vertical alignment.
        /// </summary>
        /// <value>The vertical alignment.</value>
        public VerticalAlignment VerticalAlignment { get; set; }

        /// <summary>
        /// Determines whether this style defines a border.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool HasBorder()
        {
            return this.LeftBorderStyle != BorderStyle.None || this.RightBorderStyle != BorderStyle.None
                   || this.TopBorderStyle != BorderStyle.None || this.BottomBorderStyle != BorderStyle.None;
        }

        /// <summary>
        /// Sets the color of the border.
        /// </summary>
        /// <param name="color">
        /// The color.
        /// </param>
        public void SetBorderColor(uint color)
        {
            this.LeftBorderColor = color;
            this.RightBorderColor = color;
            this.TopBorderColor = color;
            this.BottomBorderColor = color;
        }

        /// <summary>
        /// Sets the border style.
        /// </summary>
        /// <param name="allBorders">
        /// All borders.
        /// </param>
        public void SetBorderStyle(BorderStyle allBorders)
        {
            this.LeftBorderStyle = allBorders;
            this.RightBorderStyle = allBorders;
            this.TopBorderStyle = allBorders;
            this.BottomBorderStyle = allBorders;
        }

        /// <summary>
        /// Sets the border style.
        /// </summary>
        /// <param name="leftStyle">
        /// The left style.
        /// </param>
        /// <param name="topStyle">
        /// The top style.
        /// </param>
        /// <param name="rightStyle">
        /// The right style.
        /// </param>
        /// <param name="bottomStyle">
        /// The bottom style.
        /// </param>
        public void SetBorderStyle(
            BorderStyle leftStyle, BorderStyle topStyle, BorderStyle rightStyle, BorderStyle bottomStyle)
        {
            this.LeftBorderStyle = leftStyle;
            this.RightBorderStyle = rightStyle;
            this.TopBorderStyle = topStyle;
            this.BottomBorderStyle = bottomStyle;
        }
    }
}