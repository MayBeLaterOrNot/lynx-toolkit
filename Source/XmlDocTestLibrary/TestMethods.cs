namespace XmlDocTestLibrary
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents test methods.
    /// </summary>
    public class TestMethods
    {
        /// <summary>
        /// Method1s the specified <paramref name="number"/>.
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
        /// Method8s the specified <paramref name="numbers"/> of type <typeparamref name="T1"/>.
        /// </summary>
        /// <typeparam name="T1">The type.</typeparam>
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
        public List<T2> Method8<T1, T2>(List<T1> numbers) { return null; }
    }
}