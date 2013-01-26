// --------------------------------------------------------------------------------------------------------------------
// <copyright file="StringBuilderEx.cs" company="Lynx Toolkit">
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
namespace PropertyCG
{
    using System.Text;

    public class StringBuilderEx
    {
        private StringBuilder sb;

        public StringBuilderEx(int capacity = 1000)
        {
            this.sb = new StringBuilder(capacity);
        }

        private string indent = "";
        public void Indent()
        {
            this.indent += "    ";
        }
        public void Unindent()
        {
            this.indent = this.indent.Substring(4);
        }

        public void Append(object s)
        {
            this.sb.Append(s);
        }

        public void AppendFormat(string format, params object[] args)
        {
            this.sb.AppendFormat(format, args);
        }

        public void AppendLine(string format = null, params object[] args)
        {
            if (format != null)
            {
                this.sb.Append(this.indent);
                if (args.Length > 0)
                    this.sb.AppendFormat(format, args);
                else this.sb.Append(format);
            }
            this.sb.AppendLine();
        }
        public override string ToString()
        {
            return this.sb.ToString();
        }
    }
}