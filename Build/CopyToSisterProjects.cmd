call BuildRelease.cmd

del ..\..\HelixToolkit\Tools\Lynx\*.* /Q
del ..\..\OxyPlot\Tools\Lynx\*.* /Q
del ..\..\PropertyTools\Tools\Lynx\*.* /Q
del ..\..\Units\Tools\Lynx\*.* /Q
del ..\..\UsageStats\Tools\Lynx\*.* /Q
del ..\..\Glissando\Tools\Lynx\*.* /Q

copy ..\Output\*.* ..\..\HelixToolkit\Tools\Lynx
copy ..\Output\*.* ..\..\OxyPlot\Tools\Lynx
copy ..\Output\*.* ..\..\PropertyTools\Tools\Lynx
copy ..\Output\*.* ..\..\Units\Tools\Lynx
copy ..\Output\*.* ..\..\UsageStats\Tools\Lynx
copy ..\Output\*.* ..\..\Glissando\Tools\Lynx

