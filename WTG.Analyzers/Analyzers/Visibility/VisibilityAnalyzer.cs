using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;

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
			context.RegisterSyntaxNodeAction(Analyze, ModifierExtractionVisitor.SupportedSyntaxKinds);
		}

		void Analyze(SyntaxNodeAnalysisContext context)
		{
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
