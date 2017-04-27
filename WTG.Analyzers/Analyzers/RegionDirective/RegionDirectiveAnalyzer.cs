using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class RegionDirectiveAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotNestRegionsRule,
			Rules.RegionsShouldNotSplitStructuresRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterSyntaxTreeAction(Analyze);
		}

		static void Analyze(SyntaxTreeAnalysisContext context)
		{
			if (context.Tree.IsGenerated(context.CancellationToken))
			{
				return;
			}

			var root = context.Tree.GetRoot(context.CancellationToken);

			foreach (var region in RegionDirective.Extract(root))
			{
				if (region.Depth > 0)
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Rules.DoNotNestRegionsRule,
							region.Start.GetLocation(),
							new[] { region.End.GetLocation() }.AsEnumerable()));
				}

				if (DirectiveHelper.BoundingTriviaSplitsNodes(region.Start, region.End))
				{
					context.ReportDiagnostic(
						Diagnostic.Create(
							Rules.RegionsShouldNotSplitStructuresRule,
							region.Start.GetLocation(),
							new[] { region.End.GetLocation() }.AsEnumerable()));
				}
			}
		}
	}
}
