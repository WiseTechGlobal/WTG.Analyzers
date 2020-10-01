using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace WTG.Analyzers.Analyzers.BooleanLiteral
{
	static class CompilationExtensions
	{
		public static bool IsCSharpVersionOrGreater(this Compilation compilation, LanguageVersion version)
		{
			if (compilation.Language != LanguageNames.CSharp)
			{
				// It's not even C#!
				return false;
			}

			var csc = (CSharpCompilation)compilation;
			return csc.LanguageVersion >= version;
		}
	}
}
