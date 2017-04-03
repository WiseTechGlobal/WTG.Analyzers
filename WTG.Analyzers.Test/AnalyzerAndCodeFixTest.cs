using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Test
{
	[TestFixture(TypeArgs = new[] { typeof(AsyncAnalyzer), typeof(AsyncCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(BooleanComparisonAnalyzer), typeof(BooleanComparisonCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(VisibilityAnalyzer), typeof(VisibilityCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(VarAnalyzer), typeof(VarCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(WhitespaceAnalyzer), typeof(WhitespaceCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(SuppressionAnalyzer), typeof(SuppressionCodeFixProvider) })]
	class AnalyzerAndCodeFixTest<TAnalyzer, TCodeFix>
		where TAnalyzer : DiagnosticAnalyzer, new()
		where TCodeFix : CodeFixProvider, new()
	{
		[Test]
		public void FixProviderWorksWithAnalyzer()
		{
			var analyzer = new TAnalyzer();
			var codeFix = new TCodeFix();

			Assert.That(analyzer.SupportedDiagnostics.Select(x => x.Id).Intersect(codeFix.FixableDiagnosticIds), Is.Not.Empty, "The fix provider and analyzer should have at least one id in common, are you sure you got the right types?");
		}

		[Test]
		public async Task NoErrorOnEmptyInput()
		{
			var analyzer = new TAnalyzer();
			var diagnostics = await DiagnosticUtils.GetDiagnosticsAsync(analyzer, string.Empty).ConfigureAwait(false);
			Assert.That(diagnostics, IsDiagnostic.Empty);
		}

		[Test]
		public async Task RunTest([ValueSource(nameof(Samples))] SampleDataSet data)
		{
			var analyzer = new TAnalyzer();
			var actual = await DiagnosticUtils.GetDiagnosticsAsync(analyzer, data.Source).ConfigureAwait(false);

			Assert.That(actual, IsDiagnostic.EqualTo(data.Diagnostics));

			var fixer = new CodeFixer(analyzer, new TCodeFix());
			await fixer.VerifyFixAsync(data.Source, data.Result).ConfigureAwait(false);
		}

		#region Implementation

		const string TestDataPrefix = "WTG.Analyzers.Test.TestData.";
		static IEnumerable<SampleDataSet> Samples => SampleDataSet.GetSamples(typeof(AnalyzerAndCodeFixTest<,>).Assembly, TestDataPrefix + typeof(TAnalyzer).Name + ".");

		#endregion
	}
}
