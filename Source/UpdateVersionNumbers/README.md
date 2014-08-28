Updates version numbers and company/copyright information in AssemblyInfo and NuSpec files.

This program does a recursive scan to find all *AssemblyInfo.cs and *.nuspec files under the specified root folder.

## Syntax

```
UpdateVersionNumbers.exe [/Directory=..\src] [/VersionFile=..\version.txt] [/Version=x.x.x.x] [/VersionFromNuGet=packagename] 
    [/Build=x] [/Revision=x] [/PreRelease=name] [/Company=xxx] [/Copyright=xxx] [/Dependency=packagename] [/ReleaseNotesFile=filename]
```

Arguments
```
  /Directory - specifies the root directory (all subdirectories will be scanned)
  /Version - specifies the version number (Major.Minor.Build.Revision)
     * = automatic build/revision numbers
     yyyy = year
     MM = month
     dd = day
  /VersionFile - gets the version number from a file
  /VersionFromNuGet - gets the version number from the latest version of the specified package
  /PreRelease - adds a pre-release string as specified in http://semver.org/
  /Build - overwrites the build number
  /Revision - overwrites the revision number
  /ReleaseNotesFile - gets the release notes from a file
  /Company
  /Copyright
  /Dependency - specifies a NuGet package that should have the same version number
```