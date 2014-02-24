// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Property.cs" company="Lynx Toolkit">
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
//   The property.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    /// <summary>
    /// The property.
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Property"/> class.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="name">
        /// The name.
        /// </param>
        /// <param name="attributes">
        /// The attributes.
        /// </param>
        /// <param name="description">
        /// The description.
        /// </param>
        public Property(string type, string name, IEnumerable<string> attributes, string description)
        {
            this.Type = type;
            this.Name = name;
            this.DataMember = true;
            this.Description = description;
            this.Dependencies = new List<string>();
            this.Attributes = new List<string>(attributes);
        }

        /// <summary>
        /// Gets the attributes.
        /// </summary>
        /// <value>The attributes.</value>
        public IList<string> Attributes { get; private set; }

        /// <summary>
        /// Gets the name of the backing field.
        /// </summary>
        /// <value>The name of the backing field.</value>
        public string BackingFieldName
        {
            get
            {
                return this.Name[0].ToString(CultureInfo.InvariantCulture).ToLower() + this.Name.Substring(1);
            }
        }

        /// <summary>
        /// Gets the dependencies.
        /// </summary>
        /// <value>The dependencies.</value>
        public IList<string> Dependencies { get; private set; }

        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is reference.
        /// </summary>
        /// <value><c>true</c> if this instance is reference; otherwise, <c>false</c>.</value>
        public bool IsReference { get; set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a property change callback should be included.
        /// </summary>
        public bool PropertyChangeCallback { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is read only.
        /// </summary>
        /// <value><c>true</c> if the property is read only; otherwise, <c>false</c>.</value>
        public bool ReadOnly { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the property is a data member.
        /// </summary>
        /// <value><c>true</c> if the property is a data member; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// Properties that are not data members will not be included in serialization.
        /// </remarks>
        public bool DataMember { get; set; }

        /// <summary>
        /// Gets or sets the property changed flags.
        /// </summary>
        /// <value>The property changed flags.</value>
        public string PropertyChangedFlags { get; set; }

        /// <summary>
        /// Gets the summary description.
        /// </summary>
        /// <value>The summary.</value>
        public string Summary
        {
            get
            {
                string description = string.IsNullOrEmpty(this.Description)
                                         ? this.GetDescription(this.Name)
                                         : this.Description;
                string longDescription;
                if (this.Type == "bool")
                {
                    longDescription = "a value indicating whether the " + description + " should be on or off";
                }
                else
                {
                    longDescription = "the " + description;
                }

                return string.Format(this.ReadOnly ? "Gets {0}." : "Gets or sets {0}.", longDescription);
            }
        }

        /// <summary>
        /// Gets the type.
        /// </summary>
        /// <value>The type.</value>
        public string Type { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether a validate callback should be included.
        /// </summary>
        public bool ValidateCallback { get; set; }

        /// <summary>
        /// Gets the descriptive name of a property.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property.
        /// </param>
        /// <returns>
        /// The description.
        /// </returns>
        private string GetDescription(string propertyName)
        {
            var result = new StringBuilder();
            for (int i = 0; i < propertyName.Length; i++)
            {
                if (i == 0)
                {
                    result.Append(this.ToLower(propertyName[i]));
                    continue;
                }

                if (this.IsLower(propertyName[i - 1]) && !this.IsLower(propertyName[i])
                    && (i + 1 < propertyName.Length && this.IsLower(propertyName[i + 1])))
                {
                    result.Append(" ");
                    result.Append(this.ToLower(propertyName[i]));
                    continue;
                }

                result.Append(propertyName[i]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Determines whether the specified character is lowercase.
        /// </summary>
        /// <param name="p0">
        /// The input character.
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified character is lowercase; otherwise, <c>false</c> .
        /// </returns>
        private bool IsLower(char p0)
        {
            return p0.ToString(CultureInfo.InvariantCulture).ToLower()[0] == p0;
        }

        /// <summary>
        /// Converts a character to lower case.
        /// </summary>
        /// <param name="p0">
        /// The input character.
        /// </param>
        /// <returns>
        /// The converted character.
        /// </returns>
        private char ToLower(char p0)
        {
            return p0.ToString(CultureInfo.InvariantCulture).ToLower()[0];
        }
    }
}