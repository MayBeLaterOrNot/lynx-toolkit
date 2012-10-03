namespace PropertyCG
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Text;

    public class Property
    {
        public bool IsReference { get; set; }
        public string Type { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }

        public IList<string> Dependencies { get; private set; }
        public IList<string> Attributes { get; private set; }

        public bool ReadOnly { get; set; }
        public bool PropertyChangeCallback { get; set; }
        public bool ValidateCallback { get; set; }

        /// <summary>
        /// Gets the name of the backing field.
        /// </summary>
        /// <value>The name of the backing field.</value>
        public string BackingFieldName
        {
            get
            {
                return this.Name[0].ToString(CultureInfo.InvariantCulture).ToLower() + this.Name.Substring(1);
            }
        }

        /// <summary>
        /// Gets the descriptive name of a property.
        /// </summary>
        /// <param name="propertyName">
        /// Name of the property. 
        /// </param>
        /// <returns>
        /// The description. 
        /// </returns>
        private string GetDescription(string propertyName)
        {
            var result = new StringBuilder();
            for (int i = 0; i < propertyName.Length; i++)
            {
                if (i == 0)
                {
                    result.Append(this.ToLower(propertyName[i]));
                    continue;
                }

                if (this.IsLower(propertyName[i - 1]) && !this.IsLower(propertyName[i])
                    && (i + 1 < propertyName.Length && this.IsLower(propertyName[i + 1])))
                {
                    result.Append(" ");
                    result.Append(this.ToLower(propertyName[i]));
                    continue;
                }

                result.Append(propertyName[i]);
            }

            return result.ToString();
        }

        /// <summary>
        /// Determines whether the specified character is lowercase.
        /// </summary>
        /// <param name="p0">
        /// The input character. 
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified character is lowercase; otherwise, <c>false</c> . 
        /// </returns>
        private bool IsLower(char p0)
        {
            return p0.ToString(CultureInfo.InvariantCulture).ToLower()[0] == p0;
        }

        /// <summary>
        /// Converts a character to lower case.
        /// </summary>
        /// <param name="p0">
        /// The input character. 
        /// </param>
        /// <returns>
        /// The converted character. 
        /// </returns>
        private char ToLower(char p0)
        {
            return p0.ToString(CultureInfo.InvariantCulture).ToLower()[0];
        }
        public string Summary
        {
            get
            {
                string description = string.IsNullOrEmpty(this.Description) ? this.GetDescription(this.Name) : this.Description;
                string longDescription;
                if (this.Type == "bool")
                {
                    longDescription = "a value indicating whether the " + description + " should be on or off";
                }
                else
                {
                    longDescription = "the " + description;
                }

                return string.Format(this.ReadOnly ? "Gets {0}." : "Gets or sets {0}.", longDescription);
            }
        }

        public Property(string type, string name, IEnumerable<string> attributes, string description)
        {
            this.Type = type;
            this.Name = name;
            this.Description = description;
            this.Dependencies = new List<string>();
            this.Attributes = new List<string>(attributes);
        }
    }
}