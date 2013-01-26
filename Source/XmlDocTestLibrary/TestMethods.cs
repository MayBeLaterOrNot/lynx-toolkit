// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestMethods.cs" company="Lynx Toolkit">
//   The MIT License (MIT)
//   
//   Copyright (c) 2012 Oystein Bjorke
//   
//   Permission is hereby granted, free of charge, to any person obtaining a
//   copy of this software and associated documentation files (the
//   "Software"), to deal in the Software without restriction, including
//   without limitation the rights to use, copy, modify, merge, publish,
//   distribute, sublicense, and/or sell copies of the Software, and to
//   permit persons to whom the Software is furnished to do so, subject to
//   the following conditions:
//   
//   The above copyright notice and this permission notice shall be included
//   in all copies or substantial portions of the Software.
//   
//   THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
//   OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//   MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//   IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
//   CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
//   TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
//   SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
// <summary>
//   Represents test methods.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
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