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
// --------------------------------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;

namespace FindObsoleteFiles
{
    using System.IO;
    using System.Text.RegularExpressions;
    using System.Xml;

    // Finds all *.cs files that are not included in the solution's projects.
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Utilities.ApplicationHeader);

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