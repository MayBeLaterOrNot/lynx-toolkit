Finds the first match of the specified pattern and writes the value of the first group to the output file.

## Syntax

```
ExtractText.exe <input-file> <output-file> [pattern]
```

input-file   The input file (required).
output-file  The output file (required).
pattern      The regular expression (optional). The default pattern is `"\n\#\#.*?\n(.*?)\n\#\#[^#]"`