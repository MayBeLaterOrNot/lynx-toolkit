namespace CodeplexReleaseUploader
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    using CodeplexReleaseUploader.Codeplex.Services;

    class Program
    {
        static void Main(string[] args)
        {
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
                        ////  RuntimeBinary, SourceCode, Documentation, Example
                        string fileType = "RuntimeBinary";
                        var uploadValues = value.Split(',');
                        string path = uploadValues[0];
                        string fileName = Path.GetFileName(path);
                        string name = uploadValues[1];
                        if (name.IndexOf("example", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            fileType = "Example";
                        }

                        if (name.IndexOf("doc", StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            fileType = "Documentation";
                        }

                        files.Add(new ReleaseFile { FileData = File.ReadAllBytes(path), FileName = fileName, Name = name, FileType = fileType });
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
    }
}
