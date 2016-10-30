namespace NuGetManager
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using NuGet;

    public class NuGetCoreClient : INuGetClient
    {
        private readonly string source;
        private IPackageRepository repo;

        public NuGetCoreClient(string source = "https://packages.nuget.org")
        {
            this.source = source;
            this.repo = PackageRepositoryFactory.Default.CreateRepository(source + "/api/v2");
        }

        public IEnumerable<Package> GetVersions(string packageID)
        {
            foreach (var p in this.repo.GetPackages().Where(p => p.Id.Contains(packageID)))
            {
                yield return new Package
                {
                    Source = this.source,
                    Name = p.Id,
                    Version = p.Version.ToString(),
                    IsListed = p.IsListed(),
                    IsReleaseVersion = p.IsReleaseVersion(),
                    IconUrl = p.IconUrl ?? new Uri("pack://application:,,,/nuget.png")
                };
            }
        }

        public void Delete(Package package)
        {
            var semver = new SemanticVersion(package.Version);
            var pkgs = this.repo.GetPackages().Where(p => p.Id == package.Name).ToArray();
            var pkg = pkgs.FirstOrDefault(p => p.Version == semver);
            if (pkg != null)
            {
                this.repo.RemovePackage(pkg);
            }
        }
    }
}