namespace WikiPad
{
    using System.IO;
    using System.Text;

    using LynxToolkit;

    public class WikiDocument : Observable
    {
        private string content;

        private bool isModified;

        public WikiDocument(string fullPath)
        {
            this.FullPath = fullPath;
            this.Content = File.ReadAllText(this.FullPath);
            this.IsModified = false;
        }

        public string FullPath { get; set; }

        public string FileName
        {
            get
            {
                return Path.GetFileName(this.FullPath);
            }
        }
        public override string ToString()
        {
            return FileName;
        }
        public bool IsModified
        {
            get
            {
                return this.isModified;
            }
            set
            {
                this.SetValue(ref isModified, value, "IsModified");
            }
        }

        public string Content
        {
            get
            {
                return this.content;
            }

            set
            {
                if (this.SetValue(ref this.content, value, "Content"))
                {
                    this.IsModified = true;
                }
            }
        }

        public void Revert()
        {
            this.Content = File.ReadAllText(this.FullPath);
            this.IsModified = false;
        }

        public void Save(string scc = null)
        {
            if (scc != null)
            {
                Utilities.OpenForEdit(this.FullPath, scc);
            }

            File.WriteAllText(this.FullPath, this.Content, Encoding.UTF8);
            this.IsModified = false;
        }
    }
}