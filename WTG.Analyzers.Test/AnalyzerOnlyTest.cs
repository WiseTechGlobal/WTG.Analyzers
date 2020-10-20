using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Test
{
	[TestFixture(TypeArgs = new[] { typeof(ConditionalOperandAnalyzer) })]
	[TestFixture(TypeArgs = new[] { typeof(ConditionDirectiveAnalyzer) })]
	[TestFixture(TypeArgs = new[] { typeof(DiscardVariableAnalyzer) })]
	[TestFixture(TypeArgs = new[] { typeof(HttpReasonPhraseAnalyzer) })]
	[TestFixture(TypeArgs = new[] { typeof(NestedConditionalAnalyzer) })]
	[TestFixture(TypeArgs = new[] { typeof(PublicTupleTypeAnalyzer) })]
	public class AnalyzerTest<TAnalyzer>
		where TAnalyzer : DiagnosticAnalyzer, new()
	{
		[Test]
		public async Task NoErrorOnEmptyInput()
		{
			var analyzer = new TAnalyzer();
			var diagnostics = await DiagnosticUtils.GetDiagnosticsAsync(analyzer, ModelUtils.CreateDocument(string.Empty)).ConfigureAwait(false);
			Assert.That(diagnostics, IsDiagnostic.Empty);
		}

		[Test]
		public async Task RunTest([ValueSource(nameof(Samples))] SampleDataSet data)
		{
			var filter = CreateFilter(data);
			var analyzer = new TAnalyzer();
			var actual = await DiagnosticUtils.GetDiagnosticsAsync(analyzer, ModelUtils.CreateDocument(data)).ConfigureAwait(false);

			Assert.That(actual.Where(filter), IsDiagnostic.EqualTo(data.Diagnostics));
		}

		#region Implementation

		static Func<Diagnostic, bool> CreateFilter(SampleDataSet data) => d => !data.SuppressedIds.Contains(d.Id);

		const string TestDataPrefix = "WTG.Analyzers.Test.TestData.";
		static IEnumerable<SampleDataSet> Samples => SampleDataSet.GetSamples(typeof(AnalyzerTest<>).GetTypeInfo().Assembly, TestDataPrefix + typeof(TAnalyzer).Name + ".");

		#endregion
	}
}
