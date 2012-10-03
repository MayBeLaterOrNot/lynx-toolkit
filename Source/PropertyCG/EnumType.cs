namespace PropertyCG
{
    using System.Collections.Generic;

    public class EnumType
    {
        public string Name { get; set; }

        public IList<string> Values { get; private set; }

        public EnumType(string name, string values)
        {
            this.Name = name;
            this.Values = new List<string>(values.Split(','));
        }
    }
}