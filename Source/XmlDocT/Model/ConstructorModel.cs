namespace XmlDocT
{
    using System.Reflection;

    public class ConstructorModel : Content
    {
        public string Parameters { get; set; }

        public ConstructorModel(ConstructorInfo ci)
        {
            this.Info = ci;
        }

        public override string ToString()
        {
            return string.Format("{0}({1})", Utilities.GetNiceMethodName((ConstructorInfo)this.Info), Utilities.GetMethodParameters((ConstructorInfo)this.Info));
        }

        public override string GetTitle()
        {
            return string.Format("{0}.{1} Constructor", Utilities.GetNiceTypeName(this.DeclaringType), this);
        }

        public override string GetFileName()
        {
            return string.Format("{0}.{1}", this.DeclaringType.FullName, this.Info.Name);
        }

    }
}