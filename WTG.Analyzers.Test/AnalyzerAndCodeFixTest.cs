using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using WTG.Analyzers.Test.Constraints;
using WTG.Analyzers.Test.Helpers;

namespace WTG.Analyzers.Test
{
	[TestFixture(TypeArgs = new[] { typeof(AsyncAnalyzer), typeof(AsyncCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(VisibilityAnalyzer), typeof(VisibilityCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(VarAnalyzer), typeof(VarCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(WhitespaceAnalyzer), typeof(WhitespaceCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(SuppressionAnalyzer), typeof(SuppressionCodeFixProvider) })]
	internal class AnalyzerAndCodeFixTest<TAnalyzer, TCodeFix>
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
			var test = @"";

			var analyzer = new TAnalyzer();
			var diagnostics = await DiagnosticUtils.GetSortedDiagnosticsAsync(analyzer, test).ConfigureAwait(false);
			Assert.That(diagnostics, Is.Empty);
		}

		[Test]
		public async Task RunTest([ValueSource(nameof(SampleNames))] string sampleName)
		{
			var data = SampleDataSet.GetSampleData(typeof(TAnalyzer).Name, sampleName);
			var analyzer = new TAnalyzer();
			var actual = await DiagnosticUtils.GetSortedDiagnosticsAsync(analyzer, data.Source).ConfigureAwait(false);
			var expected = data.Diagnostics;

			var count = Math.Min(actual.Length, expected.Length);

			for (var i = 0; i < count; i++)
			{
				Assert.That(actual[i], IsDiagnostic.EqualTo(expected[i]), "Diagnostic[{0}]", i);
			}

			Assert.That(actual.Length, Is.EqualTo(expected.Length));

			var fixer = new CodeFixer(analyzer, new TCodeFix());
			await fixer.VerifyFixAsync(data.Source, data.Result).ConfigureAwait(false);
		}

		#region Implementation

		static IEnumerable<string> SampleNames => SampleDataSet.GetSampleNames(typeof(TAnalyzer).Name);

		#endregion
	}
}
