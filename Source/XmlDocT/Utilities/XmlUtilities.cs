// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Utilities.cs" company="Lynx">
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
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    using LynxToolkit;

    /// <summary>
    /// Utilities for reading xml doc files.
    /// </summary>
    public static class XmlUtilities
    {
        private static readonly char[] whitespaceCharacters = "\r\n ".ToCharArray();

        public static void AppendAttributes(object[] attrs, StringBuilder sb)
        {
            foreach (Attribute a in attrs)
            {
                // todo: add parameters
                sb.AppendLine(string.Format("[{0}]", a.GetType().Name.Replace("Attribute", string.Empty)));
            }
        }

        public static string GetXmlContent(XmlNode node, string elementName)
        {
            if (node != null)
            {
                var summary = node.SelectSingleNode(elementName);
                if (summary != null)
                {
                    return TrimXmlContent(summary.InnerXml);
                }
            }

            return string.Empty;
        }

        /// <summary>
        ///     Gets the inherited types.
        /// </summary>
        /// <param name="type">The source type.</param>
        /// <returns>The enumerated inherited types.</returns>
        public static IEnumerable<Type> GetInheritedTypes(Type type)
        {
            var inheritedTypes = new List<Type>();
            AddInheritedTypes(type, inheritedTypes);
            return inheritedTypes;
        }

        public static XmlNode GetMemberNode(XmlDocument xmldoc, Model model)
        {
            var name = model.GetXmlMemberName();
            var node = xmldoc.SelectSingleNode("/doc/members/member[@name='" + name + "']");
            if (node == null && name.Contains("("))
            {
                name = name.SubstringTo("(");
                node = xmldoc.SelectSingleNode("/doc/members/member[@name='" + name + "']");
            }

            if (node == null)
            {
                if (name.EndsWith("#ctor"))
                {
                    // auto-generated constructor ? 
                }
                else
                {
                    Console.WriteLine("    Missing " + name);
                }
            }

            return node;
        }

        /// <summary>
        ///     Gets the name of the method including generic arguments.
        /// </summary>
        /// <param name="mi">
        ///     The method info.
        /// </param>
        /// <returns>
        ///     The 'nice' method name.
        /// </returns>
        public static string GetNiceMethodName(MethodBase mi)
        {
            var name = mi.Name;
            if (mi.IsConstructor && mi.DeclaringType != null)
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
        ///     Gets the method parameters as a string.
        /// </summary>
        /// <param name="method">The method.</param>
        /// <param name="includeNames">
        ///     include parameter names if set to <c>true</c>.
        /// </param>
        /// <returns>
        ///     The method parameters.
        /// </returns>
        public static string GetNiceMethodParameters(MethodBase method, bool includeNames = false)
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
                {
                    par += " " + p.Name;
                }
            }

            return par;
        }

        /// <summary>
        ///     Gets the name of the type including generic arguments.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <returns>
        ///     The 'nice' type name.
        /// </returns>
        public static string GetNiceTypeName(Type type, bool fullName = false)
        {
            string name = fullName ? GetFullName(type) : GetSimpleTypeName(type.Name);

            if (!type.IsGenericType)
            {
                return name;
            }

            name = name.SubstringTo("`");

            var sb = new StringBuilder();
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

        private static string GetFullName(Type type)
        {
            if (type.FullName != null) return type.FullName;
            return type.Namespace + "." + type.Name;
        }

        public static string GetParameterDescription(XmlNode node, string parameterName)
        {
            if (node == null)
            {
                return null;
            }

            var pnode = node.SelectSingleNode("param[@name='" + parameterName + "']");
            if (pnode == null)
            {
            }

            return pnode != null ? pnode.InnerXml : null;
        }

        /// <summary>
        ///     Gets the name of the parameter type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="methodGenericArgs">The generic args defined on the method.</param>
        /// <returns>
        ///     The parameter type name.
        /// </returns>
        public static string GetXmlMemberParameterName(Type type, IEnumerable<string> methodGenericArgs = null)
        {
            string name = GetXmlMemberTypeName(type);
            if (type.IsGenericParameter)
            {
                if (methodGenericArgs != null && methodGenericArgs.Contains(type.Name))
                {
                    return "``" + type.GenericParameterPosition;
                }

                return "`" + type.GenericParameterPosition;
            }

            if (type.IsArray)
            {
                name = name.Replace("[,]", "[0:,0:]");
                name = name.Replace("[,,]", "[0:,0:,0:]");
            }

            int ii = name.IndexOf('`');
            if (ii > 0)
            {
                name = name.Substring(0, ii);
            }
            else
            {
                return name;
            }

            var sb = new StringBuilder();

            sb.Append(name);
            sb.Append("{");
            int i = 0;
            foreach (var ga in type.GetGenericArguments())
            {
                if (i++ > 0)
                {
                    sb.Append(",");
                }

                sb.Append(GetXmlMemberParameterName(ga, methodGenericArgs));
            }

            sb.Append("}");
            return sb.ToString();
        }

        /// <summary>
        ///     Gets the name of the type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The name formatted as in the xmldoc name attribute.</returns>
        public static string GetXmlMemberTypeName(Type type)
        {
            // Must replace + with . for nested types.
            var name = type.FullName != null ? type.FullName : (type.Namespace + "." + type.Name);
            name = name.Replace('+', '.');

            return name.SubstringTo("[[");
        }

        public static string GetXmlParameterList(MethodBase method, IEnumerable<string> methodGenericArgs)
        {
            string par = string.Empty;
            foreach (var p in method.GetParameters())
            {
                if (par.Length > 0)
                {
                    par += ",";
                }

                var pn = GetXmlMemberParameterName(p.ParameterType, methodGenericArgs);
                pn = pn.Replace("&", "@");
                par += pn;

                if ((p.IsOut || p.IsRetval) && !pn.Contains("@"))
                {
                    par += "@";
                }
            }

            return par;
        }

        /// <summary>
        ///     Trims each line of a multiline string.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <returns>The trimmed lines.</returns>
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

            return sb.ToString().Trim();
        }

        /// <summary>
        ///     Adds the inherited types to the specified list.
        /// </summary>
        /// <param name="type">
        ///     The type.
        /// </param>
        /// <param name="inheritedTypes">
        ///     The inherited types list.
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
        ///     Simplifies the name of the type (replaces with simplified synonym type names).
        /// </summary>
        /// <param name="name">The type name.</param>
        /// <returns>The simple type name.</returns>
        private static string GetSimpleTypeName(string name)
        {
            switch (name)
            {
                case "Object":
                    return "object";

                case "Void":
                    return "void";

                case "Double":
                    return "double";

                case "String":
                    return "string";

                case "Byte":
                    return "byte";

                case "Int16":
                    return "short";

                case "Int32":
                    return "int";

                case "Int64":
                    return "long";

                case "SByte":
                    return "sbyte";

                case "UInt16":
                    return "ushort";

                case "UInt32":
                    return "uint";

                case "UInt64":
                    return "ulong";

                default:
                    return name;
            }
        }

        private static string Trim(string innerXml)
        {
            return innerXml.Trim(whitespaceCharacters);
        }
    }
}