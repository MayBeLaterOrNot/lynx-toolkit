namespace PropertyCG
{
    using System;
    using System.IO;
    using System.Xml;

    /// <summary>
    /// Updates C# project files
    /// </summary>
    /// <remarks></remarks>
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
            foreach (XmlNode itemGroup in root.SelectNodes("//b:ItemGroup", nsmgr))
            {
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

                        var dependentUponFileName = Path.GetFileName(include.Replace(this.Extension, ".cs"));
                        var dependentUponNode = item.SelectSingleNode("b:DependentUpon", nsmgr);
                        if (dependentUponNode == null)
                        {
                            dependentUponNode = doc.CreateElement("DependentUpon", NamespaceUri);
                            item.AppendChild(dependentUponNode);
                            modified = true;
                        }

                        modified |= !string.Equals(dependentUponNode.InnerText, dependentUponFileName);
                        dependentUponNode.InnerText = dependentUponFileName;


                    }
                    if (include.Contains(".Properties.cs"))
                    {
                        if (item.Name != "Compile")
                        {
                            Console.WriteLine(include + " should have build action = Compile.");
                        }

                        var dependentUponFileName = Path.GetFileName(include.Replace(".Properties.cs", ".cs"));
                        var dependentUponNode = item.SelectSingleNode("b:DependentUpon", nsmgr);
                        if (dependentUponNode == null)
                        {
                            dependentUponNode = doc.CreateElement("DependentUpon", NamespaceUri);
                            item.AppendChild(dependentUponNode);
                            modified = true;
                        }

                        modified |= !string.Equals(dependentUponNode.InnerText, dependentUponFileName);
                        dependentUponNode.InnerText = dependentUponFileName;
                        var autogenNode = item.SelectSingleNode("b:AutoGen", nsmgr);
                        if (autogenNode == null)
                        {
                            autogenNode = doc.CreateElement("AutoGen", NamespaceUri);
                            item.AppendChild(autogenNode);
                            modified = true;
                        }
                        modified |= !string.Equals(autogenNode.InnerText, "True");
                        autogenNode.InnerText = "True";
                    }

                }
            }

            if (modified)
            {
                PropertyCodeGenerator.OpenForEdit(FileName, this.OpenForEditExecutable, this.OpenForEditArguments);
                doc.Save(FileName);
            }
            return modified;
        }

    }
}