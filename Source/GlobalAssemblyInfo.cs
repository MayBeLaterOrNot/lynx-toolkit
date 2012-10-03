//-----------------------------------------------------------------------
// <copyright file="AssemblyInfo.cs" company="Lynx">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("LynxToolkit")]
[assembly: AssemblyCopyright("Copyright (c) LynxToolkit 2012")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("2012.1.1.*")]
[assembly: AssemblyFileVersion("2012.1.1.0")]

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