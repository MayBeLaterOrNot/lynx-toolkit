//-----------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
//     Copyright © LynxToolkit. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace PropertyCG
{
    using System;
    using System.Diagnostics;
    using System.IO;

    /// <summary>
    ///   The main program.
    /// </summary>
    public class Program
    {
        #region Public Methods and Operators

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

            Console.WriteLine(LynxToolkit.Application.Header);

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

            Console.WriteLine("  Searching for {0} files in {1} and sub-folders.", SearchPattern, Folder);
            Console.WriteLine();

            Search(Folder, SearchPattern, Process);
            Search(Folder, "*.csproj", ProcessProject);
            if (filesChanged + projectsChanged > 0)
            {
                Console.WriteLine();
            }

            Console.WriteLine("  {0} files changed.", filesChanged);
            Console.WriteLine("  {0} projects changed.", projectsChanged);
            Console.WriteLine();
            Console.WriteLine("  Finished in {0} ms", stopwatch.ElapsedMilliseconds);
            Console.WriteLine();
        }

        private static int filesChanged = 0;
        private static int projectsChanged = 0;

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
            var pcg = new PropertyCodeGenerator(file) { OpenForEditExecutable = OpenForEditExecutable, OpenForEditArguments = OpenForEditArguments };
            if (pcg.IsUpToDate() && !Force)
            {
                return;
            }

            pcg.Generate();
            if (pcg.SaveIfModified())
            {
                Console.WriteLine("  " + pcg.PropertiesFileName.Replace(Folder, string.Empty));
                filesChanged++;
            }
        }

        private static void ProcessProject(string file)
        {
            var ext = Path.GetExtension(SearchPattern);
            var pcg = new ProjectUpdater(file, ext) { OpenForEditExecutable = OpenForEditExecutable, OpenForEditArguments = OpenForEditArguments };
            if (pcg.Update())
            {
                Console.WriteLine("  " + file.Replace(Folder, string.Empty));
                projectsChanged++;
            }
        }

        #endregion

    }
}