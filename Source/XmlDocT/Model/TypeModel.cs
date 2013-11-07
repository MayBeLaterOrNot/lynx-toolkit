// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeModel.cs" company="Lynx Toolkit">
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

    public class TypeModel : Model
    {
        public TypeModel(Type type, XmlDocument xmldoc)
        {
            this.DerivedTypes = new List<TypeModel>();

            this.Constructors = new List<ConstructorModel>();
            this.Properties = new List<PropertyModel>();
            this.Fields = new List<FieldModel>();
            this.Methods = new List<MethodModel>();
            this.Operators = new List<OperatorModel>();
            this.Events = new List<EventModel>();
            this.EnumMembers = new List<FieldModel>();

            this.Type = type;
            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

            var typeNode = XmlUtilities.GetMemberNode(xmldoc, this);
            this.Description = XmlUtilities.GetXmlContent(typeNode, "summary");
            this.Remarks = XmlUtilities.GetXmlContent(typeNode, "remarks");
            this.Example = XmlUtilities.GetXmlContent(typeNode, "example");

            this.InheritedTypes = XmlUtilities.GetInheritedTypes(type).ToArray();

            if (type.IsEnum)
            {
                foreach (var value in Enum.GetValues(type))
                {
                    var fi = type.GetField(value.ToString());

                    var fm = new FieldModel(this, fi);
                    var memberNode = XmlUtilities.GetMemberNode(xmldoc, fm);
                    fm.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                    fm.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                    fm.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                    this.EnumMembers.Add(fm);
                }
            }
            else
            {
                var fields = type.GetFields(flags);
                if (fields.Length > 0)
                {
                    foreach (var fi in fields)
                    {
                        if (fi.DeclaringType.Assembly != type.Assembly)
                        {
                            continue;
                        }

                        var fm = new FieldModel(this, fi);
                        var memberNode = XmlUtilities.GetMemberNode(xmldoc, fm);
                        fm.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                        fm.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                        fm.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                        this.Fields.Add(fm);
                    }
                }
            }

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

                    var cm = new ConstructorModel(this, ci);
                    var memberNode = XmlUtilities.GetMemberNode(xmldoc, cm);
                    cm.IsOverloaded = constructors.Length > 1;
                    cm.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                    cm.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                    cm.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                    foreach (var pi in ci.GetParameters())
                    {
                        var pm = new ParameterModel(cm, pi);
                        pm.Description = XmlUtilities.GetParameterDescription(memberNode, pi.Name);
                        cm.Parameters.Add(pm);
                    }

                    this.Constructors.Add(cm);
                }
            }

            var properties = type.GetProperties(flags);
            if (properties.Length > 0)
            {
                foreach (var pi in properties)
                {
                    if (pi.DeclaringType.Assembly != type.Assembly)
                    {
                        continue;
                    }

                    var pm = new PropertyModel(this, pi);
                    var memberNode = XmlUtilities.GetMemberNode(xmldoc, pm);
                    pm.IsOverloaded = properties.Count(p => p.Name == pi.Name) > 1;
                    pm.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                    pm.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                    pm.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                    this.Properties.Add(pm);
                }
            }

            var methods = type.GetMethods(flags);
            foreach (var mi in methods)
            {
                if (mi.DeclaringType.Assembly != type.Assembly)
                {
                    continue;
                }

                if (mi.Name.StartsWith("get_") || mi.Name.StartsWith("set_"))
                {
                    continue;
                }

                if (mi.Name.StartsWith("add_") || mi.Name.StartsWith("remove_"))
                {
                    continue;
                }

                if (mi.Name.StartsWith("op_"))
                {
                    continue;
                }

                var mm = new MethodModel(this, mi);
                var memberNode = XmlUtilities.GetMemberNode(xmldoc, mm);
                mm.IsOverloaded = methods.Count(m => m.Name == mi.Name) > 1;
                mm.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                mm.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                mm.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                foreach (var pi in mi.GetParameters())
                {
                    var pm = new ParameterModel(mm, pi)
                                 {
                                     Description =
                                         XmlUtilities.GetParameterDescription(memberNode, pi.Name)
                                 };
                    mm.Parameters.Add(pm);
                }

                mm.ReturnValueDescription = XmlUtilities.GetXmlContent(memberNode, "returns");

                this.Methods.Add(mm);
            }

            foreach (var mi in methods.Where(m => m.Name.StartsWith("op_")))
            {
                if (mi.DeclaringType.Assembly != type.Assembly)
                {
                    continue;
                }

                var mm = new OperatorModel(this, mi);
                var memberNode = XmlUtilities.GetMemberNode(xmldoc, mm);
                mm.IsOverloaded = methods.Count(m => m.Name == mi.Name) > 1;
                mm.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                mm.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                mm.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                foreach (var pi in mi.GetParameters())
                {
                    var pm = new ParameterModel(mm, pi)
                    {
                        Description =
                            XmlUtilities.GetParameterDescription(memberNode, pi.Name)
                    };
                    mm.Parameters.Add(pm);
                }

                mm.ReturnValueDescription = XmlUtilities.GetXmlContent(memberNode, "returns");
                this.Operators.Add(mm);
            }

            var events = type.GetEvents(flags);
            foreach (var ei in events)
            {
                if (ei.DeclaringType.Assembly != type.Assembly)
                {
                    continue;
                }

                var em = new EventModel(this, ei);
                var memberNode = XmlUtilities.GetMemberNode(xmldoc, em);
                em.Description = XmlUtilities.GetXmlContent(memberNode, "summary");
                em.Remarks = XmlUtilities.GetXmlContent(memberNode, "remarks");
                em.Example = XmlUtilities.GetXmlContent(memberNode, "example");

                this.Events.Add(em);
            }
        }

        public Assembly Assembly
        {
            get
            {
                return this.Type.Assembly;
            }
        }

        public IList<ConstructorModel> Constructors { get; private set; }

        public List<TypeModel> DerivedTypes { get; set; }

        public IList<FieldModel> EnumMembers { get; private set; }

        public IList<EventModel> Events { get; private set; }

        public Type[] InheritedTypes { get; set; }

        public IList<MethodModel> Methods { get; private set; }

        public IList<OperatorModel> Operators { get; private set; }

        public IList<FieldModel> Fields { get; private set; }

        public IList<PropertyModel> Properties { get; private set; }

        public Type Type { get; set; }

        public override string GetXmlMemberName()
        {
            return string.Format("T:{0}", XmlUtilities.GetXmlMemberTypeName(this.Type));
        }

        public override string GetPageTitle()
        {
            return string.Format("{0} ({1})", this.GetTitle(), this.Type.Namespace);
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            XmlUtilities.AppendAttributes(this.Type.GetCustomAttributes(false), sb);
            if (this.Type.IsPublic)
            {
                sb.Append("public ");
            }

            if (this.Type.IsAbstract)
            {
                sb.Append("abstract ");
            }

            if (this.Type.IsClass)
            {
                sb.Append("class ");
            }

            if (this.Type.IsInterface)
            {
                sb.Append("interface ");
            }

            if (this.Type.IsEnum)
            {
                sb.Append("enum ");
            }
            else if (this.Type.IsValueType)
            {
                sb.Append("struct ");
            }

            var niceName = XmlUtilities.GetNiceTypeName(this.Type);
            sb.Append(niceName);

            // Get interfaces implemented in the base types
            var baseAndInterfaces = new List<string>();
            if (this.Type.BaseType != null && this.Type.BaseType != typeof(object) && this.Type.BaseType != typeof(ValueType))
            {
                var name = XmlUtilities.GetNiceTypeName(this.Type.BaseType);
                name = name.Replace(this.Type.Namespace, string.Empty);
                baseAndInterfaces.Add(name);
            }

            // Get the interfaces implemented in the base type
            var baseInterfaces = this.Type.BaseType != null ? this.Type.BaseType.GetInterfaces() : new Type[] { };

            // Add interfaces implemented in this type, except interfaces implemented in the base types
            foreach (var i in this.Type.GetInterfaces().Except(baseInterfaces))
            {
                var name = XmlUtilities.GetNiceTypeName(i);
                name = name.Replace(this.Type.Namespace, string.Empty);
                baseAndInterfaces.Add(name);
            }

            if (baseAndInterfaces.Count > 0)
            {
                sb.Append(" : " + baseAndInterfaces.FormatList(", "));
            }

            return sb.ToString();
        }

        public override string GetTitle()
        {
            if (this.Type.IsClass)
            {
                return string.Format("{0} Class", this);
            }

            if (this.Type.IsInterface)
            {
                return string.Format("{0} Interface", this);
            }

            if (this.Type.IsEnum)
            {
                return string.Format("{0} Enumeration", this);
            }

            if (this.Type.IsValueType)
            {
                return string.Format("{0} Structure", this);
            }

            return this.ToString();
        }

        public override string ToString()
        {
            return XmlUtilities.GetNiceTypeName(this.Type);
        }

        protected override string GetFileNameCore()
        {
            return this.Type.FullName;
        }
    }
}