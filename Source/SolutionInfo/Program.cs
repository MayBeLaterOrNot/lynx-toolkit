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
//   Defines the Program type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace SolutionInfo
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;

    using LynxToolkit;
    using LynxToolkit.Documents.OpenXml;
    using LynxToolkit.Documents.Spreadsheet;

    internal class Program
    {
        private static void ExportSolutionInfo(string solutionFile)
        {
            var solution = new Solution(solutionFile);
            var outputFile = Path.ChangeExtension(solutionFile, ".xlsx");

            Console.WriteLine("Solution file:");
            Console.WriteLine("  " + solutionFile);
            Console.WriteLine();

            Console.WriteLine("Output files:");
            Console.WriteLine("  " + outputFile);
            Console.WriteLine();

            Console.WriteLine("Project files:");
            foreach (var p in solution.Projects.Where(IsCSProject))
            {
                Console.WriteLine("  " + Path.GetFileName(p.FilePath));
            }

            var wb = new Workbook();
            var headerStyle = wb.AddStyle(bold: true);
            var s1 = wb.AddSheet("Projects");
            s1[0, 0] = "Project";
            s1[0, 1] = "AssemblyName";
            s1[0, 2] = "RootNamespace";
            s1[0, 3] = "TargetFrameworkVersion";
            s1[0, 4] = "TargetFrameworkProfile";
            s1[0, 5] = "SignAssembly";
            s1[0, 6] = "ProjectGuid";
            s1[0, 7] = "ProjectGuid (in .sln)";
            s1.ApplyStyle("A1:H1", headerStyle);

            int i = 1;
            foreach (var project in solution.Projects.Where(IsCSProject))
            {
                s1[i, 0] = project.File;
                s1[i, 1] = project.AssemblyName;
                s1[i, 2] = project.RootNamespace;
                s1[i, 3] = project.TargetFrameworkVersion;
                s1[i, 4] = project.TargetFrameworkProfile;
                s1[i, 5] = project.SignAssembly;
                s1[i, 6] = project.ProjectGuid;
                if (string.Compare(project.SolutionProjectGuid, project.ProjectGuid, StringComparison.InvariantCultureIgnoreCase) != 0)
                {
                    s1[i, 7] = project.SolutionProjectGuid;
                }
                else
                {
                    s1[i, 7] = "ok";
                }

                i++;
            }

            s1.AutoSizeColumns();

            var s2 = wb.AddSheet("References");
            s2[0, 0] = "Project";
            s2[0, 1] = "Reference";
            s2[0, 2] = "Version";
            s2[0, 3] = "Culture";
            s2[0, 4] = "RequiredTargetFramework";
            s2[0, 5] = "SpecificVersion";
            s2[0, 6] = "SpecificVersion (computed)";
            s2[0, 7] = "HintPath";
            s2[0, 8] = "Full HintPath";
            s2.ApplyStyle("A1:I1", headerStyle);

            i = 1;
            foreach (var r in solution.Projects.SelectMany(p => p.References).OrderBy(r => r.Include))
            {
                s2[i, 0] = r.ReferencingProject.AssemblyName;
                s2[i, 1] = r.Project;
                s2[i, 2] = r.Version;
                s2[i, 3] = r.Culture;
                s2[i, 4] = r.RequiredTargetFramework;
                s2[i, 5] = r.SpecificVersion;
                s2[i, 6] = r.ActualSpecificVersion;
                s2[i, 7] = r.HintPath;
                s2[i, 8] = r.FullHintPath;
                i++;
            }

            s2.AutoSizeColumns();

            var s3 = wb.AddSheet("ProjectReferences");
            s3[0, 0] = "Project";
            s3[0, 1] = "Name";
            s3[0, 2] = "Guid";
            s3[0, 3] = "Guid (in project)";
            s3.ApplyStyle("A1:E1", headerStyle);

            i = 1;

            foreach (var r in solution.Projects.SelectMany(p => p.ProjectReferences).OrderBy(r => r.Include))
            {
                s3[i, 0] = r.ReferencingProject.AssemblyName;
                s3[i, 1] = r.Name;
                s3[i, 2] = r.Project;

                var p2 = solution.Projects.FirstOrDefault(p => p.FilePath == r.Include);
                if (p2 != null)
                {
                    if (string.Equals(p2.ProjectGuid, r.Project, StringComparison.InvariantCultureIgnoreCase))
                    {
                        s3[i, 3] = "ok";
                    }
                    else
                    {
                        s3[i, 3] = p2.ProjectGuid;
                    }
                }
                else
                {
                    s3[i, 4] = "not found";
                }

                i++;
            }

            s3.AutoSizeColumns();

            var s4 = wb.AddSheet("Configurations");
            s4[0, 0] = "Project";
            s4[0, 1] = "Configuration";
            s4[0, 2] = "Platform";
            s4[0, 3] = "DebugType";
            s4[0, 4] = "Optimize";
            s4[0, 5] = "OutputPath";
            s4[0, 6] = "DefineConstants";
            s4[0, 7] = "ErrorReport";
            s4[0, 8] = "WarningLevel";
            s4[0, 9] = "CodeAnalysisRuleSet";
            s4[0, 10] = "DocumentationFile";
            s4.ApplyStyle("A1:K1", headerStyle);

            i = 1;
            foreach (var c in solution.Projects.SelectMany(p => p.Configurations).OrderBy(c => c.Configuration))
            {
                s4[i, 0] = c.Project.AssemblyName;
                s4[i, 1] = c.Configuration;
                s4[i, 2] = c.Platform;
                s4[i, 3] = c.DebugType;
                s4[i, 4] = c.Optimize;
                s4[i, 5] = c.OutputPath;
                s4[i, 6] = c.DefineConstants;
                s4[i, 7] = c.ErrorReport;
                s4[i, 8] = c.WarningLevel;
                s4[i, 9] = c.CodeAnalysisRuleSet;
                s4[i, 10] = c.DocumentationFile;

                i++;
            }

            s4.AutoSizeColumns();

            ExcelWriter.Export(wb, outputFile);
            Process.Start(outputFile);
        }

        private static bool IsCSProject(Project p)
        {
            return p.FilePath.Contains(".csproj");
        }

        private static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);

            foreach (var arg in args)
            {
                if (arg.Contains("*"))
                {
                    var dir = Path.GetDirectoryName(arg);
                    var pattern = Path.GetFileName(arg);
                    if (dir == string.Empty)
                    {
                        dir = ".";
                    }

                    foreach (var f in Directory.GetFiles(dir, pattern))
                    {
                        ExportSolutionInfo(f);
                    }

                    continue;
                }

                ExportSolutionInfo(arg);
            }
        }
    }

    public class Solution
    {
        public Solution(string path)
        {
            this.Projects = new List<Project>(GetSolutionProjects(path));
        }

        public IList<Project> Projects { get; private set; }

        private static IEnumerable<Project> GetSolutionProjects(string path)
        {
            var content = File.ReadAllText(path);
            var dir = Path.GetDirectoryName(path);
            var regex = new Regex(
                "^Project\\(\"(.*)\"\\) = \"([^\"]*)\", \"([^\"]*)\", \"([^\"]*)\"", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(content))
            {
                var projectTypeGuid = m.Groups[1].Value;
                var name = m.Groups[2].Value;
                var file = m.Groups[3].Value;
                var projectGuid = m.Groups[4].Value;
                var filePath = Path.Combine(dir, file);
                if (!File.Exists(filePath))
                {
                    continue;
                }

                var project = new Project(filePath, projectGuid);
                yield return project;

                if (project.ProjectGuid != projectGuid)
                {
                    Console.WriteLine("ProjectGuid mismatch (" + file + ")");
                    Console.WriteLine("  " + path + ": " + projectGuid);
                    Console.WriteLine("  " + file + ": " + project.ProjectGuid);
                    Console.WriteLine();
                }
            }
        }
    }

    public class Reference
    {
        public string HintPath { get; set; }

        public string Include { get; set; }

        public Project ReferencingProject { get; set; }

        public string RequiredTargetFramework { get; set; }

        public string SpecificVersion { get; set; }

        public string Project { get; set; }

        public string Version { get; set; }

        public string Culture { get; set; }

        public string ActualSpecificVersion { get; set; }

        public string FullHintPath { get; set; }
    }

    public class ProjectReference
    {
        public string Include { get; set; }

        public string Name { get; set; }

        public string Project { get; set; }

        public Project ReferencingProject { get; set; }
    }

    public class Condition
    {
        public string CodeAnalysisRuleSet { get; set; }

        public string Configuration { get; set; }

        public string DebugType { get; set; }

        public string DefineConstants { get; set; }

        public string DocumentationFile { get; set; }

        public string ErrorReport { get; set; }

        public string Optimize { get; set; }

        public string OutputPath { get; set; }

        public string Platform { get; set; }

        public Project Project { get; set; }

        public string WarningLevel { get; set; }
    }

    public class Project
    {
        private readonly XmlDocument doc;

        private readonly XmlNamespaceManager nsmgr;

        private bool modified = false;

        public string FilePath { get; set; }

        public Project(string filePath, string projectGuidInSolutionFile)
        {
            this.FilePath = Path.GetFullPath(filePath);
            var dir = Path.GetDirectoryName(this.FilePath) ?? string.Empty;

            this.SolutionProjectGuid = projectGuidInSolutionFile;
            this.IncludeFiles = new List<string>();
            this.References = new List<Reference>();
            this.ProjectReferences = new List<ProjectReference>();
            this.Configurations = new List<Condition>();

            // load xml file
            this.doc = new XmlDocument();
            this.doc.Load(filePath);

            this.nsmgr = new XmlNamespaceManager(this.doc.NameTable);
            this.nsmgr.AddNamespace("b", "http://schemas.microsoft.com/developer/msbuild/2003");

            var root = this.doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            this.AssemblyName = this.GetValue(root, "//b:Project/b:PropertyGroup/b:AssemblyName");
            this.RootNamespace = this.GetValue(root, "//b:Project/b:PropertyGroup/b:RootNamespace");
            this.ProjectGuid = this.GetValue(root, "//b:Project/b:PropertyGroup/b:ProjectGuid");

            this.SccProjectName = this.GetValue(root, "//b:Project/b:PropertyGroup/b:SccProjectName");
            this.SccLocalPath = this.GetValue(root, "//b:Project/b:PropertyGroup/b:SccLocalPath");
            this.SccAuxPath = this.GetValue(root, "//b:Project/b:PropertyGroup/b:SccAuxPath");
            this.SccProvider = this.GetValue(root, "//b:Project/b:PropertyGroup/b:SccProvider");

            this.TargetFrameworkVersion = this.GetValue(root, "//b:Project/b:PropertyGroup/b:TargetFrameworkVersion");
            this.TargetFrameworkProfile = this.GetValue(root, "//b:Project/b:PropertyGroup/b:TargetFrameworkProfile");
            this.SignAssembly = this.GetValue(root, "//b:Project/b:PropertyGroup/b:SignAssembly");

            foreach (XmlNode node in root.SelectNodes("//b:Compile", this.nsmgr))
            {
                var include = GetAttribute(node, "Include");
                this.IncludeFiles.Add(Path.Combine(dir, include));
            }

            foreach (XmlNode node in root.SelectNodes("//b:ItemGroup/b:Reference", this.nsmgr))
            {
                if (node == null)
                {
                    continue;
                }

                var specificVersion = this.GetValue(node, "b:SpecificVersion");
                var hintPath = this.GetValue(node, "b:HintPath");
                var framework = this.GetValue(node, "b:RequiredTargetFramework");

                var include = GetAttribute(node, "Include");
                var includeValues = GetIncludeValues(include);
                string reference, version, culture, architecture;
                includeValues.TryGetValue("$Name", out reference);
                includeValues.TryGetValue("Version", out version);
                includeValues.TryGetValue("Culture", out culture);
                includeValues.TryGetValue("processorArchitecture", out architecture);

                var actualSpecificVersion = !string.IsNullOrEmpty(specificVersion)
                                                ? specificVersion
                                                : (!string.IsNullOrEmpty(version)).ToString();
                var fullHintPath = hintPath != null ? Path.GetFullPath(Path.Combine(dir, hintPath)) : null;

                var r = new Reference
                {
                    ReferencingProject = this,
                    Include = include,
                    Project = reference,
                    Version = version,
                    Culture = culture,
                    RequiredTargetFramework = framework,
                    SpecificVersion = specificVersion,
                    ActualSpecificVersion = actualSpecificVersion,
                    HintPath = hintPath,
                    FullHintPath = fullHintPath
                };

                this.References.Add(r);
            }

            foreach (XmlNode node in root.SelectNodes("//b:ItemGroup/b:ProjectReference", this.nsmgr))
            {
                if (node == null)
                {
                    continue;
                }

                var include = GetAttribute(node, "Include");
                var includeFullPath = Path.GetFullPath(Path.Combine(dir, include));
                var pr = new ProjectReference
                {
                    ReferencingProject = this,
                    Include = includeFullPath,
                    Project = this.GetValue(node, "b:Project"),
                    Name = this.GetValue(node, "b:Name")
                };
                this.ProjectReferences.Add(pr);
            }

            foreach (XmlNode node in root.SelectNodes("//b:PropertyGroup", this.nsmgr))
            {
                if (node == null)
                {
                    continue;
                }

                var condition = GetAttribute(node, "Condition");
                if (condition == null)
                {
                    continue;
                }

                var r1 = new Regex(@".*\|.*==.*'(.+)\|(.+)'");
                var m = r1.Match(condition);
                if (!m.Success)
                {
                    continue;
                }

                var c = new Condition
                {
                    Project = this,
                    Configuration = m.Groups[1].Value.Trim(),
                    Platform = m.Groups[2].Value.Trim(),
                    DebugType = this.GetValue(node, "b:DebugType"),
                    Optimize = this.GetValue(node, "b:Optimize"),
                    OutputPath = this.GetValue(node, "b:OutputPath"),
                    DefineConstants = this.GetValue(node, "b:DefineConstants"),
                    ErrorReport = this.GetValue(node, "b:ErrorReport"),
                    WarningLevel = this.GetValue(node, "b:WarningLevel"),
                    CodeAnalysisRuleSet = this.GetValue(node, "b:CodeAnalysisRuleSet"),
                    DocumentationFile = this.GetValue(node, "b:DocumentationFile"),
                };

                this.Configurations.Add(c);
            }
        }

        public void Clean()
        {
            var root = this.doc.DocumentElement;
            if (root == null)
            {
                return;
            }

            // SAK is a flag that tells Visual Studio/SSMS the project is under source control, and the real bindings
            // are persisted in the mssccprj.scc files. So, Visual Studio/SSMS reads them from there and uses them when needed.
            //modified |= this.SetValue(root, "//b:Project/b:PropertyGroup/b:SccProjectName", "SAK");
            //modified |= this.SetValue(root, "//b:Project/b:PropertyGroup/b:SccLocalPath", "SAK");
            //modified |= this.SetValue(root, "//b:Project/b:PropertyGroup/b:SccAuxPath", "SAK");
            //modified |= this.SetValue(root, "//b:Project/b:PropertyGroup/b:SccProvider", "SAK");

            //foreach (XmlNode node in root.SelectNodes("//b:ItemGroup/b:Reference", this.nsmgr))
            //{
            //    if (node == null)
            //    {
            //        continue;
            //    }

            //    modified |= this.SetAttribute(node, "Include", r.Include.SubstringTo(","));
            //    modified |= this.SetValue(node, "b:RequiredTargetFramework", null);
            //    modified |= this.SetValue(node, "b:SpecificVersion", null);
            //}

            //if (clean)
            //{
            //    if (c.Configuration == "Debug")
            //    {
            //        modified |= this.SetValue(node, "b:OutputPath", @"bin\Debug\");
            //        modified |= this.SetValue(node, "b:DocumentationFile", null);
            //    }
            //    else
            //    {
            //        if (!c.Project.AssemblyName.Contains("Tests"))
            //        {
            //            var docfile = c.OutputPath + c.Project.AssemblyName + ".XML";
            //            modified |= this.SetValue(node, "b:DocumentationFile", docfile);
            //        }
            //    }

            //    modified |= this.SetValue(node, "b:WarningLevel", "4");
            //    modified |= this.SetValue(node, "b:CodeAnalysisRuleSet", "AllRules.ruleset");
            //}


        }

        public void SaveIfModified()
        {
            if (modified)
            {
                Console.WriteLine("  Saving changes.");
                this.doc.Save(this.FilePath);
            }
        }

        public string AssemblyName { get; private set; }

        public IList<Condition> Configurations { get; set; }

        public IList<string> IncludeFiles { get; set; }

        public IList<ProjectReference> ProjectReferences { get; set; }

        public IList<Reference> References { get; set; }

        public string ProjectGuid { get; private set; }

        public string SolutionProjectGuid { get; private set; }

        public string RootNamespace { get; private set; }

        public string SccAuxPath { get; private set; }

        public string SccLocalPath { get; private set; }

        public string SccProjectName { get; private set; }

        public string SccProvider { get; private set; }

        public string SignAssembly { get; set; }

        public string TargetFrameworkProfile { get; private set; }

        public string TargetFrameworkVersion { get; private set; }

        public string File
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }

        public bool CleanElement(XmlNode node)
        {
            // remove end tag from empty element
            var element1 = node as XmlElement;
            if (element1 != null && string.IsNullOrEmpty(element1.InnerText) && !element1.IsEmpty
                && !element1.HasChildNodes)
            {
                element1.IsEmpty = true;
                return true;
            }

            return false;
        }

        public string GetValue(XmlNode node, string xpath)
        {
            var node2 = node.SelectSingleNode(xpath, this.nsmgr);
            if (node2 == null)
            {
                return null;
            }

            return node2.InnerText;
        }

        public bool SetValue(XmlNode node, string xpath, string value)
        {
            var modified = this.CleanElement(node);

            var node2 = node.SelectSingleNode(xpath, this.nsmgr);
            if (node2 == null)
            {
                return modified;
            }

            if (string.Equals(node2.InnerText, value))
            {
                return modified;
            }

            if (value == null)
            {
                node2.ParentNode.RemoveChild(node2);
                this.CleanElement(node2.ParentNode);
            }
            else
            {
                node2.InnerText = value;
            }

            Console.WriteLine("  Changed {0} to {1}", node2.Name, value ?? "null");
            return true;
        }

        private static string GetAttribute(XmlNode node, string attributeName)
        {
            if (node == null || node.Attributes[attributeName] == null)
            {
                return null;
            }

            return node.Attributes[attributeName].InnerText;
        }

        private bool SetAttribute(XmlNode node, string attributeName, string s)
        {
            if (node == null)
            {
                return false;
            }

            var value = GetAttribute(node, attributeName);
            if (string.Equals(s, value))
            {
                return false;
            }

            node.Attributes[attributeName].InnerText = s;
            Console.WriteLine("  Changed {0}/{1} to {2}", node.Name, attributeName, s);

            return true;
        }

        private static Dictionary<string, string> GetIncludeValues(string include)
        {
            var dictionary = new Dictionary<string, string>();
            foreach (var field in include.Split(','))
            {
                var keyValuePair = field.Split('=');
                if (keyValuePair.Length == 1)
                {
                    dictionary.Add("$Name", keyValuePair[0]);
                }
                else
                {
                    dictionary.Add(keyValuePair[0].Trim(), keyValuePair[1].Trim());
                }
            }

            return dictionary;
        }
    }
}