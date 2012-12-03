// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPropertyCodeGeneratorOptions.cs" company="Lynx">
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
//   Provides preferences for the property code generation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System.Collections.Generic;

    /// <summary>
    /// Provides preferences for the property code generation.
    /// </summary>
    public interface IPropertyCodeGeneratorOptions
    {
        /// <summary>
        /// Gets the property change flags dictionary.
        /// </summary>
        /// <value>The property change flags.</value>
        Dictionary<string, string> Flags { get; }

        /// <summary>
        /// Gets the format string for the property setter.
        /// </summary>
        /// <remarks>
        /// Use {0} for the name of the backing field.
        /// Use {1} for the name of the property.
        /// </remarks>
        string PropertySetter { get; }

        /// <summary>
        /// Gets the format string for the reference property setter.
        /// </summary>
        /// <remarks>
        /// Use {0} for the name of the backing field.
        /// Use {1} for the name of the property.
        /// </remarks>
        string ReferencePropertySetter { get; }

        /// <summary>
        /// Gets the reference property type.
        /// </summary>
        string ReferencePropertyType { get; }

        /// <summary>
        /// Gets the format string for the raise property changed statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>
        /// Use {0} for the name of the property.
        /// </remarks>
        string RaisePropertyChanged { get; }

        /// <summary>
        /// Gets the format string for the property change callback statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>
        /// Use {0} for the name of the property.
        /// </remarks>
        string PropertyChangeCallback { get; }

        /// <summary>
        /// Gets the format string for the validation callback statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>
        /// Use {0} for the name of the property.
        /// </remarks>
        string ValidateCallback { get; }

        /// <summary>
        /// Gets the name of the properties file.
        /// </summary>
        /// <value>The name of the properties file.</value>
        string PropertiesFileName { get; }

        /// <summary>
        /// Gets the format string for the reference resolve statement.
        /// </summary>
        string ReferenceResolve { get; }

        /// <summary>
        /// Gets a value indicating whether to validate dependency names.
        /// </summary>
        /// <value><c>true</c> if dependency names should be validated; otherwise, <c>false</c>.</value>
        /// <remarks>Disabling this feature will make it possible to set dependencies to properties that are not auto-generated.</remarks>
        bool ValidateDependencies { get; }
    }
}