// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx Toolkit">
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
// --------------------------------------------------------------------------------------------------------------------
namespace WikiTable
{
    using System;
    using System.Text;
    using System.Windows.Forms;

    class Program
    {
        [STAThread]
        private static int Main()
        {
            Console.WriteLine(LynxToolkit.Utilities.ApplicationHeader);
            var input = Clipboard.GetText();
            if (string.IsNullOrWhiteSpace(input))
            {
                Console.WriteLine("No text on clipboard.");
                return 1;
            }

            var output = Convert(input);
            Clipboard.SetText(output);
            return 0;
        }

        private static string Convert(string text, bool includeHeader = true)
        {
            var sb = new StringBuilder();
            foreach (var l in text.Split('\r'))
            {
                var line = l.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                var fields = line.Split('\t');
                sb.Append(includeHeader ? "||" : "|");
                foreach (var field in fields)
                {
                    sb.Append(" ");
                    sb.Append(field.Trim());
                    sb.Append(includeHeader ? " ||" : " |");
                }

                sb.AppendLine();
                includeHeader = false;
            }

            return sb.ToString();
        }
    }
}