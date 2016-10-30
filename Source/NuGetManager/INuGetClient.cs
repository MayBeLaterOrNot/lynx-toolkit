namespace NuGetManager
{
    using System.Collections.Generic;

    public interface INuGetClient
    {
        IEnumerable<Package> GetVersions(string name);
        void Delete(Package package);
    }
}