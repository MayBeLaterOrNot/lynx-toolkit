namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public static class Utilities
    {

        /// <summary>
        /// Gets the method parameters as a string.
        /// </summary>
        /// <param name="method">
        /// The method.
        /// </param>
        /// <returns>
        /// The get method parameters.
        /// </returns>
        public static string GetMethodParameters(MethodBase method, bool includeNames = false)
        {
            string par = string.Empty;
            foreach (var p in method.GetParameters())
            {
                if (par.Length > 0)
                {
                    par += ", ";
                }
                par += GetNiceTypeName(p.ParameterType);
                if (includeNames)
                    par += " " + p.Name;
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
        public static string GetNiceMethodName(MethodBase mi)
        {
            var name = mi.Name;
            if (mi.IsConstructor)
            {
                name = mi.DeclaringType.Name;
            }

            if (!mi.IsGenericMethod)
            {
                return name;
            }

            var sb = new StringBuilder();
            sb.Append(name);
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
        public static string GetNiceTypeName(Type type)
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

        /// <summary>
        /// Gets the description for the specified member.
        /// </summary>
        /// <param name="xmldoc">The xml document.</param>
        /// <param name="memberName">Name of the member.</param>
        /// <returns>
        /// The description.
        /// </returns>
        public static string GetSummary(XmlDocument xmldoc, string memberName)
        {
            memberName = memberName.Replace('+', '.');
            var info = GetInformation("summary", xmldoc, memberName);
            return TrimXmlContent(info);
        }

        public static string GetRemarks(XmlDocument xmldoc, string memberName)
        {
            var info = GetInformation("remarks", xmldoc, memberName);
            return TrimXmlContent(info);
        }

        public static string TrimXmlContent(string info)
        {
            var sb = new StringBuilder();
            foreach (var line in info.Split('\n'))
            {
                if (sb.Length > 0)
                {
                    sb.AppendLine();
                }
                sb.Append(Trim(line));
            }
            return sb.ToString();
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
        public static string GetInformation(string block, XmlDocument xmldoc, string memberName)
        {
            var node = xmldoc.SelectSingleNode("/doc/members/member[@name='" + memberName + "']");
            if (node == null) node = xmldoc.SelectSingleNode("/doc/members/member[starts-with(@name,'" + memberName + "')]");
            if (node != null)
            {
                var summary = node.SelectSingleNode(block);
                if (summary != null)
                {
                    return summary.InnerXml;
                }
            }

            return string.Empty;
        }

        private static char[] whitespaceCharacters = "\r\n ".ToCharArray();

        private static string Trim(string innerXml)
        {
            return innerXml.Trim(whitespaceCharacters);
        }

        public static string GetExample(XmlDocument xmldoc, string memberName)
        {
            var node = xmldoc.SelectSingleNode("/doc/members/member[@name='" + memberName + "']");
            if (node != null)
            {
                var summary = node.SelectSingleNode("example");
                if (summary != null)
                {
                    return Trim(summary.InnerXml);
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Adds the inherited types to the specified list.
        /// </summary>
        /// <param name="type">
        /// The type.
        /// </param>
        /// <param name="inheritedTypes">
        /// The inherited types list.
        /// </param>
        public static void AddInheritedTypes(Type type, List<Type> inheritedTypes)
        {
            if (type.BaseType != null)
            {
                AddInheritedTypes(type.BaseType, inheritedTypes);
            }

            inheritedTypes.Add(type);
        }

        /// <summary>
        /// Creates the directory if missing.
        /// </summary>
        /// <param name="path">The path.</param>
        public static void CreateDirectoryIfMissing(string path)
        {
            var outputdir = Path.GetFullPath(path);
            if (!Directory.Exists(outputdir))
            {
                Directory.CreateDirectory(outputdir);
            }
        }

        public static void AppendAttributes(object[] attrs, StringBuilder sb)
        {
            foreach (Attribute a in attrs)
                sb.AppendLine(string.Format("[{0}]", a.GetType().Name.Replace("Attribute", "")));
        }
    }
}