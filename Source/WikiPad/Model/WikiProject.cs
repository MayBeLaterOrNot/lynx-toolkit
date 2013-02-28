namespace WikiPad
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Serialization;

    [XmlInclude(typeof(Variable))]
    [XmlRoot]
    public class WikiProject
    {
        public WikiProject()
        {
            this.Documents = new ObservableCollection<WikiDocument>();
            this.Variables = new List<Variable>();
            this.Defines = new List<string>();
        }

        public static WikiProject Load(string filePath)
        {
            var s = new XmlSerializer(typeof(WikiProject));
            using (var r = XmlReader.Create(filePath))
            {
                var project = (WikiProject)s.Deserialize(r);
                project.FilePath = filePath;
                project.LoadDocuments();
                return project;
            }
        }

        private void LoadDocuments()
        {
            var dir = Path.GetDirectoryName(this.FilePath);
            var input = Path.Combine(dir, this.Input);
            var inputdir = Path.GetDirectoryName(input);
            var inputpattern = Path.GetFileName(input);
            foreach (var filePath in System.IO.Directory.GetFiles(inputdir, inputpattern))
            {
                var doc = new WikiDocument(filePath);
                this.Documents.Add(doc);
            }
        }

        [XmlIgnore]
        public string FilePath { get; set; }

        [XmlIgnore]
        public string FileName
        {
            get
            {
                return Path.GetFileName(this.FilePath);
            }
        }

        [XmlIgnore]
        public string Directory
        {
            get
            {
                return Path.GetDirectoryName(this.FilePath);
            }
        }

        public string Format { get; set; }
        public string Template { get; set; }
        public string LocalLinks { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        public string Scc { get; set; }

        [XmlArrayItem("Variable")]
        public List<Variable> Variables { get; set; }

        [XmlArrayItem("Define")]
        public List<string> Defines { get; set; }

        public Dictionary<string, string> GetVariables()
        {
            return this.Variables.ToDictionary(r => r.Key, r => r.Value);
        }

        public HashSet<string> GetDefines()
        {
            return new HashSet<string>(this.Defines);
        }

        [XmlIgnore]
        public ObservableCollection<WikiDocument> Documents { get; private set; }

        public string DefaultSyntax { get; set; }
    }
}