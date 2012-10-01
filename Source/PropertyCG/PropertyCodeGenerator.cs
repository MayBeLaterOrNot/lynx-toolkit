namespace LynxToolkit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    public interface IOptions
    {
        string AffectsRenderAttribute { get; }
        bool UseExpressions { get; }
        bool IncludeRegions { get; }
    }

    public class PropertyCodeGenerator : IOptions
    {
        public string FileName { get; private set; }
        public string PropertiesFileName { get; private set; }
        public string ClassFileName { get; private set; }

        public string AffectsRenderAttribute { get; set; }
        public bool UseExpressions { get; set; }
        public bool IncludeRegions { get; set; }

        public string OpenForEditExecutable { get; set; }
        public string OpenForEditArguments { get; set; }
        public PropertyClassModel PropertyClassModel { get; set; }

        public PropertyCodeGenerator(string fileName)
        {
            this.FileName = fileName;
            this.ClassFileName = Path.ChangeExtension(this.FileName, ".cs");
            this.PropertiesFileName = Path.ChangeExtension(this.FileName, ".Properties.cs");
            this.PropertyClassModel = new PropertyClassModel();
            this.AffectsRenderAttribute = "AffectsRender";
            this.UseExpressions = false;
            this.IncludeRegions = false;
        }

        public void Generate()
        {
            this.PropertyClassModel.ReadClass(ClassFileName, PropertiesFileName);
            this.PropertyClassModel.Parse(FileName, this);
        }

        public bool SaveIfModified()
        {
            var output = this.PropertyClassModel.ToString(this);
            if (File.Exists(PropertiesFileName))
            {
                var existing = File.ReadAllText(PropertiesFileName);
                if (string.Equals(existing, output)) return false;
                if (OpenForEditExecutable != null) OpenForEdit(PropertiesFileName, OpenForEditExecutable, OpenForEditArguments);
            }
            File.WriteAllText(PropertiesFileName, output);
            return true;
        }

        public static void OpenForEdit(string filename, string exe, string argumentFormatString)
        {
            if (exe == null) return;
            var psi = new ProcessStartInfo(exe, string.Format(argumentFormatString, filename)) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        public bool IsUpToDate()
        {
            var source = new FileInfo(this.FileName);
            var target = new FileInfo(this.PropertiesFileName);
            if (!target.Exists) return false;
            return target.LastWriteTime >= source.LastWriteTime;
        }
    }


    public class Property
    {
        public bool IsReference { get; set; }
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public IList<string> Dependencies { get; private set; }
        public IList<string> Attributes { get; private set; }

        public bool ReadOnly { get; set; }
        public bool PropertyChangeCallback { get; set; }
        public bool ValidateCallback { get; set; }

        public string BackingFieldName
        {
            get
            {
                return Name[0].ToString(CultureInfo.InvariantCulture).ToLower() + Name.Substring(1);
            }
        }
        /// <summary>
        /// Gets the descriptive name of a property.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property. 
        /// </param>
        /// <returns>
        /// The description. 
        /// </returns>
        private string GetDescription(string propertyName)
        {
            var result = new StringBuilder();
            for (int i = 0; i < propertyName.Length; i++)
            {
                if (i == 0)
                {
                    result.Append(this.ToLower(propertyName[i]));
                    continue;
                }

                if (this.IsLower(propertyName[i - 1]) && !this.IsLower(propertyName[i])
                    && (i + 1 < propertyName.Length && this.IsLower(propertyName[i + 1])))
                {
                    result.Append(" ");
                    result.Append(this.ToLower(propertyName[i]));
                    continue;
                }

                result.Append(propertyName[i]);
            }

            return result.ToString();
        }
        /// <summary>
        /// Determines whether the specified character is lowercase.
        /// </summary>
        /// <param name="p0">
        /// The input character. 
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified character is lowercase; otherwise, <c>false</c> . 
        /// </returns>
        private bool IsLower(char p0)
        {
            return p0.ToString(CultureInfo.InvariantCulture).ToLower()[0] == p0;
        }

        /// <summary>
        /// Converts a character to lower case.
        /// </summary>
        /// <param name="p0">
        /// The input character. 
        /// </param>
        /// <returns>
        /// The converted character. 
        /// </returns>
        private char ToLower(char p0)
        {
            return p0.ToString(CultureInfo.InvariantCulture).ToLower()[0];
        }
        public string Summary
        {
            get
            {
                string description = string.IsNullOrEmpty(this.Description) ? this.GetDescription(this.Name) : this.Description;
                string longDescription;
                if (Type == "bool")
                {
                    longDescription = "a value indicating whether the " + description + " should be on or off";
                }
                else
                {
                    longDescription = "the " + description;
                }

                return string.Format(ReadOnly ? "Gets {0}." : "Gets or sets {0}.", longDescription);
            }
        }

        public Property(string type, string name, IEnumerable<string> attributes, string description)
        {
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Dependencies = new List<string>();
            this.Attributes = new List<string>(attributes);
        }
    }

    public class EnumType
    {
        public string Name { get; set; }
        public IList<string> Values { get; private set; }

        public EnumType(string name, string values)
        {
            Name = name;
            Values = new List<string>(values.Split(','));
        }
    }

    public class PropertyClassModel
    {
        public StringBuilder FileHeader { get; set; }
        public string Namespace { get; set; }
        public string AccessModifier { get; set; }
        public string Name { get; set; }
        public IList<string> Using { get; private set; }
        public Dictionary<string, Property> Properties { get; private set; }
        public IList<EnumType> EnumTypes { get; private set; }

        public PropertyClassModel()
        {
            Using = new List<string>();
            Properties = new Dictionary<string, Property>();
            EnumTypes = new List<EnumType>();
            AccessModifier = "public";
            FileHeader = new StringBuilder();
        }

        public void ReadClass(string classFileName, string propertiesFileName)
        {
            var fileHeaderExpression = new Regex(@"^// (.*)$", RegexOptions.Compiled);
            var fileNameExpression = new Regex(@"(?<file>file=""(.*?)"")", RegexOptions.Compiled);
            var usingExpression = new Regex(@"using\s+([\w.]+);", RegexOptions.Compiled);
            var namespaceExpression = new Regex(@"namespace\s+([\w.]*)", RegexOptions.Compiled);
            var classExpression = new Regex(@"(\w+)\s+(partial)?\s+class\s+(\w+)", RegexOptions.Compiled);
            foreach (var line in File.ReadAllLines(classFileName))
            {
                var fileHeaderMatch = fileHeaderExpression.Match(line);
                if (fileHeaderMatch.Success)
                {
                    var line2 = fileNameExpression.Replace(line, "file=\"" + Path.GetFileName(propertiesFileName) + "\"");
                    this.FileHeader.AppendLine(line2);
                }
                var usingMatch = usingExpression.Match(line);
                if (usingMatch.Success)
                {
                    Using.Add(usingMatch.Groups[1].Value);
                }
                var namespaceMatch = namespaceExpression.Match(line);
                if (namespaceMatch.Success)
                {
                    Namespace = namespaceMatch.Groups[1].Value;
                }
                var classMatch = classExpression.Match(line);
                if (classMatch.Success && Name == null)
                {
                    this.AccessModifier = classMatch.Groups[1].Value;
                    Name = classMatch.Groups[3].Value;
                    if (!classMatch.Groups[2].Success)
                    {
                        Console.WriteLine("  partial is missing in " + classFileName);
                    }
                }
            }
        }

        private class Dependency
        {
            public string Source { get; set; }
            public string Target { get; set; }
        }

        public void Parse(string fileName, IOptions options)
        {
            var commentExpression = new Regex(@"^\s//", RegexOptions.Compiled);
            var usingExpression = new Regex(@"using\s+([\w.]+)", RegexOptions.Compiled);
            var propertyExpression = new Regex(@"^(?<Ref>\&)?(?<Type>[\.<>\w\?]+):?(?<Name>\w*)(?<Enum>\{.+\})?\s*(?<Flags>[\|\=\!\#\+\*rpi]*)?\s*(?<Desc>'.*')?$", RegexOptions.Compiled);
            var dependencyExpression = new Regex(@"(\w+)\s*->\s*(\w+)", RegexOptions.Compiled);
            var attributeExpression = new Regex(@"^\s*(\[.+\])\s*$", RegexOptions.Compiled);
            var attributes = new List<string>();
            var dependencies = new List<Dependency>();
            foreach (var line in File.ReadAllLines(fileName))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (commentExpression.Match(line).Success) continue;

                var usingMatch = usingExpression.Match(line);
                if (usingMatch.Success)
                {
                    Using.Add(usingMatch.Groups[1].Value);
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
                    bool affectsRender = flags.Contains("+");
                    if (affectsRender && options.AffectsRenderAttribute != null)
                        attributes.Add("[" + options.AffectsRenderAttribute + "]");

                    var type = propertyMatch.Groups["Type"].Success ? propertyMatch.Groups["Type"].Value : null;
                    var name = propertyMatch.Groups["Name"].Success ? propertyMatch.Groups["Name"].Value : type;
                    if (string.IsNullOrEmpty(name)) name = type;
                    var description = propertyMatch.Groups["Desc"].Value;

                    var p = new Property(type, name, attributes, description);
                    p.IsReference = propertyMatch.Groups["Ref"].Success;

                    bool supportsIsEnabled = flags.Contains("=");
                    bool supportsIsVisible = flags.Contains("^");
                    p.ValidateCallback = flags.Contains("!");
                    p.PropertyChangeCallback = flags.Contains("#");
                    p.ReadOnly = flags.Contains("r");

                    if (propertyMatch.Groups["Enum"].Success)
                    {
                        EnumTypes.Add(new EnumType(p.Type, propertyMatch.Groups["Enum"].Value.Trim("{}".ToCharArray())));
                    }
                    this.Properties.Add(p.Name, p);
                    attributes.Clear();

                    if (supportsIsEnabled)
                    {
                        var ename = string.Format("Is{0}Enabled", p.Name);
                        attributes.Add("Browsable(false)");
                        var edescription = string.Format("flag if {0} is enabled", p.Name);
                        var pe = new Property("bool", ename, attributes, edescription);
                        this.Properties.Add(pe.Name, pe);
                        attributes.Clear();
                    }

                    if (supportsIsVisible)
                    {
                        var ename = string.Format("Is{0}Visible", p.Name);
                        attributes.Add("Browsable(false)");
                        var edescription = string.Format("flag if {0} is visible", p.Name);
                        var pe = new Property("bool", ename, attributes, edescription);
                        this.Properties.Add(pe.Name, pe);
                        attributes.Clear();
                    }
                }
            }

            foreach (var d in dependencies)
            {
                if (!this.Properties.ContainsKey(d.Target))
                {
                    throw new InvalidOperationException("Invalid dependency relation. The target property " + this.Name + "." + d.Target + " is not defined.");
                }
                if (!this.Properties.ContainsKey(d.Source))
                {
                    throw new InvalidOperationException("Invalid dependency relation. The source property " + this.Name + "." + d.Source + " is not defined.");
                }

                var source = this.Properties[d.Source];
                source.Dependencies.Add(d.Target);
            }
        }

        public string ToString(IOptions options)
        {
            var sb = new StringBuilderEx();
            sb.Append(this.FileHeader);
            sb.AppendLine("namespace {0}", this.Namespace);
            sb.AppendLine("{");
            string previousUsing = null;
            sb.Indent();
            foreach (var u in this.Using.OrderBy(s => s, new NamespaceComparer()))
            {
                if (previousUsing != null && previousUsing[0] != u[0]) sb.AppendLine();
                sb.AppendLine("using {0};", u);
                previousUsing = u;
            }
            sb.AppendLine();

            sb.AppendLine("{0} partial class {1}", this.AccessModifier, this.Name);
            sb.AppendLine("{");
            sb.Indent();

            if (options.IncludeRegions)
            {
                sb.AppendLine("#region Fields");
                sb.AppendLine();
            }
            foreach (var p in this.Properties.Values)
            {
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// The backing field for the {0} property.", p.Name);
                sb.AppendLine("/// </summary>");
                sb.AppendLine("private {0} {1};", p.Type, p.BackingFieldName);
                sb.AppendLine();
            }
            if (options.IncludeRegions)
            {
                sb.AppendLine("#endregion");
                sb.AppendLine();
            }

            if (options.IncludeRegions)
            {
                sb.AppendLine("#region Public properties");
                sb.AppendLine();
            }
            foreach (var p in this.Properties.Values)
            {
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
                sb.AppendLine("return this.{0};", p.BackingFieldName);
                sb.Unindent();
                sb.AppendLine("}");
                sb.AppendLine();
                sb.AppendLine("set");
                sb.AppendLine("{");
                sb.Indent();

                if (p.ValidateCallback)
                {
                    sb.AppendLine("this.Validate{0}(value);", p.Name);
                }

                if (p.Dependencies.Count > 0 || p.PropertyChangeCallback)
                {
                    sb.AppendLine("if (this.SetValue(ref {0}, value, \"{1}\"))", p.BackingFieldName, p.Name);
                    sb.AppendLine("{");
                    sb.Indent();
                    foreach (var dp in p.Dependencies)
                    {
                        if (options.UseExpressions)
                        {
                            sb.AppendLine("this.RaisePropertyChanged(() => {0});", dp);
                        }
                        else
                        {
                            sb.AppendLine("this.RaisePropertyChanged(\"{0}\");", dp);
                        }
                    }
                    if (p.PropertyChangeCallback)
                    {
                        sb.AppendLine("this.On{0}Changed(value);", p.Name);
                    }
                    sb.Unindent();
                    sb.AppendLine("}");
                }
                else
                {
                    if (options.UseExpressions)
                    {
                        sb.AppendLine("this.SetValue(ref {0}, value, () => {1});", p.BackingFieldName, p.Name);
                    }
                    else
                    {
                        sb.AppendLine("this.SetValue(ref {0}, value, \"{1}\");", p.BackingFieldName, p.Name);
                    }
                }
                sb.Unindent();
                sb.AppendLine("}");
                sb.Unindent();
                sb.AppendLine("}");
                sb.AppendLine();
            }
            if (options.IncludeRegions)
            {
                sb.AppendLine("#endregion");
            }
            sb.Unindent();
            sb.AppendLine("}");
            if (this.EnumTypes.Count > 0)
                sb.AppendLine();
            foreach (var type in this.EnumTypes)
            {
                sb.AppendLine("public enum {0}", type.Name);
                sb.AppendLine("{");
                sb.Indent();
                foreach (var v in type.Values)
                {
                    sb.AppendLine(v + ",");
                }
                sb.Unindent();
                sb.AppendLine("}");
                sb.AppendLine();
            }
            sb.Unindent();
            sb.AppendLine("}");
            return sb.ToString();
        }

        private class NamespaceComparer : IComparer<string>
        {
            private const string System = "System";

            public int Compare(string x, string y)
            {
                if (x.StartsWith(System) && !y.StartsWith(System)) return -1;
                if (!x.StartsWith(System) && y.StartsWith(System)) return 1;
                return string.CompareOrdinal(x, y);
            }
        }
    }

    public class StringBuilderEx
    {
        private StringBuilder sb;

        public StringBuilderEx(int capacity = 1000)
        {
            sb = new StringBuilder(capacity);
        }

        private string indent = "";
        public void Indent()
        {
            indent += "    ";
        }
        public void Unindent()
        {
            indent = indent.Substring(4);
        }
        public void Append(object s)
        {
            sb.Append(s);
        }
        public void AppendLine(string format = null, params object[] args)
        {
            if (format != null)
            {
                sb.Append(indent);
                if (args.Length > 0)
                    sb.AppendFormat(format, args);
                else sb.Append(format);
            }
            sb.AppendLine();
        }
        public override string ToString()
        {
            return sb.ToString();
        }
    }
}