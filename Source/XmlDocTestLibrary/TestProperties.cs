namespace XmlDocTestLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a test class.
    /// </summary>
    public class TestProperties
    {
        /// <summary>
        /// Gets or sets the property1.
        /// </summary>
        /// <value>
        /// The property1.
        /// </value>
        public int Property1 { get; set; }

        /// <summary>
        /// Gets or sets the property2.
        /// </summary>
        /// <value>
        /// The property2.
        /// </value>
        public int[] Property2 { get; set; }

        /// <summary>
        /// Gets or sets the property3.
        /// </summary>
        /// <value>
        /// The property3.
        /// </value>
        public int[,] Property3 { get; set; }

        /// <summary>
        /// Gets or sets the property3b.
        /// </summary>
        /// <value>
        /// The property3b.
        /// </value>
        public int[][] Property3b { get; set; }

        /// <summary>
        /// Gets or sets the property4.
        /// </summary>
        /// <value>
        /// The property4.
        /// </value>
        public List<int> Property4 { get; set; }

        /// <summary>
        /// Gets or sets the property5.
        /// </summary>
        /// <value>
        /// The property5.
        /// </value>
        public IList<int> Property5 { get; set; }
    }
}