namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Xml;

    public class Model
    {
        public Dictionary<string, NamespaceModel> Namespaces { get; private set; }

        public string Title { get; set; }
        public string Description { get; set; }

        public Model()
        {
            this.Namespaces = new Dictionary<string, NamespaceModel>();
        }

        public void Add(string assemblyFile)
        {
            string assemblyPath = Path.GetFullPath(assemblyFile);

            string xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
            string assemblyDirectory = Path.GetDirectoryName(assemblyPath) ?? string.Empty;

            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
                {
                    var an = new AssemblyName(args.Name);
                    var assemblyFileName = an.Name + ".dll";
                    Console.WriteLine("Loading {0}", assemblyFileName);
                    var file = Path.Combine(assemblyDirectory, assemblyFileName);
                    if (File.Exists(file))
                    {
                        var asm = Assembly.LoadFile(file);
                        return asm;
                    }
                    throw new FileNotFoundException("Assembly not found.", file);
                };

            Console.WriteLine("Directory: {0}", assemblyDirectory);
            Console.WriteLine("Assembly: {0}", Path.GetFileName(assemblyPath));
            Console.WriteLine("Xml comments: {0}", Path.GetFileName(xmlPath));
            var assembly = Assembly.LoadFile(assemblyPath);
            var xmldoc = new XmlDocument();
            xmldoc.Load(xmlPath);

            Console.WriteLine();

            foreach (var type in assembly.GetTypes().OrderBy(t => t.Name))
            {
                if (DontDocumentType(type)) continue;
                var nsm = this.CreateOrGetNamespaceModel(type.Namespace);
                var tm = new TypeModel(type, xmldoc);
                nsm.Types.Add(tm);
                Console.WriteLine(Utilities.GetNiceTypeName(type));
            }

            foreach (var type in assembly.GetTypes())
            {
                if (DontDocumentType(type)) continue;
                var tm = Find(type);
                var baseModel = this.Find(type.BaseType);
                if (baseModel != null) baseModel.DerivedTypes.Add(tm);
            }
        }

        private static bool DontDocumentType(Type type)
        {
            if (!type.IsClass && !type.IsInterface && !type.IsEnum && !type.IsValueType)
            {
                return true;
            }
            if (type.Name.StartsWith("<") || type.Name.StartsWith("__"))
            {
                return true;
            }
            return false;
        }

        private NamespaceModel CreateOrGetNamespaceModel(string ns)
        {
            NamespaceModel nsm;
            if (this.Namespaces.TryGetValue(ns, out nsm))
                return nsm;
            nsm = new NamespaceModel { Name = ns };
            this.Namespaces.Add(ns, nsm);
            return nsm;
        }

        public TypeModel Find(Type type)
        {
            return this.Namespaces.Values.Select(ns => ns.Types.FirstOrDefault(tm => tm.Type == type)).FirstOrDefault(r => r != null);
        }
    }
}