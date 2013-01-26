// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyModel.cs" company="Lynx Toolkit">
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

    public class PropertyModel : MemberModel
    {
        public PropertyModel(TypeModel parent, PropertyInfo pi)
            : base(parent, pi)
        {
        }

        public PropertyInfo PropertyInfo
        {
            get
            {
                return (PropertyInfo)this.Info;
            }
        }

        public override string GetXmlMemberName()
        {
            return string.Format("P:{0}.{1}", XmlUtilities.GetXmlMemberTypeName(this.Info.DeclaringType), this.Info.Name);
        }

        public override string GetPageTitle()
        {
            return string.Format(
                "{0}.{1} ({2})", XmlUtilities.GetNiceTypeName(this.DeclaringType), this, this.DeclaringType.Namespace);
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            var pi = (PropertyInfo)this.Info;

            XmlUtilities.AppendAttributes(pi.GetCustomAttributes(false), sb);

            // pi.GetGetMethod().IsPublic
            // pi.GetSetMethod().IsPublic
            sb.Append("public ");
            sb.Append(XmlUtilities.GetNiceTypeName(pi.PropertyType));
            sb.Append(" ");
            sb.Append(pi.Name);
            sb.Append(" { ");
            if (pi.CanRead)
            {
                sb.Append("get; ");
            }

            if (pi.CanWrite)
            {
                sb.Append("set; ");
            }

            sb.Append("}");
            return sb.ToString();
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Property", XmlUtilities.GetNiceTypeName(this.DeclaringType), this);
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