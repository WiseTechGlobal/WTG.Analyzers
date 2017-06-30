@echo off
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.nuspec -OutputDirectory Bin
packages\NuGet.CommandLine\tools\NuGet.exe pack WTG.Analyzers.TestFramework.nuspec -OutputDirectory Bin
