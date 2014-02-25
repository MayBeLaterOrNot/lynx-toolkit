mkdir ..\Packages\Lynx.Documents\lib
mkdir ..\Packages\Lynx.Documents\lib\portable-NET4+wp71+win8
copy ..\Output\LynxToolkit.Documents.??? "..\Packages\Lynx.Documents\lib\portable-NET4+wp71+win8"
copy ..\license.txt ..\Packages\Lynx.Documents

mkdir ..\Packages\Lynx.Documents.Html\lib
mkdir ..\Packages\Lynx.Documents.Html\lib\NET40
copy ..\Output\LynxToolkit.Documents.Html.??? ..\Packages\Lynx.Documents.Html\lib\NET40
copy ..\license.txt ..\Packages\Lynx.Documents.Html

mkdir ..\Packages\Lynx.Documents.OpenXml\lib
mkdir ..\Packages\Lynx.Documents.OpenXml\lib\NET40
copy ..\Output\LynxToolkit.Documents.OpenXml.??? ..\Packages\Lynx.Documents.OpenXml\lib\NET40
copy ..\license.txt ..\Packages\Lynx.Documents.OpenXml

mkdir ..\Packages\Lynx.Documents.Wpf\lib
mkdir ..\Packages\Lynx.Documents.Wpf\lib\NET40
copy ..\Output\LynxToolkit.Documents.Wpf.??? ..\Packages\Lynx.Documents.Wpf\lib\NET40
copy ..\license.txt ..\Packages\Lynx.Documents.Wpf

set EnableNuGetPackageRestore=true
..\Source\.nuget\NuGet.exe pack ..\Packages\Lynx.Documents\Lynx.Documents.nuspec -OutputDirectory ..\Packages
..\Source\.nuget\NuGet.exe pack ..\Packages\Lynx.Documents.OpenXML\Lynx.Documents.OpenXML.nuspec -OutputDirectory ..\Packages
..\Source\.nuget\NuGet.exe pack ..\Packages\Lynx.Documents.Html\Lynx.Documents.Html.nuspec -OutputDirectory ..\Packages
..\Source\.nuget\NuGet.exe pack ..\Packages\Lynx.Documents.Wpf\Lynx.Documents.Wpf.nuspec -OutputDirectory ..\Packages
