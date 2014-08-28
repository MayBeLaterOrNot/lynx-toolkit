Generates MSDN-style documentation for .NET code.

```
XmlDocT [/singlepage] [/creatememberpages] [/indexpage=file] [/helpcontents=file]
        [/input=folder] [/output=file] [/stylesheet=file] [/toplevel=1] [/template=file]
		[/format=html|owiki|xml|json|word] [/extension=.owiki] [/ignore=attributeName]
        [/$key=value] ...
```

Argument               | Description
-----------------------|-----------------------------------------------------------------
/singlepage            | Specifies if output should be a single page.
/creatememberpages     | Specifies if member pages should be created.
/indexpage             | Specifies the index page.
/helpcontents          | Generates a help contents file. Specify the output file name (.hhc).
/input                 | Specifies an input folder.
/output                | Specifies the output root folder.
/stylesheet            | Specify a stylesheet to use.
/toplevel              | The level of the page headers.
/template              | Specify a output template.
/format                | Specify the output format (html).
/extension             | Specify the output extension.
/ignore                | Specify the name of an attribute.
/$key                  | Specify key/value pairs. The keys will be replaced by the value.
