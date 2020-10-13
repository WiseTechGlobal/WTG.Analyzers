# WiseTech Global Analyzers

[![CI/CD](https://github.com/WiseTechGlobal/WTG.Analyzers/workflows/CI/CD/badge.svg?branch=master&event=push)](https://github.com/WiseTechGlobal/WTG.Analyzers/actions?query=workflow%3ACI%2FCD)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg)](https://github.com/WiseTechGlobal/WTG.Analyzers/blob/master/LICENSE.md)
[![NuGet](https://img.shields.io/nuget/v/WTG.Analyzers.svg)](http://nuget.org/packages/WTG.Analyzers)

Roslyn Analyzers from [WiseTech Global](https://www.wisetechglobal.com/) to enforce our styles, behaviours, and prevent common mistakes.

**Note for WTG staff:** This repository is only for rules that are generic enough to apply to any .NET project. For rules that are specific to internal projects and frameworks, create an internal Analyzers project.

A list of rules can be found on the [project wiki](https://github.com/WiseTechGlobal/WTG.Analyzers/wiki).

## Using WTG.Analyzers
For internal .NET Framework projects, follow the existing convention for that project.

For any other project - including internal .NET Core projects, and public or personal projects - the preferred method is to add the [NuGet package](https://www.nuget.org/packages/WTG.Analyzers/) to your project.

## Current Status
This is used in production on regular builds of our codebase, hundreds of times per day.

Names and IDs are not expected to change.
