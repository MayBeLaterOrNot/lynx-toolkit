//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace CleanSource
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// The program.
    /// </summary>
    /// <remarks>
    /// Cleaning up for a Resharper (I think) code-cleanup bug.
    /// </remarks>
    public static class Program
    {
        /// <summary>
        /// The constructor summary substring to search for.
        /// </summary>
        private const string ConstructorSummarySubString = "/// Initializes a new instance of the";

        /// <summary>
        /// The number of files cleaned.
        /// </summary>
        private static int filesCleaned;

        /// <summary>
        /// The number of files scanned.
        /// </summary>
        private static int fileCount;

        /// <summary>
        /// The main entry point of the program.
        /// </summary>
        /// <param name="args">The args.</param>
        private static void Main(string[] args)
        {
            args.ForEach(Scan);
            Console.WriteLine("{0} files cleaned (of {1})", filesCleaned, fileCount);
        }

        /// <summary>
        /// Scans the specified directory.
        /// </summary>
        /// <param name="directory">The directory.</param>
        private static void Scan(string directory)
        {
            Directory.GetDirectories(directory).ForEach(Scan);
            Directory.GetFiles(directory, "*.cs").ForEach(Clean);
        }

        /// <summary>
        /// Cleans the specified file.
        /// </summary>
        /// <param name="file">The file.</param>
        private static void Clean(string file)
        {
            fileCount++;
            var input = File.ReadAllLines(file);
            var output = new List<string>();
            bool modified = false;
            for (int i = 0; i < input.Length; i++)
            {
                bool skip = false;

                // Remove duplicate lines containing "/// Initializes a new instance of the"
                if (i > 0 && input[i - 1].Contains(ConstructorSummarySubString) && input[i].Contains(ConstructorSummarySubString))
                {
                    modified = true;
                    skip = true;
                }

                if (skip)
                {
                    continue;
                }

                output.Add(input[i]);
            }

            if (modified)
            {
                Console.WriteLine(file);
                File.WriteAllLines(file, output);
                filesCleaned++;
            }
        }
    }
}
