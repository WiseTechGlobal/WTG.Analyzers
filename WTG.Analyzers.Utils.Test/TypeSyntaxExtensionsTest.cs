using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	class TypeSyntaxExtensionsTest
	{
		[TestCase("System", "System", ExpectedResult = true)]
		[TestCase("System", "Syste", ExpectedResult = false)]
		[TestCase("Syste", "System", ExpectedResult = false)]
		[TestCase("System.Diagnostics", "System.Diagnostics", ExpectedResult = true)]
		[TestCase("System.Diagnostics", "SystemxDiagnostics", ExpectedResult = false)]
		[TestCase("System.Diagnostics.Contracts", "System.Diagnostics.Contracts", ExpectedResult = true)]
		[TestCase("global::System.Diagnostics.Contracts", "System.Diagnostics.Contracts", ExpectedResult = true)]
		[TestCase("System.Diagnostics.Contracts", "System.Diagnostics.Diagnostic", ExpectedResult = false)]
		[TestCase("System.Diagnostics.Contracts", "System.Diagnostics", ExpectedResult = false)]
		[TestCase("System.Diagnostics", "System.Diagnostics.Diagnostic", ExpectedResult = false)]
		public bool IsMatch(string type, string expected)
			=> SyntaxFactory.ParseTypeName(type).IsMatch(expected);
	}
}
