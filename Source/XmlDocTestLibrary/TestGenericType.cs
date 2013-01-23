namespace XmlDocTestLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a generic test class.
    /// </summary>
    /// <typeparam name="T">The type.</typeparam>
    public class TestGenericType<T>
    {
        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>The value.</value>
        public T Value { get; set; }

        /// <summary>
        /// Gets or sets the value array.
        /// </summary>
        /// <value>The value array.</value>
        public T[] ValueArray { get; set; }

        /// <summary>
        /// Gets or sets the values of type <typeparamref name="T"/>.
        /// </summary>
        /// <value>The values.</value>
        public List<T> Values { get; set; }

        /// <summary>
        /// Method8s the specified numbers.
        /// </summary>
        /// <typeparam name="T1">The type of the numbers.</typeparam>
        /// <param name="numbers">The numbers.</param>
        /// <returns>
        /// The numbers.
        /// </returns>
        public List<T1> Method8<T1>(List<T1> numbers) { return null; }
    }
}