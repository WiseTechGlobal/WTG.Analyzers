using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class DiscardThrowAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.AvoidDiscardCoalesceThrowRule);

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
				SyntaxKind.SimpleAssignmentExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var assignment = (AssignmentExpressionSyntax)context.Node;

			if (assignment.Left.IsKind(SyntaxKind.IdentifierName) &&
				assignment.Parent.IsKind(SyntaxKind.ExpressionStatement) &&
				IsCoalesceThrow(assignment.Right) &&
				IsDiscard(context.SemanticModel, (IdentifierNameSyntax)assignment.Left))
			{
				var statement = (ExpressionStatementSyntax)assignment.Parent;

				context.ReportDiagnostic(Rules.CreateAvoidDiscardCoalesceThrowDiagnostic(statement.GetLocation()));
			}
		}

		static bool IsCoalesceThrow(ExpressionSyntax expression)
		{
			if (!expression.IsKind(SyntaxKind.CoalesceExpression))
			{
				return false;
			}

			var coalesce = (BinaryExpressionSyntax)expression;

			return coalesce.Right.IsKind(SyntaxKind.ThrowExpression);
		}

		static bool IsDiscard(SemanticModel semanticModel, IdentifierNameSyntax expression)
		{
			if (expression.Identifier.Text != "_")
			{
				return false;
			}

			var symbol = semanticModel.GetSymbolInfo(expression).Symbol;

			return symbol != null && symbol.Kind == SymbolKind.Discard;
		}
	}
}
