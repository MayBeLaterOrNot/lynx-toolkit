namespace PropertyCG
{
    /// <summary>
    /// Provides preferences for the property code generation.
    /// </summary>
    public interface IPropertyCodeGeneratorOptions
    {
        /// <summary>
        /// Gets the "affects render" attribute.
        /// </summary>
        string AffectsRenderAttribute { get; }

        /// <summary>
        /// Gets a value indicating whether to create regions.
        /// </summary>
        bool CreateRegions { get; }

        /// <summary>
        /// Gets the format string for the property setter.
        /// </summary>
        /// <remarks>
        /// Use {0} for the name of the backing field.
        /// Use {1} for the name of the property.
        /// </remarks>
        string PropertySetter { get; }

        /// <summary>
        /// Gets the format string for the reference property setter.
        /// </summary>
        /// <remarks>
        /// Use {0} for the name of the backing field.
        /// Use {1} for the name of the property.
        /// </remarks>
        string ReferencePropertySetter { get; }

        /// <summary>
        /// Gets the reference property type.
        /// </summary>
        string ReferencePropertyType { get; }

        /// <summary>
        /// Gets the format string for the raise property changed statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>
        /// Use {0} for the name of the property.
        /// </remarks>
        string RaisePropertyChanged { get; }

        /// <summary>
        /// Gets the format string for the property change callback statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>
        /// Use {0} for the name of the property.
        /// </remarks>
        string PropertyChangeCallback { get; }

        /// <summary>
        /// Gets the format string for the validation callback statement.
        /// </summary>
        /// <value>The format string.</value>
        /// <remarks>
        /// Use {0} for the name of the property.
        /// </remarks>
        string ValidateCallback { get; }

        /// <summary>
        /// Gets the name of the properties file.
        /// </summary>
        /// <value>The name of the properties file.</value>
        string PropertiesFileName { get; }

        /// <summary>
        /// Gets the format string for the reference resolve statement.
        /// </summary>
        string ReferenceResolve { get; }
    }
}