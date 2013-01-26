// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MethodModel.cs" company="Lynx Toolkit">
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
    using System.Linq;
    using System.Reflection;
    using System.Text;

    public class MethodModel : MemberModel
    {
        public MethodModel(TypeModel parent, MethodInfo mi)
            : base(parent, mi)
        {
            this.Parameters = new List<ParameterModel>();
        }

        public MethodInfo MethodInfo
        {
            get
            {
                return (MethodInfo)this.Info;
            }
        }

        public IList<ParameterModel> Parameters { get; private set; }

        public override IEnumerable<ParameterModel> GetParameters()
        {
            return this.Parameters;
        }

        public Type ReturnType
        {
            get
            {
                return ((MethodInfo)this.Info).ReturnType;
            }
        }

        public string ReturnValueDescription { get; set; }

        public override string GetXmlMemberName()
        {
            var memberName = string.Format(
                "M:{0}.{1}", XmlUtilities.GetXmlMemberTypeName(this.Info.DeclaringType), this.Info.Name);

            IEnumerable<string> methodGenericArgs = null;
            if (this.MethodInfo.IsGenericMethod)
            {
                memberName += "``" + this.MethodInfo.GetGenericArguments().Length;
                methodGenericArgs = this.MethodInfo.GetGenericArguments().Select(t => t.Name);
            }

            var memberParameters = string.Format(
                "({0})", XmlUtilities.GetXmlParameterList(this.MethodInfo, methodGenericArgs));
            return memberName + memberParameters;
        }

        public override IEnumerable<Type> GetRelatedTypes()
        {
            yield return this.Info.DeclaringType;
            var mi = (MethodInfo)this.Info;
            yield return mi.ReturnType;
            foreach (var p in mi.GetParameters())
            {
                yield return p.ParameterType;
            }
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();

            var mb = this.MethodInfo;
            XmlUtilities.AppendAttributes(mb.GetCustomAttributes(false), sb);
            if (mb.IsPublic)
            {
                sb.Append("public ");
            }

            if (mb.IsPrivate)
            {
                sb.Append("private ");
            }

            if (mb.IsVirtual)
            {
                sb.Append("virtual ");
            }

            sb.Append(XmlUtilities.GetNiceTypeName(mb.ReturnType));
            sb.Append(" ");

            sb.Append(mb.Name);
            sb.Append("(");
            sb.Append(XmlUtilities.GetNiceMethodParameters(mb, true));
            sb.Append(")");
            return sb.ToString();
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Method", XmlUtilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override bool IsInherited()
        {
            return this.Info == null || this.Info.DeclaringType != this.Parent.Type;
        }

        public override string ToString()
        {
            if (!this.IsOverloaded) return XmlUtilities.GetNiceMethodName(this.MethodInfo);
            return string.Format(
                "{0}({1})",
                XmlUtilities.GetNiceMethodName(this.MethodInfo),
                XmlUtilities.GetNiceMethodParameters(this.MethodInfo));
        }

        protected override string GetFileNameCore()
        {
            //if (this.IsOverloaded)
            //    return string.Format("{0}.{1}({2})", this.DeclaringType.FullName, this.Name);
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this);
        }
    }
}