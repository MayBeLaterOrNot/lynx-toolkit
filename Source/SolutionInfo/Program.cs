using System;
using System.Collections.Generic;

namespace SolutionInfo
{
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Xml;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);

            var clean = false;
            foreach (var arg in args)
            {
                if (arg == "/Clean")
                {
                    clean = true;
                    continue;
                }

                ExportSolutionInfo(arg, clean);
            }
        }

        static void ExportSolutionInfo(string solutionFile, bool clean)
        {
            var dir = Path.GetDirectoryName(solutionFile);
            var solution = new Solution(solutionFile);

            var solutionFileName = Path.GetFileName(solutionFile);

            var projectsFile = Path.ChangeExtension(solutionFileName, ".Projects.csv");
            var referencesFile = Path.ChangeExtension(solutionFileName, ".References.csv");
            var projectReferencesFile = Path.ChangeExtension(solutionFileName, ".ProjectReferences.csv");
            var configurationsFile = Path.ChangeExtension(solutionFileName, ".Configurations.csv");

            Console.WriteLine("Solution file:");
            Console.WriteLine("  " + solutionFile);
            Console.WriteLine();

            Console.WriteLine("Output files:");
            Console.WriteLine("  " + projectsFile);
            Console.WriteLine("  " + referencesFile);
            Console.WriteLine("  " + projectReferencesFile);
            Console.WriteLine("  " + configurationsFile);
            Console.WriteLine();

            Console.WriteLine("Project files:");
            foreach (var p in solution.Projects.Where(IsCSProject))
            {
                Console.WriteLine("  " + Path.GetFileName(p));
            }

            var projects = new List<Project>();
            using (var ps = new StreamWriter(projectsFile))
            {
                ps.WriteLine("Project;AssemblyName;RootNamespace;TargetFrameworkVersion;TargetFrameworkProfile;SignAssembly");
                foreach (var projectFile in solution.Projects.Where(IsCSProject))
                {
                    var path = Path.Combine(dir, projectFile);
                    var project = new Project(path, clean);
                    ps.WriteLine(
                        "{0};{1};{2};{3};{4};{5}",
                        projectFile,
                        project.AssemblyName,
                        project.RootNamespace,
                        project.TargetFrameworkVersion,
                        project.TargetFrameworkProfile,
                        project.SignAssembly);

                    projects.Add(project);
                }
            }

            using (var rs = new StreamWriter(referencesFile))
            {
                rs.WriteLine("Project;Include;RequiredTargetFramework;SpecificVersion;HintPath");
                foreach (var r in projects.SelectMany(p => p.References).OrderBy(r => r.Include))
                {
                    rs.WriteLine(
                        "{0};{1};{2};{3};{4}",
                        r.ReferencingProject.AssemblyName,
                        r.Include,
                        r.RequiredTargetFramework == null ? null : string.Format("\"v{0}\"", r.RequiredTargetFramework),
                        r.SpecificVersion,
                        r.HintPath);
                }
            }

            using (var prs = new StreamWriter(projectReferencesFile))
            {
                prs.WriteLine("Project;Include;Name;Guid");

                foreach (var r in projects.SelectMany(p => p.ProjectReferences).OrderBy(r => r.Include))
                {
                    prs.WriteLine("{0};{1};{2};{3}",
                        r.ReferencingProject.AssemblyName,
                        r.Include,
                        r.Name,
                        r.Project);
                }
            }

            using (var cs = new StreamWriter(configurationsFile))
            {
                cs.WriteLine("Project;Configuration;Platform;DebugType;Optimize;OutputPath;DefineConstants;ErrorReport;WarningLevel;CodeAnalysisRuleSet;DocumentationFile");
                foreach (var c in projects.SelectMany(p => p.Configurations).OrderBy(c => c.Configuration))
                {
                    cs.WriteLine(
                        "{0};{1};{2};{3};{4};{5};{6};{7};{8};{9};{10}",
                        c.Project.AssemblyName,
                        c.Configuration,
                        c.Platform,
                        c.DebugType,
                        c.Optimize,
                        c.OutputPath,
                        "\"" + c.DefineConstants + "\"",
                        c.ErrorReport,
                        c.WarningLevel,
                        c.CodeAnalysisRuleSet,
                        c.DocumentationFile);
                }
            }
        }

        private static bool IsCSProject(string s)
        {
            return s.Contains(".csproj");
        }
    }


    public class Solution
    {
        public Solution(string path)
        {
            Projects = new List<string>(GetSolutionProjects(path));
        }

        public IList<string> Projects { get; private set; }

        private static IEnumerable<string> GetSolutionProjects(string path)
        {
            var r = new StreamReader(path);
            var content = r.ReadToEnd();
            var regex = new Regex("^Project\\(\".*\"\\) = \"(.*?)\", \"(.*?)\"", RegexOptions.Multiline);
            foreach (Match m in regex.Matches(content))
            {
                var name = m.Groups[1].Value;
                var file = m.Groups[2].Value;
                yield return file;
            }
        }

    }

    public class Reference
    {
        public Project ReferencingProject { get; set; }
        public string Include { get; set; }
        public string RequiredTargetFramework { get; set; }
        public bool SpecificVersion { get; set; }
        public string HintPath { get; set; }
    }

    public class ProjectReference
    {
        public Project ReferencingProject { get; set; }
        public string Include { get; set; }
        public string Project { get; set; }
        public string Name { get; set; }
    }

    public class Condition
    {
        public Project Project { get; set; }
        public string Configuration { get; set; }
        public string Platform { get; set; }
        public string DebugType { get; set; }
        public string Optimize { get; set; }
        public string OutputPath { get; set; }
        public string DefineConstants { get; set; }
        public string ErrorReport { get; set; }
        public string WarningLevel { get; set; }
        public string CodeAnalysisRuleSet { get; set; }
        public string DocumentationFile { get; set; }
    }

    public class Project
    {
        public IList<string> IncludeFiles { get; set; }
        public IList<Reference> References { get; set; }
        public IList<ProjectReference> ProjectReferences { get; set; }
        public IList<Condition> Configurations { get; set; }

        public string AssemblyName { get; private set; }
        public string RootNamespace { get; private set; }

        public string SccProjectName { get; private set; }
        public string SccLocalPath { get; private set; }
        public string SccAuxPath { get; private set; }
        public string SccProvider { get; private set; }

        public string TargetFrameworkVersion { get; private set; }
        public string TargetFrameworkProfile { get; private set; }
        public string SignAssembly { get; set; }

        private XmlDocument doc;
        private XmlNamespaceManager nsmgr;

        public Project(string path, bool clean = false)
        {
            bool modified = false;

            IncludeFiles = new List<string>();
            References = new List<Reference>();
            ProjectReferences = new List<ProjectReference>();
            Configurations = new List<Condition>();

            // load xml file
            doc = new XmlDocument();
            doc.Load(path);

            nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("b", "http://schemas.microsoft.com/developer/msbuild/2003");

            var root = doc.DocumentElement;
            if (root == null) return;

            this.AssemblyName = GetValue(root, "//b:Project/b:PropertyGroup/b:AssemblyName");
            this.RootNamespace = GetValue(root, "//b:Project/b:PropertyGroup/b:RootNamespace");

            this.SccProjectName = GetValue(root, "//b:Project/b:PropertyGroup/b:SccProjectName");
            this.SccLocalPath = GetValue(root, "//b:Project/b:PropertyGroup/b:SccLocalPath");
            this.SccAuxPath = GetValue(root, "//b:Project/b:PropertyGroup/b:SccAuxPath");
            this.SccProvider = GetValue(root, "//b:Project/b:PropertyGroup/b:SccProvider");

            if (clean)
            {
                // SAK is a flag that tells Visual Studio/SSMS the project is under source control, and the real bindings 
                // are persisted in the mssccprj.scc files. So, Visual Studio/SSMS reads them from there and uses them when needed.
                modified |= SetValue(root, "//b:Project/b:PropertyGroup/b:SccProjectName", "SAK");
                modified |= SetValue(root, "//b:Project/b:PropertyGroup/b:SccLocalPath", "SAK");
                modified |= SetValue(root, "//b:Project/b:PropertyGroup/b:SccAuxPath", "SAK");
                modified |= SetValue(root, "//b:Project/b:PropertyGroup/b:SccProvider", "SAK");
            }

            this.TargetFrameworkVersion = GetValue(root, "//b:Project/b:PropertyGroup/b:TargetFrameworkVersion");
            this.TargetFrameworkProfile = GetValue(root, "//b:Project/b:PropertyGroup/b:TargetFrameworkProfile");
            this.SignAssembly = GetValue(root, "//b:Project/b:PropertyGroup/b:SignAssembly");

            var dir = Path.GetDirectoryName(path);

            foreach (XmlNode node in root.SelectNodes("//b:Compile", nsmgr))
            {
                var include = GetAttribute(node, "Include");
                IncludeFiles.Add(Path.Combine(dir, include));
            }

            foreach (XmlNode node in root.SelectNodes("//b:ItemGroup/b:Reference", nsmgr))
            {
                if (node == null) continue;

                var r = new Reference
                    {
                        Include = GetAttribute(node, "Include"),
                        ReferencingProject = this,
                        RequiredTargetFramework = this.GetValue(node, "b:RequiredTargetFramework"),
                        SpecificVersion = string.Equals(this.GetValue(node, "b:SpecificVersion"), "TRUE", StringComparison.InvariantCultureIgnoreCase),
                        HintPath = this.GetValue(node, "b:HintPath")
                    };

                References.Add(r);

                if (clean)
                {
                    modified |= SetAttribute(node, "Include", SubstringUntil(r.Include, ','));
                    modified |= this.SetValue(node, "b:RequiredTargetFramework", null);
                    modified |= this.SetValue(node, "b:SpecificVersion", null);
                }
            }

            foreach (XmlNode node in root.SelectNodes("//b:ItemGroup/b:ProjectReference", nsmgr))
            {
                if (node == null) continue;

                var pr = new ProjectReference
                    {
                        ReferencingProject = this,
                        Include = GetAttribute(node, "Include"),
                        Project = this.GetValue(node, "b:Project"),
                        Name = this.GetValue(node, "b:Name")
                    };
                ProjectReferences.Add(pr);

                if (clean)
                {
                    modified |= SetAttribute(node, "Include", SubstringUntil(pr.Include, ','));
                }
            }

            foreach (XmlNode node in root.SelectNodes("//b:PropertyGroup", nsmgr))
            {
                if (node == null) continue;
                var condition = GetAttribute(node, "Condition");
                if (condition == null) continue;

                var r1 = new Regex(@".*\|.*==.*'(.+)\|(.+)'");
                var m = r1.Match(condition);
                if (!m.Success) continue;

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

                if (clean)
                {
                    if (c.Configuration == "Debug")
                    {
                        modified |= this.SetValue(node, "b:OutputPath", @"bin\Debug\");
                        modified |= this.SetValue(node, "b:DocumentationFile", null);
                    }
                    else
                    {
                        if (!c.Project.AssemblyName.Contains("Tests"))
                        {
                            var docfile = c.OutputPath + c.Project.AssemblyName + ".XML";
                            modified |= this.SetValue(node, "b:DocumentationFile", docfile);
                        }
                    }
                    modified |= this.SetValue(node, "b:WarningLevel", "4");
                    modified |= this.SetValue(node, "b:CodeAnalysisRuleSet", "AllRules.ruleset");
                }

                Configurations.Add(c);
            }

            if (clean && modified)
            {
                Console.WriteLine("  Saving changes.");
                doc.Save(path);
            }
        }

        private string SubstringUntil(string include, char c)
        {
            int i = include.IndexOf(c);
            if (i <= 0) return include;
            return include.Substring(0, i);
        }

        private bool SetAttribute(XmlNode node, string attributeName, string s)
        {
            if (node == null) return false;
            var value = GetAttribute(node, attributeName);
            if (string.Equals(s, value)) return false;
            node.Attributes[attributeName].InnerText = s;
            Console.WriteLine("  Changed {0}/{1} to {2}", node.Name, attributeName, s);

            return true;
        }

        private static string GetAttribute(XmlNode node, string attributeName)
        {
            if (node == null || node.Attributes[attributeName] == null) return null;
            return node.Attributes[attributeName].InnerText;
        }

        public string GetValue(XmlNode node, string xpath)
        {
            var node2 = node.SelectSingleNode(xpath, nsmgr);
            if (node2 == null) return null;
            return node2.InnerText;
        }

        public bool CleanElement(XmlNode node)
        {
            // remove end tag from empty element
            var element1 = node as XmlElement;
            if (element1 != null && string.IsNullOrEmpty(element1.InnerText) && !element1.IsEmpty && !element1.HasChildNodes)
            {
                element1.IsEmpty = true;
                return true;
            }
            return false;
        }

        public bool SetValue(XmlNode node, string xpath, string value)
        {
            var modified = CleanElement(node);

            var node2 = node.SelectSingleNode(xpath, nsmgr);
            if (node2 == null) return modified;

            if (string.Equals(node2.InnerText, value)) return modified;
            if (value == null)
            {
                node2.ParentNode.RemoveChild(node2);
                CleanElement(node2.ParentNode);
            }
            else
            {
                node2.InnerText = value;
            }

            Console.WriteLine("  Changed {0} to {1}", node2.Name, value ?? "null");
            return true;
        }
    }

}
