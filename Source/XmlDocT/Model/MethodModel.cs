namespace XmlDocT
{
    using System.Reflection;
    using System.Text;

    public class MethodModel : Content
    {
        public MethodModel(MethodInfo mi)
        {
            this.Info = mi;
        }

        public override string ToString()
        {
            return Utilities.GetNiceMethodName((MethodBase)this.Info);
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Method", Utilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override string GetFileName()
        {
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this.Name);
        }

        public override string GetSyntax()
        {
            var sb = new StringBuilder();
            var mb = (MethodBase)this.Info;
            if (mb.IsPublic) sb.Append("public ");
            if (mb.IsPrivate) sb.Append("private ");
            if (mb.IsVirtual) sb.Append("virtual ");
            if (!mb.IsConstructor)
            {
                var mi = (MethodInfo)this.Info;
                sb.Append(Utilities.GetNiceTypeName(mi.ReturnType));
                sb.Append(" ");
            }
            sb.Append(mb.Name);
            sb.Append("(");
            sb.Append(Utilities.GetMethodParameters(mb, true));
            sb.Append(");");
            return sb.ToString();
        }

    }
}