Shows information about the Byte-order-marks of text files.

## Syntax

```
BomInfo.exe /directory=<name> /searchpattern=<pattern> [/ignore=<pattern> ...] [/donotreport=<encoding> ...] [/force=<encoding>]
```

Argument         | Default value | Description
-----------------|-----------------------------------------------------------------------------------
/directory       | .             | Directory to search
/searchpattern   | *.cs          | Files to include
/ignore          | bin, obj      | Files/folders to exclude
/donotreport     | UTF-8         | List of BOM that should not be reported
/force           |               | If specified, the files will be patched to use the specified BOM
 