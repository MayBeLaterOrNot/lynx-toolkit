// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProjectUpdater.cs" company="Lynx Toolkit">
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
//   Updates C# project files
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml;

    /// <summary>
    /// Updates C# project files
    /// </summary>

    public class ProjectUpdater
    {
        public string Extension { get; set; }
        public string OpenForEditExecutable { get; set; }
        public string OpenForEditArguments { get; set; }
        public string FileName { get; private set; }

        public ProjectUpdater(string fileName, string extension)
        {
            this.FileName = fileName;
            this.Extension = extension;
        }

        public bool Update()
        {
            var doc = new XmlDocument();
            doc.Load(FileName);
            var root = doc.DocumentElement;
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            const string NamespaceUri = "http://schemas.microsoft.com/developer/msbuild/2003";
            nsmgr.AddNamespace("b", NamespaceUri);
            bool modified = false;

            Func<XmlNode, string, bool> changeInnerText = (node, newValue) =>
                {
                    var oldValue = node.InnerText;
                    node.InnerText = newValue;
                    return !string.Equals(oldValue, newValue);
                };

            foreach (XmlNode itemGroup in root.SelectNodes("//b:ItemGroup", nsmgr))
            {
                var propertyFiles = new List<string>();
                foreach (XmlNode item in itemGroup.ChildNodes)
                {
                    if (item == null)
                    {
                        continue;
                    }

                    var includeAttribute = item.Attributes["Include"];
                    if (includeAttribute == null)
                    {
                        continue;
                    }

                    var include = includeAttribute.Value;

                    if (include.Contains(this.Extension))
                    {
                        if (item.Name != "None")
                        {
                            Console.WriteLine(include + " should have build action = None.");
                        }

                        var dependentUponNode = item.SelectSingleNode("b:DependentUpon", nsmgr);
                        if (dependentUponNode != null)
                        {
                            item.RemoveChild(dependentUponNode);
                            modified = true;
                        }

                        propertyFiles.Add(include);

                        // Make property source file dependen upon main class?
                        //if (dependentUponNode == null)
                        //{
                        //    dependentUponNode = doc.CreateElement("DependentUpon", NamespaceUri);
                        //    item.AppendChild(dependentUponNode);
                        //    modified = true;
                        //}

                        //var dependentUponFileName = Path.GetFileName(include.Replace(this.Extension, ".cs"));
                        //modified |= ChangeInnerText(dependentUponNode, dependentUponFileName);
                    }
                }

                foreach (var omlFile in propertyFiles)
                {
                    var propertiesFile = Path.ChangeExtension(omlFile, ".cs");
                    var item = itemGroup.SelectSingleNode("//b:Compile[@Include='" + propertiesFile + "']", nsmgr);
                    if (item == null)
                    {
                        item = doc.CreateElement("Compile", itemGroup.NamespaceURI);
                        var include = doc.CreateAttribute("Include");
                        include.Value = propertiesFile;
                        item.Attributes.Append(include);
                        itemGroup.AppendChild(item);
                    }
                    else
                    {
                        var include = item.Attributes["Include"].Value;
                        if (item.Name != "Compile")
                        {
                            Console.WriteLine(include + " should have build action = Compile.");
                        }
                    }

                    var dependentUponFileName = omlFile; //  Path.GetFileName(include.Replace(".Properties.cs", this.Extension));
                    var dependentUponNode = item.SelectSingleNode("b:DependentUpon", nsmgr);
                    if (dependentUponNode == null)
                    {
                        dependentUponNode = doc.CreateElement("DependentUpon", NamespaceUri);
                        item.AppendChild(dependentUponNode);
                        modified = true;
                    }

                    modified |= changeInnerText(dependentUponNode, dependentUponFileName);

                    var autogenNode = item.SelectSingleNode("b:AutoGen", nsmgr);
                    if (autogenNode == null)
                    {
                        autogenNode = doc.CreateElement("AutoGen", NamespaceUri);
                        item.AppendChild(autogenNode);
                        modified = true;
                    }

                    modified |= changeInnerText(autogenNode, "True");

                }
            }

            if (modified)
            {
                PropertyCodeGenerator.OpenForEdit(this.FileName, this.OpenForEditExecutable, this.OpenForEditArguments);
                doc.Save(this.FileName);
            }

            return modified;
        }

    }
}