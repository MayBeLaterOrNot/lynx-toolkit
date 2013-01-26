// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestObject.Properties.cs" company="Lynx Toolkit">
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
//   Defines the properties for the TestObject class. This file is auto-generated.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG.Tests
{
    using System;

    /// <summary>
    /// Defines the properties for the TestObject class. This file is auto-generated.
    /// </summary>
    public partial class TestObject
    {
        /// <summary>
        /// The backing field for the Guid property.
        /// </summary>
        private Guid guid;

        /// <summary>
        /// The backing field for the Name property.
        /// </summary>
        private string name;

        /// <summary>
        /// The backing field for the Color property.
        /// </summary>
        private Color color;

        /// <summary>
        /// The backing field for the ReferencedObject property.
        /// </summary>
        private Reference<TestObject> referencedObject;

        /// <summary>
        /// Gets or sets the guid.
        /// </summary>
        public Guid Guid
        {
            get
            {
                return this.guid;
            }

            set
            {
                this.SetValue(ref this.guid, value, "Guid");
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.SetValue(ref this.name, value, "Name");
            }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color Color
        {
            get
            {
                return this.color;
            }

            set
            {
                this.SetValue(ref this.color, value, "Color");
            }
        }

        /// <summary>
        /// Gets or sets the referenced object.
        /// </summary>
        public Reference<TestObject> ReferencedObject
        {
            get
            {
                this.ResolveReference(ref this.referencedObject);
                return this.referencedObject;
            }

            set
            {
                this.SetReference(ref this.referencedObject, value, "ReferencedObject");
            }
        }
    }
}