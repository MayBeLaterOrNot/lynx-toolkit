namespace XmlDocT
{
    using System.Reflection;
    using System.Text;

    public class PropertyModel : Content
    {
        public PropertyModel(PropertyInfo pi)
        {
            this.Info = pi;
        }

        public override string ToString()
        {
            return this.Name;
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Property", Utilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override string GetPageTitle()
        {
            return string.Format("{0}.{1} ({2})", Utilities.GetNiceTypeName(this.DeclaringType), this, this.DeclaringType.Namespace);
        }

        public override string GetFileName()
        {
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this.Name);
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            var pi = (PropertyInfo)this.Info;

            Utilities.AppendAttributes(pi.GetCustomAttributes(false), sb);
            sb.Append("public ");
            sb.Append(Utilities.GetNiceTypeName(pi.PropertyType));
            sb.Append(" ");
            sb.Append(pi.Name);
            sb.Append(" { ");
            if (pi.CanRead)
                sb.Append("get; ");
            if (pi.CanWrite)
                sb.Append("set; ");
            sb.Append("}");
            return sb.ToString();
        }
    }
}