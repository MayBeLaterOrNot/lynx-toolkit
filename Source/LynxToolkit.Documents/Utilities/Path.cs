﻿namespace LynxToolkit.Documents
{
    public static class Path
    {
        public static string GetDirectoryName(string path)
        {
            var i = path.LastIndexOfAny("\\/".ToCharArray());
            if (i >= 0)
            {
                return path.Substring(0, i);
            }

            return string.Empty;
        }

        public static string ChangeExtension(string path, string ext)
        {
            int i = path.LastIndexOf('.');
            if (ext.Length == 0)
            {
                ext = ".";
            }

            return (i >= 0 ? path.Substring(0, i) : path) + ext;
        }

        public static string GetExtension(string path)
        {
            int i = path.LastIndexOf('.');
            return i >= 0 ? path.Substring(i) : string.Empty;
        }

        public static bool IsPathRooted(string path)
        {
            var i = path.IndexOf(':');
            if (i >= 0)
            {
                path = path.Substring(i + 1);
            }

            return path.StartsWith("\\");
        }

        public static string Combine(string path1, string path2)
        {
            if (path1.Length > 0 && !path1.EndsWith("\\") && !path1.EndsWith("/"))
            {
                path1 += "\\";
            }

            return path1 + path2;
        }
    }
}