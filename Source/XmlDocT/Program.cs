//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Xml;

    /// <summary>
    /// Utility to generate "MSDN-style" documentation in Codeplex wiki format.
    ///   - Reads XML comments from the XML documentation files
    ///   - Reflects the assemblies to find inheritance hierarchy
    /// Limitations
    ///   - Does not create cross references (yet)
    /// </summary>
    internal class Program
    {
        #region Public Methods

        public static string OutputDirectory { get; set; }

        /// <summary>
        /// The main program.
        /// </summary>
        /// <param name="args">
        /// The command line arguments.
        /// </param>
        internal static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);

            OutputDirectory = string.Empty;
            string format = "html";
            bool singlePage = false;

            foreach (var arg in args)
            {
                string[] kv = arg.Split('=');
                if (kv.Length > 0)
                {
                    switch (kv[0].ToLower())
                    {
                        case "/singlepage":
                            singlePage = true;
                            break;
                        case "/output":
                            OutputDirectory = kv[1];
                            Console.WriteLine("Output directory:   {0}", OutputDirectory);
                            Console.WriteLine();
                            continue;
                        case "/format":
                            format = kv[1].ToLower();
                            Console.WriteLine("Format:             {0}", format);
                            Console.WriteLine();
                            continue;
                    }
                }

                if (File.Exists(arg))
                {
                    GenerateDoc(arg, format, singlePage);
                }
                else
                {
                    if (Directory.Exists(arg))
                    {
                        foreach (var fileName in Directory.GetFiles(arg, "*.dll"))
                        {
                            GenerateDoc(fileName, format, singlePage);
                        }
                    }
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the inherited types to the specified list.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="inheritedTypes">
        /// The inherited types list.
        /// </param>
        private static void AddInheritedTypes(Type type, List<Type> inheritedTypes)
        {
            if (type.BaseType != null)
            {
                AddInheritedTypes(type.BaseType, inheritedTypes);
            }

            inheritedTypes.Add(type);
        }

        /// <summary>
        /// Generates the documentation for the specified assembly.
        /// </summary>
        /// <param name="assemblyFile">
        /// The assembly file.
        /// </param>
        private static void GenerateDoc(string assemblyFile, string format, bool singlePage)
        {
            string assemblyPath = Path.GetFullPath(assemblyFile);
            string fileName = Path.GetFileName(assemblyFile);

            string xmlPath = Path.ChangeExtension(assemblyPath, ".xml");
            string docPath = Path.Combine(OutputDirectory, Path.ChangeExtension(fileName, ".txt"));

            CreateDirectoryIfMissing(OutputDirectory);

            string assemblyDirectory = Path.GetDirectoryName(assemblyPath);

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

            Console.WriteLine("Directory:          {0}", assemblyDirectory);
            Console.WriteLine("Assembly:           {0}", Path.GetFileName(assemblyPath));
            var assembly = Assembly.LoadFile(assemblyPath);
            var xmldoc = new XmlDocument();
            Console.WriteLine("Xml comments:       {0}", Path.GetFileName(xmlPath));
            xmldoc.Load(xmlPath);

            Console.WriteLine("Output:             {0}", Path.GetFileName(docPath));

            Console.WriteLine();
            using (var w = new StreamWriter(docPath))
            {
                GenerateNamespaceDoc(assembly, w, xmldoc);
                w.WriteLine();

                foreach (var type in assembly.GetTypes().OrderBy(t => t.Name))
                {
                    if (!type.IsClass)
                    {
                        continue;
                    }
                    if (type.Name.StartsWith("<"))
                    {
                        continue;
                    }
                    //if (type.IsAbstract)
                    //{
                    //    continue;
                    //}
                    Console.WriteLine(GetNiceTypeName(type));
                    GenerateDoc(w, type, xmldoc);
                    w.WriteLine();
                }
            }

            // Process.Start(docPath);
        }

        /// <summary>
        /// Creates the directory if missing.
        /// </summary>
        /// <param name="path">The path.</param>
        private static void CreateDirectoryIfMissing(string path)
        {
            var outputdir = Path.GetFullPath(path);
            if (!Directory.Exists(outputdir))
            {
                Directory.CreateDirectory(outputdir);
            }
        }

        private static void GenerateNamespaceDoc(Assembly assembly, StreamWriter w, XmlDocument xmldoc)
        {
            var types = assembly.GetTypes().OrderBy(t => t.Name);
            var namespaces = new HashSet<string>();
            foreach (var type in types)
            {
                if (!namespaces.Contains(type.Namespace)) namespaces.Add(type.Namespace);
            }
            string header1 = "! {0} Namespace";
            string header2 = "!! Classes";
            string header3 = "!! Structures";
            string classesHeader = "|| Class || Description ||";
            string classesRow = "| {0} | {1} |";
            string structuresHeader = "|| Structure || Description ||";
            string structuresRow = "| {0} | {1} |";

            foreach (var ns in namespaces)
            {
                if (ns == null)
                    continue;
                if (ns.EndsWith(".Properties"))
                    continue;
                if (ns == "XamlGeneratedNamespace")
                    continue;

                w.WriteLine(header1, ns);
                w.WriteLine();
                var classes = types.Where(t => t.Namespace == ns && t.IsClass);
                if (classes.Count() > 0)
                {
                    w.WriteLine(header2);
                    w.WriteLine();
                    w.WriteLine(classesHeader);
                    foreach (var c in classes)
                    {
                        if (c.Name.StartsWith("<"))
                            continue;
                        w.WriteLine(classesRow, GetNiceTypeName(c), GetDescription(xmldoc, "T:" + c.FullName));
                    }
                }

                w.WriteLine();
                var structures = types.Where(t => t.Namespace == ns && t.IsValueType);
                if (structures.Count() > 0)
                {
                    w.WriteLine(header3);
                    w.WriteLine();
                    w.WriteLine(structuresHeader);
                    foreach (var c in structures)
                    {
                        w.WriteLine(structuresRow, GetNiceTypeName(c), GetDescription(xmldoc, "T:" + c.FullName));
                    }
                }
                w.WriteLine();
            }

        }

        /// <summary>
        /// Generates the documentation for the specified type.
        /// </summary>
        /// <param name="w">
        /// The stream writer.
        /// </param>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="xmldoc">
        /// The xmldoc.
        /// </param>
        private static void GenerateDoc(StreamWriter w, Type type, XmlDocument xmldoc)
        {
            BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            // Only declared members
            // flags|= BindingFlags.DeclaredOnly;

            string header1 = "!! {0} Class";
            string header2 = "*{0}*";
            string tableHeader = "|| Name || Description ||";

            var assembly = Assembly.GetAssembly(type);

            var typeName = GetNiceTypeName(type);

            w.WriteLine(header1, typeName);
            w.WriteLine(GetDescription(xmldoc, "T:" + type.FullName));
            w.WriteLine();

            w.WriteLine("*Inheritance Hierarchy*");
            var inheritedTypes = new List<Type>();
            AddInheritedTypes(type, inheritedTypes);
            var prefix = string.Empty;
            foreach (var t in inheritedTypes)
            {
                prefix += "*";
                w.WriteLine(prefix + " " + t.FullName);
            }

            w.WriteLine();

            w.WriteLine("*Namespace:* {0}", type.Namespace);
            w.WriteLine("*Assembly:* {0} (in {0}.dll)", assembly.GetName().Name);
            w.WriteLine();

            var constructors = type.GetConstructors(flags);
            if (constructors.Length > 0)
            {
                w.WriteLine(header2, "Constructors");
                w.WriteLine(tableHeader);
                foreach (var ci in constructors)
                {
                    if (ci.DeclaringType.Assembly != type.Assembly) continue;

                    string description = GetDescription(xmldoc, "M:" + type.FullName + ".#ctor") + GetInheritedText(ci, type);
                    w.WriteLine("| {0}({1}) | {2} |", GetNiceTypeName(type), GetMethodParameters(ci), description);
                }

                w.WriteLine();
            }

            var properties = type.GetProperties(flags);
            if (properties.Length > 0)
            {
                w.WriteLine(header2, "Properties");
                w.WriteLine(tableHeader);
                foreach (var pi in properties)
                {
                    if (pi.DeclaringType.Assembly != type.Assembly) continue;
                    string description = GetDescription(xmldoc, "P:" + pi.DeclaringType.FullName + "." + pi.Name) + GetInheritedText(pi, type);
                    w.WriteLine("| " + pi.Name + " | " + description + " | ");
                }

                w.WriteLine();
            }

            var methods = type.GetMethods(flags);
            if (methods.Length > 0)
            {
                bool isHeaderAdded = false;
                foreach (var mi in methods)
                {
                    if (mi.DeclaringType.Assembly != type.Assembly) continue;

                    if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))
                    {
                        continue;
                    }
                    if (mi.Name.StartsWith("add_") || mi.Name.StartsWith("remove_"))
                    {
                        continue;
                    }

                    if (!isHeaderAdded)
                    {
                        w.WriteLine(header2, "Methods");
                        w.WriteLine(tableHeader);
                        isHeaderAdded = true;
                    }

                    string description = GetDescription(xmldoc, "M:" + mi.DeclaringType.FullName + "." + mi.Name) + GetInheritedText(mi, type);
                    w.WriteLine("| {0}({1}) | {2} |", GetNiceMethodName(mi), GetMethodParameters(mi), description);
                }

                w.WriteLine();
            }

            var events = type.GetEvents(flags);
            if (events.Length > 0)
            {
                w.WriteLine(header2, "Events");
                w.WriteLine(tableHeader);
                foreach (var ei in events)
                {
                    if (ei.DeclaringType.Assembly != type.Assembly) continue;

                    string description = GetDescription(xmldoc, "E:" + ei.DeclaringType.FullName + "." + ei.Name) + GetInheritedText(ei, type);
                    w.WriteLine("| {0} | {1} |", ei.Name, description);
                }

                w.WriteLine();
            }

            var remarks = GetInformation("remarks", xmldoc, "T:" + type.FullName);
            if (!string.IsNullOrWhiteSpace(remarks))
            {
                w.WriteLine(header2, "Remarks");
                w.WriteLine(remarks);
                w.WriteLine();
            }

            var example = GetInformation("example", xmldoc, "T:" + type.FullName);
            if (!string.IsNullOrWhiteSpace(example))
            {
                w.WriteLine(header2, "Example");
                w.WriteLine(example);
                w.WriteLine();
            }
        }

        /// <summary>
        /// Return a description if the member is inherited.
        /// </summary>
        /// <param name="pi">The pi.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private static string GetInheritedText(MemberInfo pi, Type type)
        {
            if (pi.DeclaringType == type) return null;
            return " (Inherited from " + GetNiceTypeName(pi.DeclaringType) + ".)";
        }

        /// <summary>
        /// Gets the description for the specified member.
        /// </summary>
        /// <param name="xmldoc">The xml document.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>
        /// The description.
        /// </returns>
        private static string GetDescription(XmlDocument xmldoc, string memberName)
        {
            memberName = memberName.Replace('+', '.');
            return GetInformation("summary", xmldoc, memberName);
        }

        static Regex seeRegex = new Regex("<see.*?cref.*?=.*?\\\"(?<cref>[\\:\\.\\w]*\\.(?<name>.*?))\".*?/>");
        static Regex paramrefRegex = new Regex("<paramref.*?name.*?=.*?\"(?<name>.*?)\".*?/>");
        static Regex typeParamRegex = new Regex("<typeparam.*?name.*?=.*?\"(?<name>.*?)\".*?/>");
        static Regex cRegex = new Regex("<c>(?<code>.*?)</c>");
        static Regex paraRegex = new Regex("<para>(?<para>.*?)</para>");
        static Regex codeRegex = new Regex("<code>(?<code>.*?)</code>");
        // static Regex xmlCodeRegex = new Regex("<code>(?<code>\s*<.*?)</code>");

        private static string ConvertToWiki(string description)
        {
            // http://msdn.microsoft.com/en-us/library/5ast78ax.aspx
            description = seeRegex.Replace(description, "${name}");
            description = paramrefRegex.Replace(description, "${name}");
            description = typeParamRegex.Replace(description, "${name}");
            description = cRegex.Replace(description, "\r\n${code}");
            description = paraRegex.Replace(description, "\r\n${para}");
            description = codeRegex.Replace(description, new MatchEvaluator(ReplaceCode));
            return description;
        }

        private static string ReplaceCode(Match m)
        {
            string source = m.Groups["code"].Value;
            string tag = LooksLikeXml(source) ? "{code:xml}" : "{code:c#}";
            return string.Format("{0}\r\n{1}\r\n{2}", tag, source, tag);

        }

        private static bool LooksLikeXml(string source)
        {
            source = source.Trim();
            if (source.StartsWith("<?xml")) return true;
            if (source.StartsWith("<")) return true;
            return false;
        }

        /// <summary>
        /// Gets the description for the specified member from the xml comment document.
        /// </summary>
        /// <param name="xmldoc">
        /// The xml document.
        /// </param>
        /// <param name="memberName">
        /// Name of the member.
        /// </param>
        /// <returns>
        /// The get description.
        /// </returns>
        private static string GetInformation(string block, XmlDocument xmldoc, string memberName)
        {
            var node = xmldoc.SelectSingleNode("/doc/members/member[@name='" + memberName + "']");
            if (node == null)
                node = xmldoc.SelectSingleNode("/doc/members/member[starts-with(@name,'" + memberName + "')]");
            if (node != null)
            {
                var summary = node.SelectSingleNode(block);
                if (summary != null)
                {
                    var d = summary.InnerXml;
                    d = ConvertToWiki(d);
                    d = d.Trim(whitespace);
                    d = TrimLines(d);
                    return d;
                }
            }

            return string.Empty;
        }

        private static string GetExample(XmlDocument xmldoc, string memberName)
        {
            var node = xmldoc.SelectSingleNode("/doc/members/member[@name='" + memberName + "']");
            if (node != null)
            {
                var summary = node.SelectSingleNode("example");
                if (summary != null)
                {
                    var d = summary.InnerXml;
                    d = ConvertToWiki(d);
                    d = d.Trim(whitespace);
                    d = TrimLines(d);
                    return d;
                }
            }

            return string.Empty;
        }

        private static string TrimLines(string s)
        {
            StringBuilder sb = new StringBuilder();
            var lines = s.Split('\n');
            foreach (var line in lines)
            {
                var trimmedLine = line.Trim(whitespace);
                if (sb.Length > 0) sb.AppendLine();
                sb.Append(trimmedLine);
            }
            return sb.ToString();
        }

        static char[] whitespace = " \r\n".ToCharArray();

        /// <summary>
        /// Gets the method parameters as a string.
        /// </summary>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <returns>
        /// The get method parameters.
        /// </returns>
        private static string GetMethodParameters(MethodBase method)
        {
            string par = string.Empty;
            foreach (var p in method.GetParameters())
            {
                if (par.Length > 0)
                {
                    par += ", ";
                }

                par += GetNiceTypeName(p.ParameterType);
            }

            return par;
        }

        /// <summary>
        /// Gets the name of the method including generic arguments.
        /// </summary>
        /// <param name="mi">
        /// The method info.
        /// </param>
        /// <returns>
        /// The nice method name.
        /// </returns>
        private static string GetNiceMethodName(MethodInfo mi)
        {
            if (!mi.IsGenericMethod)
            {
                return mi.Name;
            }

            var sb = new StringBuilder();
            sb.Append(mi.Name);
            sb.Append("<");
            int i = 0;
            foreach (var ga in mi.GetGenericArguments())
            {
                if (i++ > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetNiceTypeName(ga));
            }

            sb.Append(">");
            return sb.ToString();
        }

        /// <summary>
        /// Gets the name of the type including generic arguments.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <returns>
        /// The nice type name.
        /// </returns>
        private static string GetNiceTypeName(Type type)
        {
            if (!type.IsGenericType)
            {
                return type.Name;
            }

            var sb = new StringBuilder();
            string name = type.Name;
            int ii = name.IndexOf('`');
            if (ii > 0)
            {
                name = name.Substring(0, ii);
            }

            sb.Append(name);
            sb.Append("<");
            int i = 0;
            foreach (var ga in type.GetGenericArguments())
            {
                if (i++ > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetNiceTypeName(ga));
            }

            sb.Append(">");
            return sb.ToString();
        }

        #endregion
    }
}
