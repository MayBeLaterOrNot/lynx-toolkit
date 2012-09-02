//-----------------------------------------------------------------------
// <copyright file="ExtensionMethods.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CleanSource
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Provides extension methods.
    /// </summary>
    internal static class ExtensionMethods
    {
        /// <summary>
        /// Performs the specified action on a sequence.
        /// </summary>
        /// <typeparam name="T">The type of the elements of source.</typeparam>
        /// <param name="source">The IEnumerable to invoke the action on.</param>
        /// <param name="action">The action.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
            }
        }
    }
}