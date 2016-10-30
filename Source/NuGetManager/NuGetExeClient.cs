namespace NuGetManager
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public class NuGetExeClient : INuGetClient
    {
        private readonly string apiKey;
        private string exe = "nuget.exe";

        public NuGetExeClient(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public IEnumerable<Package> GetVersions(string name)
        {
            foreach (var line in this.Run($"list {name} -AllVersions -Prerelease"))
            {
                var version = line.Replace(name, "").Trim();
                yield return new Package { Name = name, Version = version };
            }
        }

        public void Delete(Package package)
        {
            this.Run($"delete {package.Name} {package.Version} {this.apiKey} -source {package.Source} -NonInteractive").ToArray();
        }

        private IEnumerable<string> Run(string args)
        {
            System.Console.WriteLine(this.exe + " " + args);
            var psi = new ProcessStartInfo(exe, args);
            psi.UseShellExecute = false;
            psi.RedirectStandardOutput = true;
            psi.CreateNoWindow = true;
            var proc = Process.Start(psi);
            while (!proc.StandardOutput.EndOfStream)
            {
                var line = proc.StandardOutput.ReadLine();
                System.Console.WriteLine(line);
                yield return line;
            }
        }
    }
}