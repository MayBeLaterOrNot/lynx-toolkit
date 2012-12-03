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
        /// The backing field for the Included property.
        /// </summary>
        private bool included;

        /// <summary>
        /// The backing field for the Length property.
        /// </summary>
        private double length;

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
        /// Gets or sets a value indicating whether the included should be on or off.
        /// </summary>
        public bool Included
        {
            get
            {
                return this.included;
            }

            set
            {
                var oldValue = this.included;
                if (this.SetValue(ref this.included, value, "Included", PropertyChangedFlags.AffectsResults))
                {
                    this.OnIncludedChanged(oldValue, value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the length.
        /// </summary>
        public double Length
        {
            get
            {
                return this.length;
            }

            set
            {
                this.SetValue(ref this.length, value, "Length", PropertyChangedFlags.AffectsRender | PropertyChangedFlags.AffectsResults);
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
                this.SetValue(ref this.color, value, "Color", PropertyChangedFlags.AffectsRender);
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
