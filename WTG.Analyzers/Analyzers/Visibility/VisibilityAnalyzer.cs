using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class VisibilityAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(new[]
		{
			Rules.DoNotUseThePrivateKeywordRule,
		});

		public override void Initialize(AnalysisContext context)
		{
			context.RegisterCompilationStartAction(CompilationStart);
		}

		static void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();
			context.RegisterSyntaxNodeAction(c => Analyze(c, cache), ModifierExtractionVisitor.SupportedSyntaxKinds);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var list = ModifierExtractionVisitor.Instance.Visit(context.Node);

			foreach (var modifier in list)
			{
				if (modifier.Kind() == SyntaxKind.PrivateKeyword)
				{
					context.ReportDiagnostic(Rules.CreateDoNotUseThePrivateKeywordDiagnostic(modifier.GetLocation()));
				}
			}
		}
	}
}
