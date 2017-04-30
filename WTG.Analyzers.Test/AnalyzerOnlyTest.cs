using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Test
{
	[TestFixture(TypeArgs = new[] { typeof(ConditionDirectiveAnalyzer) })]
	class AnalyzerTest<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
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
		}

		#region Implementation

		const string TestDataPrefix = "WTG.Analyzers.Test.TestData.";
		static IEnumerable<SampleDataSet> Samples => SampleDataSet.GetSamples(typeof(AnalyzerTest<>).Assembly, TestDataPrefix + typeof(TAnalyzer).Name + ".");

		#endregion
	}
}
