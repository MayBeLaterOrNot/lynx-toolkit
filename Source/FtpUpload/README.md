Uploads a single file to FTP using binary/passive mode.


## Syntax

```
FtpUpload.exe <host> <username> <password> <localfile> <remotefile> [/r] [/t=3]
```

Argument | Default | Description
---------|---------|---------------------------
/r       | true    | recursive search
/t       | 3       | number of upload threads