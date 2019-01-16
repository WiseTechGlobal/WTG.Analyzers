@echo off
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.nuspec -OutputDirectory Bin -Version %1 -Properties commitid=%2
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.Utils.nuspec -OutputDirectory Bin -Version %1 -Properties commitid=%2
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.TestFramework.nuspec -OutputDirectory Bin -Version %1 -Properties commitid=%2
