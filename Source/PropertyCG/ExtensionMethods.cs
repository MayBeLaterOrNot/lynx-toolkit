// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="Lynx">
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
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Provides various extension methods.
    /// </summary>
    public static class ExtensionMethods
    {
        /// <summary>
        /// Formats the list of items to a string with the specified separator.
        /// </summary>
        /// <param name="items">The items.</param>
        /// <param name="separator">The separator.</param>
        /// <returns>The System.String.</returns>
        public static string FormatList(this IEnumerable<object> items, string separator)
        {
            var sb = new StringBuilder();
            foreach (var item in items)
            {
                if (sb.Length > 0)
                {
                    sb.Append(separator);
                }
                sb.Append(item);
            }

            return sb.ToString();
        }
    }
}