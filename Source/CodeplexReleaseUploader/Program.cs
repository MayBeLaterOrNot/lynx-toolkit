// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="Lynx">
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
// --------------------------------------------------------------------------------------------------------------------
namespace CodeplexReleaseUploader
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Codeplex.Services;

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(LynxToolkit.Application.Header);

            var rs = new ReleaseServiceSoapClient();
            string projectName = null;
            string releaseName = null;
            string description = null;
            string releaseDate = DateTime.Now.ToString("yyyy-MM-dd");
            string status = "Stable"; // Planning, Alpha, Beta, Stable
            bool showToPublic = true;
            bool isDefaultRelease = true;
            string username = Environment.GetEnvironmentVariable("CODEPLEX_USERNAME", EnvironmentVariableTarget.User);
            string password = Environment.GetEnvironmentVariable("CODEPLEX_PASSWORD", EnvironmentVariableTarget.User);

            var files = new List<ReleaseFile>();

            // command line argument parsing
            foreach (string arg in args)
            {
                string[] keyAndValue = arg.Split('=');
                string key = keyAndValue[0];
                string value = keyAndValue[1];

                switch (key)
                {
                    case "/Project":
                        projectName = value;
                        break;
                    case "/Release":
                        releaseName = value;
                        break;
                    case "/Description":
                        description = value;
                        break;
                    case "/ReleaseDate":
                        releaseDate = value;
                        break;
                    case "/Status":
                        status = value;
                        break;
                    case "/ShowToPublic":
                        showToPublic = bool.Parse(value);
                        break;
                    case "/IsDefaultRelease":
                        isDefaultRelease = bool.Parse(value);
                        break;
                    case "/UserName":
                        username = value;
                        break;
                    case "/Password":
                        password = value;
                        break;
                    case "/Upload":
                        if (value.Contains("*"))
                        {
                            AddFiles(files, value);
                        }
                        else
                        {
                            var uploadValues = value.Split(',');
                            string path = uploadValues[0];
                            string name = uploadValues.Length > 1 ? uploadValues[1] : null;
                            AddFile(files, path, name);
                        }

                        break;
                }
            }

            Console.WriteLine("Project:      {0}", projectName);
            Console.WriteLine("Release:      {0}", releaseName);
            Console.WriteLine("Description:  {0}", description);
            Console.WriteLine("Date:         {0}", releaseDate);
            Console.WriteLine("UserName:     {0}", username ?? "n/a");
            Console.WriteLine("Password:     {0}", password != null ? new string('*', password.Length) : "n/a");
            Console.WriteLine();

            Console.WriteLine("Creating release");
            try
            {
                rs.CreateARelease(
                    projectName,
                    releaseName,
                    description,
                    releaseDate,
                    status,
                    showToPublic,
                    isDefaultRelease,
                    username,
                    password);
                Console.WriteLine("  Completed.");
            }
            catch (Exception e1)
            {
                Console.WriteLine("  " + e1.Message);
            }

            if (files.Count > 0)
            {
                var recommendedFileName = files[0].FileName;

                Console.WriteLine("Uploading files");
                foreach (var f in files)
                {
                    Console.WriteLine("  {0} '{1}' ({2}) {3}kB", f.FileName, f.Name, f.FileType, f.FileData.Length / 1024);
                }

                try
                {
                    rs.UploadTheReleaseFiles(
                        projectName, releaseName, files.ToArray(), recommendedFileName, username, password);
                    Console.WriteLine("  Completed.");
                }
                catch (Exception e1)
                {
                    Console.WriteLine("  " + e1.Message);
                }
            }
        }

        private static void AddFiles(List<ReleaseFile> files, string path)
        {
            var dir = Path.GetDirectoryName(path);
            var searchPattern = Path.GetFileName(path);
            foreach (var file in Directory.GetFiles(dir, searchPattern))
            {
                AddFile(files, file);
            }
        }

        private static void AddFile(List<ReleaseFile> files, string path, string name = null)
        {
            string fileName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(fileName) || files.FirstOrDefault(f => f.FileName == fileName) != null)
            {
                return;
            }

            if (name == null)
            {
                name = fileName;
            }

            string fileType = "RuntimeBinary";
            if (name.IndexOf("example", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                fileType = "Example";
            }

            if (name.IndexOf("doc", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                fileType = "Documentation";
            }

            //// FileType { RuntimeBinary, SourceCode, Documentation, Example }

            files.Add(
                new ReleaseFile { FileData = File.ReadAllBytes(path), FileName = fileName, Name = name, FileType = fileType });
        }
    }
}