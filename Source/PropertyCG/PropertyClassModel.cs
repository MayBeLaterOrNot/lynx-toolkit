// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyClassModel.cs" company="Lynx">
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
//   Provides a model of a class.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    using LynxToolkit;

    /// <summary>
    /// Provides a model of a class.
    /// </summary>
    public class PropertyClassModel
    {
        /// <summary>
        /// Gets or sets the file header format string.
        /// </summary>
        /// <value>The file header.</value>
        /// <remarks>
        /// The {0} will be replaced by the filename.
        /// </remarks>
        public StringBuilder FileHeader { get; set; }

        public string Namespace { get; set; }
        public string AccessModifier { get; set; }
        public bool IsAbstract { get; set; }
        public string Name { get; set; }
        public IList<string> Using { get; private set; }
        public Dictionary<string, Property> Properties { get; private set; }
        public IList<EnumType> EnumTypes { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyClassModel" /> class.
        /// </summary>
        public PropertyClassModel()
        {
            this.Using = new List<string>();
            this.Properties = new Dictionary<string, Property>();
            this.EnumTypes = new List<EnumType>();
            this.AccessModifier = "public";
            this.FileHeader = new StringBuilder();
        }

        /// <summary>
        /// Reads information (file header, name spaces, class name) from the class file.
        /// </summary>
        /// <param name="classFileName">Name of the class file.</param>
        public void ParseClass(string classFileName)
        {
            var fileHeaderExpression = new Regex(@"^// (.*)$", RegexOptions.Compiled);
            var fileNameExpression = new Regex(@"(?<file>file=""(.*?)"")", RegexOptions.Compiled);
            var usingExpression = new Regex(@"using\s+([\w.]+);", RegexOptions.Compiled);
            var namespaceExpression = new Regex(@"namespace\s+([\w.]*)", RegexOptions.Compiled);
            var classExpression = new Regex(@"(\w+)\s+(abstract\s)?(partial\s)\s*class\s+([\w\<\>]+?)[\s:]", RegexOptions.Compiled);
            foreach (var line in File.ReadAllLines(classFileName))
            {
                var fileHeaderMatch = fileHeaderExpression.Match(line);
                if (fileHeaderMatch.Success)
                {
                    var line2 = fileNameExpression.Replace(line, "file=\"{0}\"");
                    this.FileHeader.AppendLine(line2);
                }

                var usingMatch = usingExpression.Match(line);
                if (usingMatch.Success)
                {
                    this.Using.Add(usingMatch.Groups[1].Value);
                }

                var namespaceMatch = namespaceExpression.Match(line);
                if (namespaceMatch.Success)
                {
                    this.Namespace = namespaceMatch.Groups[1].Value;
                }

                var classMatch = classExpression.Match(line);
                if (classMatch.Success && this.Name == null)
                {
                    this.AccessModifier = classMatch.Groups[1].Value;
                    this.IsAbstract = !string.IsNullOrEmpty(classMatch.Groups[2].Value);
                    this.Name = classMatch.Groups[4].Value;
                    if (!classMatch.Groups[4].Success)
                    {
                        Console.WriteLine("  partial is missing in " + classFileName);
                    }
                }
            }
        }

        /// <summary>
        /// Defines a dependency.
        /// </summary>
        private class Dependency
        {
            /// <summary>
            /// Gets or sets the source.
            /// </summary>
            /// <value>The source.</value>
            public string Source { get; set; }

            /// <summary>
            /// Gets or sets the target.
            /// </summary>
            /// <value>The target.</value>
            public string Target { get; set; }
        }

        /// <summary>
        /// Parses the specified file.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="options">The options.</param>
        /// <exception cref="System.InvalidOperationException">Invalid dependency relation. The target property </exception>
        public void Parse(string fileName, IPropertyCodeGeneratorOptions options)
        {
            var commentExpression = new Regex(@"^\s//", RegexOptions.Compiled);
            var usingExpression = new Regex(@"using\s+([\w.]+)", RegexOptions.Compiled);
            var propertyExpression = new Regex(
@"^
(?<Ref>\&)?
(?<Type>[\.<>\w\?]+)
:?(?<Name>\w*)
(?<Enum>\{.+\})?
\s*
(?<Flags>[\|\=\!\#\+\*$rpi]*)?
\s*
(?<Desc>'.*')?
$",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
            var dependencyExpression = new Regex(@"(\w+)\s*->\s*(\w+)", RegexOptions.Compiled);
            var attributeExpression = new Regex(@"^\s*(\[.+\])\s*$", RegexOptions.Compiled);
            var attributes = new List<string>();
            var dependencies = new List<Dependency>();
            foreach (var line in File.ReadAllLines(fileName))
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                if (commentExpression.Match(line).Success)
                {
                    continue;
                }

                var usingMatch = usingExpression.Match(line);
                if (usingMatch.Success)
                {
                    this.Using.Add(usingMatch.Groups[1].Value);
                    continue;
                }

                var dependencyMatch = dependencyExpression.Match(line);
                if (dependencyMatch.Success)
                {
                    dependencies.Add(new Dependency { Source = dependencyMatch.Groups[1].Value, Target = dependencyMatch.Groups[2].Value });
                    continue;
                }

                var attributeMatch = attributeExpression.Match(line);
                if (attributeMatch.Success)
                {
                    attributes.Add(attributeMatch.Groups[1].Value);
                    continue;
                }

                var propertyMatch = propertyExpression.Match(line);
                if (propertyMatch.Success)
                {
                    var flags = propertyMatch.Groups["Flags"].Value;
                    bool supportsIsEnabled = flags.Contains("=");
                    bool supportsIsVisible = flags.Contains("^");
                    bool includeValidateCallback = flags.Contains("!");
                    bool includePropertyChangeCallback = flags.Contains("#");
                    bool isReadOnly = flags.Contains("r");

                    var propertyChangedFlags = options.Flags.Where(f => flags.Contains(f.Key)).Select(f => f.Value).FormatList(" | ");
                    
                    var type = propertyMatch.Groups["Type"].Success ? propertyMatch.Groups["Type"].Value : null;
                    var name = propertyMatch.Groups["Name"].Success ? propertyMatch.Groups["Name"].Value : type;
                    if (string.IsNullOrEmpty(name))
                    {
                        name = type;
                    }

                    var description = propertyMatch.Groups["Desc"].Value;
                    bool isReference = propertyMatch.Groups["Ref"].Success;
                    if (isReference)
                    {
                        type = string.Format(options.ReferencePropertyType, type);
                    }

                    var p = new Property(type, name, attributes, description)
                        {
                            IsReference = isReference,
                            ValidateCallback = includeValidateCallback,
                            PropertyChangeCallback = includePropertyChangeCallback,
                            ReadOnly = isReadOnly,
                            PropertyChangedFlags = propertyChangedFlags
                        };

                    if (propertyMatch.Groups["Enum"].Success)
                    {
                        this.EnumTypes.Add(new EnumType(p.Type, propertyMatch.Groups["Enum"].Value.Trim("{}".ToCharArray())));
                    }

                    this.Properties.Add(p.Name, p);
                    attributes.Clear();

                    if (supportsIsEnabled)
                    {
                        var isEnabledPropertyName = string.Format("Is{0}Enabled", p.Name);
                        attributes.Add("[Browsable(false)]");
                        var isEnabledDescription = string.Format("flag if {0} is enabled", p.Name);
                        var isEnabledProperty = new Property("bool", isEnabledPropertyName, attributes, isEnabledDescription);
                        this.Properties.Add(isEnabledProperty.Name, isEnabledProperty);
                        attributes.Clear();
                    }

                    if (supportsIsVisible)
                    {
                        var isVisiblePropertyName = string.Format("Is{0}Visible", p.Name);
                        attributes.Add("[Browsable(false)]");
                        var isVisibleDescription = string.Format("flag if {0} is visible", p.Name);
                        var isVisibleProperty = new Property("bool", isVisiblePropertyName, attributes, isVisibleDescription);
                        this.Properties.Add(isVisibleProperty.Name, isVisibleProperty);
                        attributes.Clear();
                    }
                }
            }

            foreach (var d in dependencies)
            {
                if (options.ValidateDependencies)
                {
                    if (!this.Properties.ContainsKey(d.Target))
                    {
                        throw new InvalidOperationException(
                            "Invalid dependency relation. The target property " + this.Name + "." + d.Target
                            + " is not defined.");
                    }

                    if (!this.Properties.ContainsKey(d.Source))
                    {
                        throw new InvalidOperationException(
                            "Invalid dependency relation. The source property " + this.Name + "." + d.Source
                            + " is not defined.");
                    }
                }

                var source = this.Properties[d.Source];
                source.Dependencies.Add(d.Target);
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents the partial property class.
        /// </summary>
        /// <param name="options">The options.</param>
        /// <returns>A <see cref="System.String" /> that represents this class.</returns>
        public string ToString(IPropertyCodeGeneratorOptions options)
        {
            var sb = new StringBuilderEx();
            sb.AppendFormat(this.FileHeader.ToString(), Path.GetFileName(options.PropertiesFileName));
            sb.AppendLine("namespace {0}", this.Namespace);
            sb.AppendLine("{");
            string previousUsing = null;
            sb.Indent();
            foreach (var u in this.Using.OrderBy(s => s, new NamespaceComparer()))
            {
                if (previousUsing != null && previousUsing[0] != u[0])
                {
                    sb.AppendLine();
                }

                sb.AppendLine("using {0};", u);
                previousUsing = u;
            }

            sb.AppendLine();

            sb.AppendLine("/// <summary>");
            sb.AppendLine("/// Defines the properties for the {0} class. This file is auto-generated.", EncodeComment(this.Name));
            sb.AppendLine("/// </summary>");
            sb.AppendLine("{0} partial class {1}", this.AccessModifier, this.Name);
            sb.AppendLine("{");
            sb.Indent();

            foreach (var p in this.Properties.Values)
            {
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// The backing field for the {0} property.", p.Name);
                sb.AppendLine("/// </summary>");
                sb.AppendLine("private {0} {1};", p.Type, p.BackingFieldName);
                sb.AppendLine();
            }

            int i = 0;
            foreach (var p in this.Properties.Values)
            {
                i++;
                bool isLast = i == this.Properties.Count;

                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// {0}", p.Summary);
                sb.AppendLine("/// </summary>");
                foreach (var a in p.Attributes)
                {
                    sb.AppendLine(a);
                }

                sb.AppendLine("public {0} {1}", p.Type, p.Name);
                sb.AppendLine("{");
                sb.Indent();
                sb.AppendLine("get");
                sb.AppendLine("{");
                sb.Indent();

                if (p.IsReference && !string.IsNullOrEmpty(options.ReferenceResolve))
                {
                    sb.AppendLine(options.ReferenceResolve + ";", p.BackingFieldName, p.Name);
                }

                sb.AppendLine("return this.{0};", p.BackingFieldName);
                sb.Unindent();
                sb.AppendLine("}");
                sb.AppendLine();
                var setModifier = p.ReadOnly ? "private " : string.Empty;
                sb.AppendLine("{0}set", setModifier);
                sb.AppendLine("{");
                sb.Indent();

                if (p.ValidateCallback)
                {
                    sb.AppendLine(options.ValidateCallback + ";", p.Name);
                }

                var propertyChangedFlags = string.IsNullOrEmpty(p.PropertyChangedFlags)
                                               ? string.Empty
                                               : ", " + p.PropertyChangedFlags;

                var setterFormatString = p.IsReference ? options.ReferencePropertySetter : options.PropertySetter;
                if (p.Dependencies.Count > 0 || p.PropertyChangeCallback)
                {
                    if (p.PropertyChangeCallback)
                    {
                        sb.AppendLine("var oldValue = this.{0};", p.BackingFieldName);
                    }

                    sb.AppendLine("if ({0})", string.Format(setterFormatString, p.BackingFieldName, p.Name, propertyChangedFlags));
                    sb.AppendLine("{");
                    sb.Indent();
                    foreach (var dp in p.Dependencies)
                    {
                        if (this.Properties.ContainsKey(dp))
                        {
                            // The property was generated by code, so the dependency is "safe"
                            sb.AppendLine(options.RaisePropertyChanged + ";", dp);
                        }
                        else
                        {
                            // The property was not generated, so use an expression
                            sb.AppendLine(options.RaisePropertyChangedByExpression + ";", dp);
                        }
                    }

                    if (p.PropertyChangeCallback)
                    {
                        sb.AppendLine(options.PropertyChangeCallback + ";", p.Name);
                    }

                    sb.Unindent();
                    sb.AppendLine("}");
                }
                else
                {
                    sb.AppendLine(setterFormatString + ";", p.BackingFieldName, p.Name, propertyChangedFlags);
                }

                sb.Unindent();
                sb.AppendLine("}");
                sb.Unindent();
                sb.AppendLine("}");
                if (!isLast)
                {
                    sb.AppendLine();
                }
            }

            sb.Unindent();
            sb.AppendLine("}");
            if (this.EnumTypes.Count > 0)
            {
                sb.AppendLine();
            }

            foreach (var type in this.EnumTypes)
            {
                sb.AppendLine("public enum {0}", type.Name);
                sb.AppendLine("{");
                sb.Indent();
                for (int j = 0; j < type.Values.Count; j++)
                {
                    sb.AppendLine(type.Values[j] + (j + 1 < type.Values.Count ? "," : string.Empty));
                }

                sb.Unindent();
                sb.AppendLine("}");
                sb.AppendLine();
            }

            sb.Unindent();
            sb.AppendLine("}");
            return sb.ToString();
        }

        /// <summary>
        /// Encodes the specified string for use in XML comments.
        /// </summary>
        /// <param name="name">The input string.</param>
        /// <returns>The encoded string.</returns>
        private string EncodeComment(string name)
        {
            if (name == null)
            {
                return null;
            }

            name = name.Replace("<", "&lt;");
            name = name.Replace(">", "&gt;");
            return name;
        }

        /// <summary>
        /// Compares name spaces, makes sure System namespaces come first.
        /// </summary>
        private class NamespaceComparer : IComparer<string>
        {
            /// <summary>
            /// The "System" namespace.
            /// </summary>
            private const string System = "System";

            /// <summary>
            /// Compares the specified strings.
            /// </summary>
            /// <param name="x">The first string.</param>
            /// <param name="y">The second string.</param>
            /// <returns>The comparison result.</returns>
            public int Compare(string x, string y)
            {
                if (x.StartsWith(System) && !y.StartsWith(System))
                {
                    return -1;
                }

                if (!x.StartsWith(System) && y.StartsWith(System))
                {
                    return 1;
                }

                return string.CompareOrdinal(x, y);
            }
        }
    }
}