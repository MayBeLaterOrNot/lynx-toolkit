// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx Toolkit">
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
//   The main program.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace PropertyCG
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;

    /// <summary>
    /// The main program.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Reviewed. Suppression is OK here.")]
    public class Program
    {
        private static int filesFound;
        private static int filesChanged;
        private static int projectsFound;
        private static int projectsChanged;

        public static string OpenForEditExecutable { get; set; }
        
        public static string OpenForEditArguments { get; set; }
        
        public static string SearchPattern { get; set; }
        
        public static string Folder { get; set; }
        
        public static bool Force { get; set; }

        /// <summary>
        /// The main program.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        public static void Main(string[] args)
        {
            var stopwatch = Stopwatch.StartNew();

            Console.WriteLine(LynxToolkit.Utilities.ApplicationHeader);

            SearchPattern = "*.oml";
            Folder = ".";
            string scc = null;
            foreach (var arg in args)
            {
                if (arg.Contains("*"))
                {
                    SearchPattern = arg;
                    continue;
                }

                var argx = arg.Split('=');
                switch (argx[0].ToLower())
                {
                    case "/scc":
                        scc = argx[1];
                        continue;
                    case "/type":
                        SearchPattern = "*." + argx[1];
                        continue;
                    case "/f":
                        Force = true;
                        continue;
                }

                Folder = arg;
            }

            if (string.Equals(scc, "p4", StringComparison.InvariantCultureIgnoreCase))
            {
                OpenForEditExecutable = "p4.exe";
                OpenForEditArguments = "edit {0}";
            }

            if (!Folder.EndsWith("\\"))
            {
                Folder += "\\";
            }

            Console.WriteLine("  Searching for {0} files in {1} and sub-folders.", SearchPattern, Path.GetFullPath(Folder));
            Console.WriteLine();

            Search(Folder, SearchPattern, Process);
            Search(Folder, "*.csproj", ProcessProject);
            if (filesChanged + projectsChanged > 0)
            {
                Console.WriteLine();
            }

            Console.WriteLine("  {0} {1} files found.", filesFound, SearchPattern.Replace("*", string.Empty));
            Console.WriteLine("  {0} cs files changed.", filesChanged);
            Console.WriteLine("  {0} project files found.", projectsFound);
            Console.WriteLine("  {0} projects changed.", projectsChanged);
            Console.WriteLine();
            Console.WriteLine("  Finished in {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine();
        }

        private static void Search(string folder, string searchPattern, Action<string> process)
        {
            foreach (var file in Directory.GetFiles(folder, searchPattern))
            {
                process(file);
            }

            foreach (var dir in Directory.GetDirectories(folder))
            {
                Search(dir, searchPattern, process);
            }
        }

        private static void Process(string file)
        {
            filesFound++;
            var pcg = new PropertyCodeGenerator(file) { OpenForEditExecutable = OpenForEditExecutable, OpenForEditArguments = OpenForEditArguments };
            if (pcg.IsUpToDate() && !Force)
            {
                return;
            }

            pcg.GenerateModel();
            if (pcg.SaveIfModified())
            {
                Console.WriteLine("  " + pcg.PropertiesFileName.Replace(Folder, string.Empty));
                filesChanged++;
            }
        }

        private static void ProcessProject(string file)
        {
            projectsFound++;
            var ext = Path.GetExtension(SearchPattern);
            var pcg = new ProjectUpdater(file, ext) { OpenForEditExecutable = OpenForEditExecutable, OpenForEditArguments = OpenForEditArguments };
            if (pcg.Update())
            {
                Console.WriteLine("  " + file.Replace(Folder, string.Empty));
                projectsChanged++;
            }
        }
    }
}