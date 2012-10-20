namespace LynxToolkit
{
    using System;
    using System.Diagnostics;
    using System.IO;

    public static class Utilities
    {
        /// <summary>
        /// Determines whether the specified file should be excluded.
        /// </summary>
        /// <param name="excludedItems">The excluded items.</param>
        /// <param name="path">The path.</param>
        /// <returns><c>true</c> if the specified excluded items is excluded; otherwise, <c>false</c>.</returns>
        public static bool IsExcluded(string excludedItems, string path)
        {
            var name = Path.GetFileName(path);
            if (name == null)
            {
                return true;
            }

            foreach (var item in excludedItems.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(item))
                {
                    continue;
                }

                if (item.StartsWith("*"))
                {
                    var pattern = item.TrimStart('*');
                    if (name.EndsWith(pattern))
                    {
                        return true;
                    }
                }

                if (item.EndsWith("*"))
                {
                    var pattern = item.TrimEnd('*');
                    if (name.StartsWith(pattern))
                    {
                        return true;
                    }
                }

                if (string.Equals(name, item, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Opens the specified file for edit.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <param name="exe">The executable.</param>
        /// <param name="argumentFormatString">The argument format string.</param>
        public static void OpenForEdit(string filename, string exe, string argumentFormatString)
        {
            if (exe == null)
            {
                return;
            }

            var psi = new ProcessStartInfo(exe, string.Format(argumentFormatString, filename)) { CreateNoWindow = true, WindowStyle = ProcessWindowStyle.Hidden };
            var p = Process.Start(psi);
            p.WaitForExit();
        }
    }
}