// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FieldModel.cs" company="Lynx">
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

namespace XmlDocT
{
    using System.Reflection;
    using System.Text;

    public class FieldModel : MemberModel
    {
        public FieldModel(TypeModel parent, FieldInfo pi)
            : base(parent, pi)
        {
        }

        public FieldInfo FieldInfo
        {
            get
            {
                return (FieldInfo)this.Info;
            }
        }

        public override string GetXmlMemberName()
        {
            return string.Format("F:{0}.{1}", Utilities.GetXmlMemberTypeName(this.Info.DeclaringType), this.Info.Name);
        }

        public override string GetPageTitle()
        {
            return string.Format(
                "{0}.{1} ({2})", Utilities.GetNiceTypeName(this.DeclaringType), this, this.DeclaringType.Namespace);
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            var pi = (FieldInfo)this.Info;

            Utilities.AppendAttributes(pi.GetCustomAttributes(false), sb);

            sb.Append("public ");
            sb.Append(Utilities.GetNiceTypeName(pi.FieldType));
            sb.Append(" ");
            sb.Append(pi.Name);
            return sb.ToString();
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Field", Utilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override string ToString()
        {
            return this.Name;
        }

        protected override string GetFileNameCore()
        {
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this.Name);
        }
    }
}