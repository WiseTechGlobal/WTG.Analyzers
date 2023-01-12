using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class CompilationExtensionsTest
	{
		[TestCase(LanguageVersion.CSharp3, LanguageVersion.CSharp4, ExpectedResult = false)]
		[TestCase(LanguageVersion.CSharp4, LanguageVersion.CSharp4, ExpectedResult = true)]
		[TestCase(LanguageVersion.CSharp5, LanguageVersion.CSharp4, ExpectedResult = true)]
		public bool IsCSharpVersionOrGreaterWithCSharpCompilation(LanguageVersion compilationVersion, LanguageVersion comparisonVersion)
		{
			var options = new CSharpParseOptions(compilationVersion);
			var syntaxTree = SyntaxFactory.ParseSyntaxTree(string.Empty, options);
			var compilation = CSharpCompilation.Create("TEST", new[] { syntaxTree });

			return compilation.IsCSharpVersionOrGreater(comparisonVersion);
		}
	}
}
