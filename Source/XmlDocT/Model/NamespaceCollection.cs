// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NamespaceCollection.cs" company="Lynx">
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

namespace XmlDocT
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Xml;

    public class NamespaceCollection : IEnumerable<NamespaceModel>
    {
        public NamespaceCollection()
        {
            this.Namespaces = new Dictionary<string, NamespaceModel>();
            this.IgnoreAttributes = new List<string>();
        }

        public string Description { get; set; }

        public List<string> IgnoreAttributes { get; private set; }

        public Dictionary<string, NamespaceModel> Namespaces { get; private set; }

        public string Title { get; set; }

        public void Add(string assemblyFile)
        {
            string assemblyPath = Path.GetFullPath(assemblyFile);

            string xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var an = new AssemblyName(args.Name);
                    var assemblyFileName = an.Name + ".dll";

                    //// Console.WriteLine("Loading {0}", assemblyFileName);
                    var file = Path.Combine(assemblyDirectory, assemblyFileName);
                    if (File.Exists(file))
                    {
                        var asm = Assembly.LoadFile(file);
                        return asm;
                    }

                    throw new FileNotFoundException("Assembly not found.", file);
                };

            Console.WriteLine();
            Console.WriteLine("  Assembly: {0}", Path.GetFullPath(assemblyPath));
            Console.WriteLine("  Xml comments: {0}", Path.GetFullPath(xmlPath));
            var assembly = Assembly.LoadFile(assemblyPath);
            var xmldoc = new XmlDocument();
            xmldoc.Load(xmlPath);

            Console.WriteLine();

            foreach (var type in assembly.GetTypes().OrderBy(t => t.Name))
            {
                if (!this.IsTypeDocumentable(type))
                {
                    continue;
                }

                var nsm = this.CreateOrGetNamespaceModel(type.Namespace, xmldoc);
                var tm = new TypeModel(type, xmldoc);
                nsm.Types.Add(tm);
                Console.WriteLine("    " + Utilities.GetNiceTypeName(type));
            }

            foreach (var type in assembly.GetTypes())
            {
                if (!this.IsTypeDocumentable(type))
                {
                    continue;
                }

                var tm = this.Find(type);
                TypeModel baseModel;
                if (this.Find(type.BaseType, out baseModel))
                {
                    baseModel.DerivedTypes.Add(tm);
                }
            }
        }

        public TypeModel Find(Type type)
        {
            return
                this.Namespaces.Values.Select(ns => ns.Types.FirstOrDefault(tm => tm.Type == type))
                    .FirstOrDefault(r => r != null);
        }

        public bool Find(Type type, out TypeModel model)
        {
            model = this.Find(type);
            return model != null;
        }

        public bool Find(string cref, Type scope, out Model model)
        {
            if (cref.StartsWith("T:"))
            {
                if (this.FindType(cref.Substring(2), scope, out model))
                {
                    return true;
                }
            }

            model = null;
            return false;
        }

        public bool FindMethod(string name, out Model model)
        {
            foreach (var ns in this)
            {
                foreach (var t in ns.Types)
                {
                    foreach (var m in t.Methods)
                    {
                        if (m.GetXmlMemberName() == name)
                        {
                            model = m;
                            return true;
                        }
                    }
                }
            }

            model = null;
            return false;
        }

        public bool FindType(string name, out Model model)
        {
            model =
                this.Namespaces.Values.Select(ns => ns.Types.FirstOrDefault(tm => tm.Type.FullName == name))
                    .FirstOrDefault(r => r != null);
            return model != null;
        }

        public IEnumerator<NamespaceModel> GetEnumerator()
        {
            return this.Namespaces.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private NamespaceModel CreateOrGetNamespaceModel(string ns, XmlDocument xmldoc)
        {
            NamespaceModel nsm;
            if (this.Namespaces.TryGetValue(ns, out nsm))
            {
                return nsm;
            }

            nsm = new NamespaceModel { Name = ns };
            var node = Utilities.GetMemberNode(xmldoc, nsm);
            nsm.Description = Utilities.GetXmlContent(node, "summary");
            this.Namespaces.Add(ns, nsm);
            return nsm;
        }

        private bool FindMethod(string name, Type scope, out Model model)
        {
            if (this.FindMethod(name, out model))
            {
                return true;
            }

            if (scope != null)
            {
                if (this.FindMethod(string.Format("{0}.{1}", scope.FullName, name), out model))
                {
                    return true;
                }
            }

            model = null;
            return false;
        }

        private bool FindType(string name, Type scope, out Model model)
        {
            if (this.FindType(name, out model))
            {
                return true;
            }

            if (scope != null)
            {
                if (this.FindType(string.Format("{0}.{1}", scope.Namespace, name), out model))
                {
                    return true;
                }

                if (this.FindType(string.Format("{0}.{1}", scope.FullName, name), out model))
                {
                    return true;
                }
            }

            model = null;
            return false;
        }

        private bool IsTypeDocumentable(Type type)
        {
            if (!type.IsClass && !type.IsInterface && !type.IsEnum && !type.IsValueType)
            {
                return false;
            }

            if (type.Name.StartsWith("<") || type.Name.StartsWith("__"))
            {
                return false;
            }

            foreach (Attribute attribute in type.GetCustomAttributes(true))
            {
                if (attribute is CompilerGeneratedAttribute)
                {
                    return false;
                }

                if (this.IgnoreAttributes.Any(ia => attribute.GetType().Name == ia))
                {
                    return false;
                }
            }

            return true;
        }
    }
}