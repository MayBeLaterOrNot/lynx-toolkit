// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PropertyCodeGenerator.cs" company="Lynx">
//   Copyright © Lynx Toolkit.
// </copyright>
// <summary>
//   The Options interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    /// Generates property code for a class.
    /// </summary>
    public class PropertyCodeGenerator : IPropertyCodeGeneratorOptions
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PropertyCodeGenerator"/> class.
        /// </summary>
        /// <param name="fileName">
        /// Name of the input file.
        /// </param>
        public PropertyCodeGenerator(string fileName)
        {
            this.FileName = fileName;
            this.ClassFileName = Path.ChangeExtension(this.FileName, ".cs");
            this.PropertiesFileName = Path.ChangeExtension(this.FileName, ".Properties.cs");
            this.PropertyClassModel = new PropertyClassModel();
            this.AffectsRenderAttribute = "[AffectsRender]";
            this.PropertySetter = "this.SetValue(ref this.{0}, value, \"{1}\")";
            this.ReferencePropertySetter = "this.SetReference(ref this.{0}, value, \"{1}\")";
            this.RaisePropertyChanged = "this.RaisePropertyChanged(\"{0}\")";
            this.ReferencePropertyType = "Reference<{0}>";
            this.ReferenceResolve = "this.ResolveReference(ref this.{0})";
            this.PropertyChangeCallback = "this.On{0}Changed(oldValue, value)";
            this.ValidateCallback = "this.Validate{0}(value)";
            this.CreateRegions = false;
        }

        /// <summary>
        /// Gets or sets the 'affects render' attribute.
        /// </summary>
        public string AffectsRenderAttribute { get; set; }

        /// <summary>
        /// Gets the name of the class file.
        /// </summary>
        /// <value>The name of the class file.</value>
        public string ClassFileName { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether to create regions.
        /// </summary>
        /// <value><c>true</c> if regions should be created; otherwise, <c>false</c>.</value>
        public bool CreateRegions { get; set; }

        /// <summary>
        /// Gets the name of the input file.
        /// </summary>
        /// <value>The name of the input file.</value>
        public string FileName { get; private set; }

        /// <summary>
        /// Gets or sets the 'open for edit' arguments.
        /// </summary>
        /// <value>The open for edit arguments.</value>
        public string OpenForEditArguments { get; set; }

        /// <summary>
        /// Gets or sets the 'open for edit' executable.
        /// </summary>
        /// <value>The open for edit executable.</value>
        public string OpenForEditExecutable { get; set; }

        /// <summary>
        /// Gets the name of the properties file.
        /// </summary>
        /// <value>The name of the properties file.</value>
        public string PropertiesFileName { get; private set; }

        /// <summary>
        /// Gets or sets the format string for the property change callback statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>Use {0} for the name of the property.</remarks>
        public string PropertyChangeCallback { get; set; }

        /// <summary>
        /// Gets or sets the property class model.
        /// </summary>
        /// <value>The property class model.</value>
        public PropertyClassModel PropertyClassModel { get; set; }

        /// <summary>
        /// Gets or sets the format string for the property setter.
        /// </summary>
        /// <value>The property setter.</value>
        /// <remarks>Use {0} for the name of the backing field.
        /// Use {1} for the name of the property.</remarks>
        public string PropertySetter { get; set; }

        /// <summary>
        /// Gets or sets the format string for the raise property changed statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>Use {0} for the name of the property.</remarks>
        public string RaisePropertyChanged { get; set; }

        /// <summary>
        /// Gets or sets the format string for the reference property setter.
        /// </summary>
        /// <value>The reference property setter.</value>
        /// <remarks>Use {0} for the name of the backing field.
        /// Use {1} for the name of the property.</remarks>
        public string ReferencePropertySetter { get; set; }

        /// <summary>
        /// Gets or sets the reference property type.
        /// </summary>
        /// <value>The type of the reference property.</value>
        public string ReferencePropertyType { get; set; }

        /// <summary>
        /// Gets the format string for the reference resolve statement.
        /// </summary>
        public string ReferenceResolve { get; set; }

        /// <summary>
        /// Gets or sets the format string for the validation callback statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>Use {0} for the name of the property.</remarks>
        public string ValidateCallback { get; set; }

        /// <summary>
        /// Opens the specified file for edit.
        /// </summary>
        /// <param name="filename">
        /// The file name.
        /// </param>
        /// <param name="exe">
        /// The executable.
        /// </param>
        /// <param name="argumentFormatString">
        /// The argument format string. {0} will be replaced by the file name.
        /// </param>
        public static void OpenForEdit(string filename, string exe, string argumentFormatString)
        {
            if (exe == null)
            {
                return;
            }

            var psi = new ProcessStartInfo(exe, string.Format(argumentFormatString, filename))
                {
                   CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden 
                };
            var p = Process.Start(psi);
            p.WaitForExit();
        }

        /// <summary>
        /// Generates the model.
        /// </summary>
        public void GenerateModel()
        {
            this.PropertyClassModel.ParseClass(this.ClassFileName);
            this.PropertyClassModel.Parse(this.FileName, this);
        }

        /// <summary>
        /// Determines whether the output file is up to date.
        /// </summary>
        /// <returns><c>true</c> if the output file is up to date; otherwise, <c>false</c>.</returns>
        public bool IsUpToDate()
        {
            var source = new FileInfo(this.FileName);
            var target = new FileInfo(this.PropertiesFileName);
            if (!target.Exists)
            {
                return false;
            }

            return target.LastWriteTime >= source.LastWriteTime;
        }

        /// <summary>
        /// Saves if modified.
        /// </summary>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise</returns>
        public bool SaveIfModified()
        {
            var output = this.PropertyClassModel.ToString(this);
            if (File.Exists(this.PropertiesFileName))
            {
                var existing = File.ReadAllText(this.PropertiesFileName);
                if (string.Equals(existing, output))
                {
                    return false;
                }

                if (this.OpenForEditExecutable != null)
                {
                    OpenForEdit(this.PropertiesFileName, this.OpenForEditExecutable, this.OpenForEditArguments);
                }
            }

            File.WriteAllText(this.PropertiesFileName, output);
            return true;
        }
    }
}