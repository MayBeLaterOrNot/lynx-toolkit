namespace XmlDocT
{
    using System;
    using System.Reflection;

    public abstract class Content
    {
        public string Description { get; set; }
        public string Remarks { get; set; }
        public string Example { get; set; }

        public MemberInfo Info { get; set; }

        public Type DeclaringType { get { return Info.DeclaringType; } }
        public string Name { get { return Info.Name; } }

        public virtual string GetTitle()
        {
            return null;
        }

        public virtual string GetPageTitle()
        {
            return this.GetTitle();
        }

        public virtual string GetFileName()
        {
            return null;
        }
        public virtual string GetSyntax()
        {
            return null;
        }
    }
}