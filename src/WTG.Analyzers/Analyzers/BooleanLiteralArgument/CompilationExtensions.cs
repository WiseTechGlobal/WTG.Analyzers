using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers.Analyzers.BooleanLiteral
{
	static class CompilationExtensions
	{
		public static bool IsCSharpVersionOrGreater([NotNullWhen(true)] this Compilation? compilation, LanguageVersion version)
		{
			if (compilation == null || compilation.Language != LanguageNames.CSharp)
			{
				// It's not even C#!
				return false;
			}

			var csc = (CSharpCompilation)compilation;
			return csc.LanguageVersion >= version;
		}
	}
}
