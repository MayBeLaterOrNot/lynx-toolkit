// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GlobalAssemblyInfo.cs" company="Lynx">
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
//   Provides utility methods for the Lynx toolkit console applications.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("LynxToolkit")]
[assembly: AssemblyCopyright("Copyright (c) LynxToolkit 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("2012.2.1.*")]
[assembly: AssemblyFileVersion("2012.2.1.0")]

namespace LynxToolkit
{
    /// <summary>
    /// Provides utility methods for the Lynx toolkit console applications.
    /// </summary>
    /// <remarks></remarks>
    public static class Application
    {
        /// <summary>
        /// Gets the application header (product name, version number and copyright notice).
        /// </summary>
        public static string Header
        {
            get
            {
                var sb = new System.Text.StringBuilder();
                var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(typeof(Application).Assembly.Location);
                sb.AppendLine(fvi.ProductName);
                sb.AppendFormat("Version {0}.{1} (build {2})", fvi.ProductMajorPart, fvi.ProductMinorPart, fvi.ProductBuildPart);
                sb.AppendLine();
                sb.AppendLine(fvi.LegalCopyright);
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the application description.
        /// </summary>
        /// <remarks></remarks>
        public static string Description
        {
            get
            {
                var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return fvi.FileDescription;
            }
        }

        /// <summary>
        /// Gets the application comments.
        /// </summary>
        /// <remarks></remarks>
        public static string Comments
        {
            get
            {
                var fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly().Location);
                return fvi.Comments;
            }
        }
    }
}