namespace CleanProjects
{
    using System;
    using System.IO;
    using System.Linq;

    using LynxToolkit;

    /// <summary>
    /// The 'CleanProjects' program.
    /// </summary>
    public static class Program
    {
        private static long totalSize;

        static void Main(string[] args)
        {
            Console.WriteLine(Utilities.ApplicationHeader);
            var rootDirectory = args.Length > 0 ? args[0] : ".";
            Clean(rootDirectory);
            Console.WriteLine();
            Console.WriteLine("{0} MB deleted", totalSize / 1024 / 1024);
        }

        private static void Clean(string directory)
        {
            var name = Path.GetFileName(directory);
            if (name == null)
            {
                return;
            }

            if (string.Equals(name, "bin", StringComparison.InvariantCultureIgnoreCase))
            {
                Remove(directory);
                return;
            }

            if (string.Equals(name, "obj", StringComparison.InvariantCultureIgnoreCase))
            {
                Remove(directory);
                return;
            }

            if (string.Equals(name, "packages", StringComparison.InvariantCulture))
            {
                Remove(directory);
                return;
            }

            if (name.StartsWith("_ReSharper", StringComparison.InvariantCultureIgnoreCase))
            {
                Remove(directory);
                return;
            }

            foreach (var subDirectory in Directory.GetDirectories(directory))
            {
                Clean(subDirectory);
            }
        }

        private static void Remove(string directory)
        {
            Console.WriteLine(directory);
            var di = new DirectoryInfo(directory);
            totalSize += di.EnumerateFiles("*", SearchOption.AllDirectories).Sum(x => x.Length);
            try
            {
                // Delete recursively
                Directory.Delete(directory, true);
            }
            catch
            {
                Console.WriteLine("  Could not be deleted.");
            }
        }
    }
}
