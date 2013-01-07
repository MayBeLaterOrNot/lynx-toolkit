namespace XmlDocTestLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a test class.
    /// </summary>
    public class TestClass
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

        /// <summary>
        /// Method1s the specified number.
        /// </summary>
        /// <param name="number">The number.</param>
        /// <returns>A number.</returns>
        public int Method1(int number) { return 0; }

        /// <summary>
        /// Method2s the specified numbers.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>Numbers.</returns>
        public int[] Method2(int[] numbers) { return null; }

        /// <summary>
        /// Method3s the specified numbers.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>Numbers.</returns>
        public int[,] Method3(int[,] numbers) { return null; }

        /// <summary>
        /// Method3bs the specified numbers.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>Numbers.</returns>
        public int[][] Method3b(int[][] numbers) { return null; }

        /// <summary>
        /// Method4s the specified numbers.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>Numbers.</returns>
        public List<int> Method4(List<int> numbers) { return null; }

        /// <summary>
        /// Method5s the specified numbers.
        /// </summary>
        /// <param name="numbers">The numbers.</param>
        /// <returns>Numbers.</returns>
        public IList<int> Method5(IList<int> numbers) { return null; }

        /// <summary>
        /// Method6s the specified number.
        /// </summary>
        /// <param name="number">The number.</param>
        public void Method6(out int number) { number = 0; }

        /// <summary>
        /// Method7s the specified number.
        /// </summary>
        /// <param name="number">The number.</param>
        public void Method7(ref int number) { number = 0; }

        /// <summary>
        /// Method8s the specified numbers.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The numbers.</returns>
        public List<T1> Method8<T1>(List<T1> numbers) { return null; }

        /// <summary>
        /// Method8s the specified numbers.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The numbers</returns>
        public List<T2> Method8<T1,T2>(List<T1> numbers) { return null; }
    }

    public class TestClass<T>
    {
        /// <summary>
        /// Method8s the specified numbers.
        /// </summary>
        /// <typeparam name="T">The type.</typeparam>
        /// <param name="numbers">The numbers.</param>
        /// <returns>The numbers.</returns>
        public List<T> Method8<T>(List<T> numbers) { return null; }

    }
}