using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Test
{
	[TestFixture(TypeArgs = new[] { typeof(ArrayAnalyzer), typeof(ArrayCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(AsyncAnalyzer), typeof(AsyncCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(AwaitCompletedAnalyzer), typeof(AwaitCompletedCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(BooleanLiteralAnalyzer), typeof(BooleanComparisonCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(BooleanLiteralAnalyzer), typeof(BooleanLiteralCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(CodeContractsAnalyzer), typeof(CodeContractsCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(CompletedTaskAnalyzer), typeof(CompletedTaskCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(DeconstructionAnalyzer), typeof(DeconstructionCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(DiscardThrowAnalyzer), typeof(DiscardThrowCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(EmitAnalyzer), typeof(EmitCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(FlagsAnalyzer), typeof(FlagsCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(LinqAnalyzer), typeof(LinqCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(NullComparisonAnalyzer), typeof(NullComparisonCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(PointlessOverrideAnalyzer), typeof(PointlessOverrideCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(RegexAnalyzer), typeof(RegexCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(RegionDirectiveAnalyzer), typeof(RegionDirectiveCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(SuppressionAnalyzer), typeof(SuppressionCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(ToStringAnalyzer), typeof(ToStringCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(UsingsAnalyzer), typeof(UsingsCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(VarAnalyzer), typeof(VarCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(VisibilityAnalyzer), typeof(VisibilityCodeFixProvider) })]
	[TestFixture(TypeArgs = new[] { typeof(WhitespaceAnalyzer), typeof(WhitespaceCodeFixProvider) })]
	public class AnalyzerAndCodeFixTest<TAnalyzer, TCodeFix>
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
			var diagnostics = await DiagnosticUtils.GetDiagnosticsAsync(analyzer, ModelUtils.CreateDocument(string.Empty)).ConfigureAwait(false);
			Assert.That(diagnostics, IsDiagnostic.Empty);
		}

		[Test]
		public void CodeFixProviderAttribute()
		{
			var codeFixType = typeof(TCodeFix);
			var att = codeFixType.GetCustomAttribute<ExportCodeFixProviderAttribute>();
			Assert.That(att, Is.Not.Null & Has.Property(nameof(att.Name)).EqualTo(codeFixType.Name));
		}

		[Test]
		public async Task RunTest([ValueSource(nameof(Samples))] SampleDataSet data)
		{
			var filter = CreateFilter(data);
			var analyzer = new TAnalyzer();
			var document = ModelUtils.CreateDocument(data);
			var actual = await DiagnosticUtils.GetDiagnosticsAsync(analyzer, document).ConfigureAwait(false);

			Assert.That(actual.Where(filter), IsDiagnostic.EqualTo(data.Diagnostics));

			var fixer = new CodeFixer(analyzer, new TCodeFix())
			{
				DiagnosticFilter = filter,
			};

			await fixer.VerifyFixAsync(document, data.Result).ConfigureAwait(false);
		}

		[Test]
		public async Task BulkUpdate([ValueSource(nameof(Samples))] SampleDataSet data)
		{
			var analyzer = new TAnalyzer();
			var document = ModelUtils.CreateDocument(data);

			var fixer = new CodeFixer(analyzer, new TCodeFix())
			{
				DiagnosticFilter = CreateFilter(data),
			};

			await fixer.VerifyBulkFixAsync(document, data.Result).ConfigureAwait(false);
		}

		#region Implementation

		static Func<Diagnostic, bool> CreateFilter(SampleDataSet data) => d => !data.SuppressedIds.Contains(d.Id);

		const string TestDataPrefix = "WTG.Analyzers.Test.TestData.";
		static IEnumerable<SampleDataSet> Samples => SampleDataSet.GetSamples(typeof(AnalyzerAndCodeFixTest<,>).GetTypeInfo().Assembly, TestDataPrefix + typeof(TAnalyzer).Name + ".");

		#endregion
	}
}
