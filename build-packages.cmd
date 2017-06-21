@echo off
dotnet pack --no-build WTG.Analyzers\WTG.Analyzers.csproj
dotnet pack --no-build WTG.Analyzers.TestFramework\WTG.Analyzers.TestFramework.csproj
