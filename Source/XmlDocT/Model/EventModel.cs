// --------------------------------------------------------------------------------------------------------------------
// <copyright file="EventModel.cs" company="Lynx">
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

    public class EventModel : MemberModel
    {
        public EventModel(TypeModel parent, EventInfo ei)
            : base(parent, ei)
        {
        }

        public EventInfo EventInfo
        {
            get
            {
                return (EventInfo)this.Info;
            }
        }

        public override string GetXmlMemberName()
        {
            return string.Format("E:{0}.{1}", XmlUtilities.GetXmlMemberTypeName(this.Info.DeclaringType), this.Info.Name);
        }

        public override IEnumerable<Type> GetRelatedTypes()
        {
            yield return this.Info.DeclaringType;
            var mi = (EventInfo)this.Info;
            yield return mi.EventHandlerType;
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            var ei = this.EventInfo;

            sb.Append("event ");
            XmlUtilities.AppendAttributes(ei.GetCustomAttributes(false), sb);
            sb.Append(XmlUtilities.GetNiceTypeName(ei.EventHandlerType));
            sb.Append(" ");
            sb.Append(ei.Name);
            return sb.ToString();
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Event", XmlUtilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override bool IsInherited()
        {
            return this.Info == null || this.Info.DeclaringType != this.Parent.Type;
        }

        public override string ToString()
        {
            return this.Info.Name;
        }

        protected override string GetFileNameCore()
        {
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this.Name);
        }
    }
}