namespace XmlDocT
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using System.Xml;

    public class TypeModel : Content
    {
        public IList<ConstructorModel> Constructors { get; private set; }
        public IList<PropertyModel> Properties { get; private set; }
        public IList<MethodModel> Methods { get; private set; }
        public IList<EventModel> Events { get; private set; }

        public Type Type { get; set; }
        public Type[] InheritedTypes { get; set; }
        public List<TypeModel> DerivedTypes { get; set; }
        public Assembly Assembly { get { return this.Type.Assembly; } }

        public override string GetTitle()
        {
            if (this.Type.IsClass)
                return this.ToString() + " Class";
            if (this.Type.IsEnum)
                return this.ToString() + " Enumeration";
            else
                if (this.Type.IsValueType)
                    return this.ToString() + " Structure";
            return this.ToString();
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            Utilities.AppendAttributes(this.Type.GetCustomAttributes(false), sb);
            if (this.Type.IsPublic)
                sb.Append("public ");
            if (this.Type.IsClass)
                sb.Append("class ");
            if (this.Type.IsEnum)
                sb.Append("enum ");
            else if (this.Type.IsValueType)
                sb.Append("struct ");
            sb.Append(Utilities.GetNiceTypeName(this.Type));
            return sb.ToString();
        }

        public override string GetPageTitle()
        {
            return string.Format("{0} ({1})", this.GetTitle(), this.Type.Namespace);
        }

        public override string GetFileName()
        {
            return this.Type.FullName;
        }

        public override string ToString()
        {
            return Utilities.GetNiceTypeName(this.Type);
        }

        public TypeModel(Type type, XmlDocument xmldoc)
        {
            this.DerivedTypes = new List<TypeModel>();

            this.Constructors = new List<ConstructorModel>();
            this.Properties = new List<PropertyModel>();
            this.Methods = new List<MethodModel>();
            this.Events = new List<EventModel>();

            this.Type = type;
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            this.Description = Utilities.GetSummary(xmldoc, "T:" + type.FullName);

            var inheritedTypes = new List<Type>();
            Utilities.AddInheritedTypes(type, inheritedTypes);
            this.InheritedTypes = inheritedTypes.ToArray();

            var constructors = type.GetConstructors(flags);
            if (constructors.Length > 0)
            {
                foreach (var ci in constructors)
                {
                    // skip constructors declared in other assemblies
                    if (ci != null && ci.DeclaringType != null && ci.DeclaringType.Assembly != type.Assembly)
                    {
                        continue;
                    }

                    var cname = "M:" + type.FullName + ".#ctor";
                    var cm = new ConstructorModel(ci)
                        {
                            Description = Utilities.GetSummary(xmldoc, cname),
                            Parameters = Utilities.GetMethodParameters(ci),
                            Remarks = Utilities.GetRemarks(xmldoc, cname),
                            Example = Utilities.GetExample(xmldoc, cname)
                        };

                    this.Constructors.Add(cm);
                }
            }

            var properties = type.GetProperties(flags);
            if (properties.Length > 0)
            {
                foreach (var pi in properties)
                {
                    if (pi.DeclaringType.Assembly != type.Assembly) continue;

                    var pname = "P:" + pi.DeclaringType.FullName + "." + pi.Name;
                    var pm = new PropertyModel(pi)
                        {
                            Description = Utilities.GetSummary(xmldoc, pname),
                            Remarks = Utilities.GetRemarks(xmldoc, pname),
                            Example = Utilities.GetExample(xmldoc, pname)
                        };

                    this.Properties.Add(pm);
                }
            }

            var methods = type.GetMethods(flags);
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

                var mname = "M:" + mi.DeclaringType.FullName + "." + mi.Name;
                var mm = new MethodModel(mi)
                    {
                        Description = Utilities.GetSummary(xmldoc, mname),
                        Remarks = Utilities.GetRemarks(xmldoc, mname),
                        Example = Utilities.GetExample(xmldoc, mname)
                    };
                this.Methods.Add(mm);
            }

            var events = type.GetEvents(flags);
            foreach (var ei in events)
            {
                if (ei.DeclaringType.Assembly != type.Assembly) continue;
                var ename = "E:" + ei.DeclaringType.FullName + "." + ei.Name;
                var em = new EventModel(ei)
                    {
                        Description = Utilities.GetSummary(xmldoc, ename),
                        Remarks = Utilities.GetRemarks(xmldoc, ename),
                        Example = Utilities.GetExample(xmldoc, ename)
                    };
            }

            this.Remarks = Utilities.GetRemarks(xmldoc, "T:" + type.FullName);
            this.Example = Utilities.GetExample(xmldoc, "T:" + type.FullName);
        }
    }
}