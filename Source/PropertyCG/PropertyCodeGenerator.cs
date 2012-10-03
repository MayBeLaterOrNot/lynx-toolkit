namespace PropertyCG
{
    using System.Diagnostics;
    using System.IO;

    public interface IOptions
    {
        string AffectsRenderAttribute { get; }
        bool UseExpressions { get; }
        bool CreateRegions { get; }
    }

    public class PropertyCodeGenerator : IOptions
    {
        public string FileName { get; private set; }
        public string PropertiesFileName { get; private set; }
        public string ClassFileName { get; private set; }

        public string AffectsRenderAttribute { get; set; }
        public bool UseExpressions { get; set; }
        public bool CreateRegions { get; set; }

        public string OpenForEditExecutable { get; set; }
        public string OpenForEditArguments { get; set; }
        public PropertyClassModel PropertyClassModel { get; set; }

        public PropertyCodeGenerator(string fileName)
        {
            this.FileName = fileName;
            this.ClassFileName = Path.ChangeExtension(this.FileName, ".cs");
            this.PropertiesFileName = Path.ChangeExtension(this.FileName, ".Properties.cs");
            this.PropertyClassModel = new PropertyClassModel();
            this.AffectsRenderAttribute = "AffectsRender";
            this.UseExpressions = false;
            this.CreateRegions = false;
        }

        public void Generate()
        {
            this.PropertyClassModel.ReadClass(ClassFileName, PropertiesFileName);
            this.PropertyClassModel.Parse(FileName, this);
        }

        public bool SaveIfModified()
        {
            var output = this.PropertyClassModel.ToString(this);
            if (File.Exists(PropertiesFileName))
            {
                var existing = File.ReadAllText(PropertiesFileName);
                if (string.Equals(existing, output)) return false;
                if (OpenForEditExecutable != null) OpenForEdit(PropertiesFileName, OpenForEditExecutable, OpenForEditArguments);
            }
            File.WriteAllText(PropertiesFileName, output);
            return true;
        }

        public static void OpenForEdit(string filename, string exe, string argumentFormatString)
        {
            if (exe == null) return;
            var psi = new ProcessStartInfo(exe, string.Format(argumentFormatString, filename)) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        public bool IsUpToDate()
        {
            var source = new FileInfo(this.FileName);
            var target = new FileInfo(this.PropertiesFileName);
            if (!target.Exists) return false;
            return target.LastWriteTime >= source.LastWriteTime;
        }
    }
}