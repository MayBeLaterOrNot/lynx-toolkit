//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="LynxToolkit">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace ReplaceTool
{
    using System;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    ///   The main program.
    /// </summary>
    public class Program
    {
        #region Constants and Fields

        /// <summary>
        ///   Valid text file types.
        /// </summary>
        private const string ValidTextFileTypes = ".cs .xml .xaml .sln .csproj .DotSettings .user .StyleCop .txt";

        /// <summary>
        ///   The expression.
        /// </summary>
        private static Regex expression;

        /// <summary>
        ///   The replacement string.
        /// </summary>
        private static string replacement;

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The main program.
        /// </summary>
        /// <param name="args">
        /// The args. 
        /// </param>
        public static void Main(string[] args)
        {
            Console.WriteLine("ReplaceTool [pattern] [replacement] [startdirectory]");
            Console.WriteLine("version " + Assembly.GetEntryAssembly().GetName().Version);
            Console.WriteLine();
            Console.WriteLine("Replaces text in directory names, file names and text files.");
            Console.WriteLine();

            if (args.Length < 2)
            {
                return;
            }

            var pattern = args[0];
            replacement = args[1];
            var source = args.Length > 2 ? args[2] : ".";

            expression = new Regex(pattern, RegexOptions.Compiled);

            Console.WriteLine("Search pattern: \"{0}\"", pattern);
            Console.WriteLine("Replace by: \"{0}\"", replacement);
            Console.WriteLine("Start in: \"{0}\"", source);
            Console.WriteLine();

            Search(source);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether the specified path is a textfile .
        /// </summary>
        /// <param name="path">
        /// The path. 
        /// </param>
        /// <returns>
        /// <c>true</c> if the specified path is text; otherwise, <c>false</c> . 
        /// </returns>
        /// <remarks>
        /// See http://stackoverflow.com/questions/910873/how-can-i-determine-if-a-file-is-binary-or-text-in-c
        /// </remarks>
        private static bool IsText(string path)
        {
            Encoding e;
            return IsText(out e, path, 1024);
        }

        /// <summary>
        /// Detect if a file is text and detect the encoding.
        /// </summary>
        /// <param name="encoding">
        /// The detected encoding. 
        /// </param>
        /// <param name="fileName">
        /// The file name. 
        /// </param>
        /// <param name="windowSize">
        /// The number of characters to use for testing. 
        /// </param>
        /// <returns>
        /// true if the file is text. 
        /// </returns>
        private static bool IsText(out Encoding encoding, string fileName, int windowSize)
        {
            using (var fileStream = File.OpenRead(fileName))
            {
                var rawData = new byte[windowSize];
                var text = new char[windowSize];
                var isText = true;

                // Read raw bytes
                var rawLength = fileStream.Read(rawData, 0, rawData.Length);
                fileStream.Seek(0, SeekOrigin.Begin);

                // Detect encoding correctly (from Rick Strahl's blog)
                // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
                if (rawData[0] == 0xef && rawData[1] == 0xbb && rawData[2] == 0xbf)
                {
                    encoding = Encoding.UTF8;
                }
                else if (rawData[0] == 0xfe && rawData[1] == 0xff)
                {
                    encoding = Encoding.Unicode;
                }
                else if (rawData[0] == 0 && rawData[1] == 0 && rawData[2] == 0xfe && rawData[3] == 0xff)
                {
                    encoding = Encoding.UTF32;
                }
                else if (rawData[0] == 0x2b && rawData[1] == 0x2f && rawData[2] == 0x76)
                {
                    encoding = Encoding.UTF7;
                }
                else
                {
                    encoding = Encoding.Default;
                }

                // Read text and detect the encoding
                using (var streamReader = new StreamReader(fileStream))
                {
                    streamReader.Read(text, 0, text.Length);
                }

                using (var memoryStream = new MemoryStream())
                {
                    using (var streamWriter = new StreamWriter(memoryStream, encoding))
                    {
                        // Write the text to a buffer
                        streamWriter.Write(text);
                        streamWriter.Flush();

                        // Get the buffer from the memory stream for comparision
                        var memoryBuffer = memoryStream.GetBuffer();

                        // Compare only bytes read
                        for (var i = 0; i < rawLength && isText; i++)
                        {
                            isText = rawData[i] == memoryBuffer[i];
                        }
                    }
                }

                return isText;
            }
        }

        /// <summary>
        /// Searches the specified folder.
        /// </summary>
        /// <param name="folder">
        /// The folder. 
        /// </param>
        private static void Search(string folder)
        {
            // Rename folder
            var destFolder = expression.Replace(folder, replacement);
            if (folder != destFolder)
            {
                Console.WriteLine("Rename folder to {0}", destFolder);
                Directory.Move(folder, destFolder);
                folder = destFolder;
            }

            foreach (var path in Directory.GetFiles(folder, "*.*"))
            {
                var ext = (Path.GetExtension(path) ?? string.Empty).ToLower();
                var filename = Path.GetFileName(path) ?? string.Empty;
                var newFilename = expression.Replace(filename, replacement);
                var dir = Path.GetDirectoryName(path) ?? string.Empty;

                var newPath = Path.Combine(dir, newFilename);
                if (newPath != path)
                {
                    // Rename file
                    Console.WriteLine("Rename file to {0}", newPath);
                    File.Move(path, newPath);
                }

                if (ValidTextFileTypes.Contains(ext) || IsText(path))
                {
                    // Replace in file
                    var content = File.ReadAllText(newPath);
                    var newContent = expression.Replace(content, replacement);
                    if (content != newContent)
                    {
                        Console.WriteLine("Modified {0}", newPath);
                        File.WriteAllText(newPath, newContent);
                    }
                }
            }

            // Search in sub-folders
            foreach (var d in Directory.GetDirectories(folder))
            {
                Search(d);
            }
        }

        #endregion
    }
}