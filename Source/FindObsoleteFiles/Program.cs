//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace LynxToolkit
{
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;

    // Finds all *.cs files that are not included in the solution's projects.
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Application.Header);

            foreach (var arg in args)
            {
                var f = new Finder();
                f.Find(arg);
            }
        }
    }

    class Finder
    {
        public IEnumerable<string> GetProjects(string path)
        {
            var r = new StreamReader(path);
            var content = r.ReadToEnd();
            var regex = new Regex("^Project\\(\".*\"\\) = \"(.*?)\", \"(.*?)\"",RegexOptions.Multiline);
            foreach (Match m in regex.Matches(content))
            {
                yield return m.Groups[2].Value;
            }
        }

        public void Find(string slnPath)
        {
            var files = new List<string>();
            var slnDir = Path.GetDirectoryName(slnPath);
            foreach (var proj in GetProjects(slnPath))
            {
                if (!proj.EndsWith(".csproj"))
                    continue;
                var projPath = Path.Combine(slnDir, proj);
                files.AddRange(GetFiles(projPath));
            }

            Search(slnDir,path=>
                {
                    if (!files.Contains(path))
                        Console.WriteLine(path);
                });
        }

        private void Search(string path, Action<string> action)
        {
            if (IsExcluded(path))
                return;
            foreach (var f in Directory.GetFiles(path, "*.cs")) action(f);
            foreach (var d in Directory.GetDirectories(path)) Search(d, action);
        }

        private bool IsExcluded(string path)
        {
            var name=Path.GetFileName(path);
            if (name.StartsWith("bin"))
                return true;
            if (name.StartsWith("obj"))
                return true;
            if (name.StartsWith("packages"))
                return true;
            return false;
        }

        private IEnumerable<string> GetFiles(string projPath)
        {
            var dir = Path.GetDirectoryName(projPath);
            var doc = new XmlDocument();
            doc.Load(projPath);
            var root = doc.DocumentElement;
            var nsmgr = new XmlNamespaceManager(doc.NameTable);
            nsmgr.AddNamespace("b", "http://schemas.microsoft.com/developer/msbuild/2003");

            foreach (XmlNode node in root.SelectNodes("//b:Compile",nsmgr))
            {
                var include = node.Attributes["Include"].InnerText;
                yield return Path.Combine(dir, include);
            }
        }
    }
}
