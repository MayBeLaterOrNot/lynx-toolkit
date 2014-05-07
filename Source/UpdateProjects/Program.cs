// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx Toolkit">
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
//   The UpdateProjects program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UpdateProjects
{
    using System;
    using System.IO;
    using System.Xml;

    using LynxToolkit;

    /// <summary>
    /// The UpdateProjects program.
    /// </summary>
    public class Program
    {
        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        /// <param name="args">The arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);

            var source = Directory.GetCurrentDirectory();

            if (args.Length > 0)
            {
                source = Path.GetFullPath(args[0]);
            }

            foreach (var project in Utilities.FindFiles(source, "*.csproj"))
            {
                Process(project);
            }
        }

        /// <summary>
        /// Processes the project file at the specified path.
        /// </summary>
        /// <param name="projectPath">The path to the project file.</param>
        private static void Process(string projectPath)
        {
            var doc = new XmlDocument();
            doc.Load(projectPath);

            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            var msbuildNamespace = "http://schemas.microsoft.com/developer/msbuild/2003";
            nsmgr.AddNamespace("b", msbuildNamespace);

            var root = doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            var modified = false;

            foreach (XmlNode propertyGroupNode in root.SelectNodes("//b:PropertyGroup", nsmgr))
            {
                // Find PropertyGroups where Condition attribute is set
                if (propertyGroupNode.Attributes["Condition"] != null)
                {
                    // Set TreatWarningsAsErrors = true
                    var twae = propertyGroupNode.SelectSingleNode("b:TreatWarningsAsErrors", nsmgr);
                    if (twae == null)
                    {
                        // Add the element
                        twae = doc.CreateElement("TreatWarningsAsErrors", msbuildNamespace);
                        propertyGroupNode.AppendChild(twae);
                    }

                    twae.InnerText = "true";
                    modified = true;
                }
            }

            if (modified)
            {
                doc.Save(projectPath);
            }
        }
    }
}
