using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed class BooleanLiteralCombiningAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.AvoidBoolLiteralsInLargerBoolExpressionsRule);

		public override void Initialize(AnalysisContext context)
		{
			context.EnableConcurrentExecution();
			context.RegisterCompilationStartAction(CompilationStart);
		}

		void CompilationStart(CompilationStartAnalysisContext context)
		{
			var cache = new FileDetailCache();

			context.RegisterSyntaxNodeAction(
				c => Analyze(c, cache),
				SyntaxKind.TrueLiteralExpression,
				SyntaxKind.FalseLiteralExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			if (CanBeSimplified(context.Node.Parent))
			{
				context.ReportDiagnostic(
					Rules.CreateAvoidBoolLiteralsInLargerBoolExpressionsDiagnostic(
						context.Node.GetLocation()));
			}
		}

		static bool CanBeSimplified(SyntaxNode node)
		{
			switch (node.Kind())
			{
				case SyntaxKind.LogicalAndExpression:
				case SyntaxKind.LogicalOrExpression:
				case SyntaxKind.ConditionalExpression:
				case SyntaxKind.LogicalNotExpression:
				case SyntaxKind.ParenthesizedExpression:
				case SyntaxKind.AndAssignmentExpression:
				case SyntaxKind.OrAssignmentExpression:
					return true;

				default:
					return false;
			}
		}
	}
}
