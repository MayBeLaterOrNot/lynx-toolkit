namespace WikiPad
{
    using System.Xml.Serialization;

    public class Variable
    {
        [XmlAttribute]
        public string Key { get; set; }
        [XmlText]
        public string Value { get; set; }
    }
}