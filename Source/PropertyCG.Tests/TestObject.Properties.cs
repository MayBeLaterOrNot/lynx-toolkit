namespace PropertyCG.Tests
{
    using System;

    /// <summary>
    /// Defines the properties for the TestObject class. This file is auto-generated.
    /// </summary>
    public partial class TestObject
    {
        /// <summary>
        /// The backing field for the Guid property.
        /// </summary>
        private Guid guid;

        /// <summary>
        /// The backing field for the Name property.
        /// </summary>
        private string name;

        /// <summary>
        /// The backing field for the Color property.
        /// </summary>
        private Color color;

        /// <summary>
        /// The backing field for the ReferencedObject property.
        /// </summary>
        private Reference<TestObject> referencedObject;

        /// <summary>
        /// Gets or sets the guid.
        /// </summary>
        public Guid Guid
        {
            get
            {
                return this.guid;
            }

            set
            {
                this.SetValue(ref this.guid, value, "Guid");
            }
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }

            set
            {
                this.SetValue(ref this.name, value, "Name");
            }
        }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public Color Color
        {
            get
            {
                return this.color;
            }

            set
            {
                this.SetValue(ref this.color, value, "Color");
            }
        }

        /// <summary>
        /// Gets or sets the referenced object.
        /// </summary>
        public Reference<TestObject> ReferencedObject
        {
            get
            {
                this.ResolveReference(ref this.referencedObject);
                return this.referencedObject;
            }

            set
            {
                this.SetReference(ref this.referencedObject, value, "ReferencedObject");
            }
        }
    }
}
