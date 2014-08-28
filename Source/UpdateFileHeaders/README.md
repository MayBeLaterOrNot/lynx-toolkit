Updates file headers in all .cs files under the specified directory.

## Syntax

```
UpdateFileHeaders.exe </company=companyName> [/copyright=copyrightNotice] [/copyright-file=path] [/exclude=filestoExclude] <directory>
```

- AssemblyInfo.cs is excluded by default.
- The default copyright notice is "Copyright © {Company}. All rights reserved."

## Example

```
UpdateFileHeaders.exe /company=MyCompany /copyright-file=LICENSE src
```