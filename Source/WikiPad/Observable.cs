// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Observable.cs" company="Lynx Toolkit">
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
//   Provides a base class that implements the INotifyPropertyChanged interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace WikiPad
{
    using System.ComponentModel;

    /// <summary>
    /// Provides a base class that implements the INotifyPropertyChanged interface.
    /// </summary>
    public abstract class Observable : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <typeparam name="T">The property type.</typeparam>
        /// <param name="property">The property.</param>
        /// <param name="value">The value.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>True if the property was changed.</returns>
        protected bool SetValue<T>(ref T property, T value, string propertyName)
        {
            if (Equals(property, value))
            {
                return false;
            }

            property = value;

            this.OnPropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Called when a property was changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}