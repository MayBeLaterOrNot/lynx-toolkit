namespace XmlDocT
{
    using System.Collections.Generic;

    public class NamespaceModel : Content
    {
        public new string Name { get; set; }

        public IList<TypeModel> Types { get; private set; }

        public override string GetTitle()
        {
            return this.Name + " Namespace";
        }
        public override string GetFileName()
        {
            return this.Name;
        }

        public NamespaceModel()
        {
            this.Types = new List<TypeModel>();
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}