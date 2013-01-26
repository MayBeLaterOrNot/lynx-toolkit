// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Model.cs" company="Lynx Toolkit">
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

    public abstract class Model
    {
        public string Description { get; set; }

        public string Example { get; set; }

        public string Remarks { get; set; }

        public virtual string GetDescription()
        {
            return this.Description;
        }

        public string GetFileName()
        {
            var fileName = this.GetFileNameCore();
            return fileName.Replace('<', '~').Replace('>', '~').Replace("()", string.Empty).Replace(" ", string.Empty);
        }

        public abstract string GetXmlMemberName();

        public virtual string GetPageTitle()
        {
            return this.GetTitle();
        }

        public virtual IEnumerable<Type> GetRelatedTypes()
        {
            yield break;
        }

        public virtual string GetSyntax()
        {
            return null;
        }

        public virtual string GetTitle()
        {
            return null;
        }

        public virtual bool IsInherited()
        {
            return true;
        }

        protected virtual string GetFileNameCore()
        {
            return null;
        }
    }
}