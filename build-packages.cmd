@echo off
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.nuspec -OutputDirectory Bin -Version %1
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.TestFramework.nuspec -OutputDirectory Bin -Version %1
