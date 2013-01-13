// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ConstructorModel.cs" company="Lynx">
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
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;

    public class ConstructorModel : MemberModel
    {
        public ConstructorModel(TypeModel parent, ConstructorInfo ci)
            : base(parent, ci)
        {
            this.Parameters = new List<ParameterModel>();
        }

        public ConstructorInfo ConstructorInfo
        {
            get
            {
                return (ConstructorInfo)this.Info;
            }
        }

        public IList<ParameterModel> Parameters { get; private set; }

        public override string GetXmlMemberName()
        {
            var memberName = "M:" + XmlUtilities.GetXmlMemberTypeName(this.Info.DeclaringType) + ".#ctor";
            var memberParameters = string.Format("({0})", XmlUtilities.GetXmlParameterList(this.ConstructorInfo, null));
            return memberName + memberParameters;
        }

        public override IEnumerable<ParameterModel> GetParameters()
        {
            return this.Parameters;
        }

        public override IEnumerable<Type> GetRelatedTypes()
        {
            yield return this.Info.DeclaringType;
            foreach (var p in this.ConstructorInfo.GetParameters())
            {
                yield return p.ParameterType;
            }
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            var ci = this.ConstructorInfo;
            if (ci.IsPublic)
            {
                sb.Append("public ");
            }

            sb.Append(ci.DeclaringType.Name);
            sb.Append("(");
            sb.Append(XmlUtilities.GetNiceMethodParameters(ci, true));
            sb.Append(")");
            return sb.ToString();
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Constructor", XmlUtilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override string ToString()
        {
            return string.Format(
                "{0}({1})",
                XmlUtilities.GetNiceMethodName(this.ConstructorInfo),
                XmlUtilities.GetNiceMethodParameters(this.ConstructorInfo));
        }

        protected override string GetFileNameCore()
        {
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this);
        }
    }
}