using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class BooleanLiteralAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.UseNamedArgumentsWhenPassingBooleanLiteralsRule);

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

			var expression = (LiteralExpressionSyntax)context.Node;
			if (!expression.Parent.IsKind(SyntaxKind.Argument))
			{
				return;
			}

			var argument = (ArgumentSyntax)expression.Parent;
			if (argument.NameColon != null)
			{
				return;
			}

			var argumentSymbol = argument.TryFindCorrespondingParameterSymbol(context.SemanticModel, context.CancellationToken);

			if (argumentSymbol is null || argumentSymbol.OriginalDefinition.Type.TypeKind == TypeKind.TypeParameter)
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					Rules.UseNamedArgumentsWhenPassingBooleanLiteralsRule,
					argument.GetLocation()));
		}
	}
}
