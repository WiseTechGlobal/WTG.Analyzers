using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

			if (CanBeSimplified(context.Node, context.SemanticModel, context.CancellationToken))
			{
				context.ReportDiagnostic(
					Rules.CreateAvoidBoolLiteralsInLargerBoolExpressionsDiagnostic(
						context.Node.GetLocation()));
			}
		}

		static bool CanBeSimplified(SyntaxNode node, SemanticModel semanticModel, CancellationToken cancellationToken)
		{
			switch (node.Parent.Kind())
			{
				case SyntaxKind.LogicalAndExpression:
				case SyntaxKind.LogicalOrExpression:
				case SyntaxKind.LogicalNotExpression:
				case SyntaxKind.ParenthesizedExpression:
				case SyntaxKind.AndAssignmentExpression:
				case SyntaxKind.OrAssignmentExpression:
					return true;

				case SyntaxKind.ConditionalExpression:
					var condition = (ConditionalExpressionSyntax)node.Parent;
					var expressionToCheck = condition.WhenTrue == node ? condition.WhenFalse : condition.WhenTrue;
					var result = IsSimpleBooleanExpression(expressionToCheck, semanticModel, cancellationToken);
					return result;

				default:
					return false;
			}

			static bool IsSimpleBooleanExpression(ExpressionSyntax syntax, SemanticModel semanticModel, CancellationToken cancellationToken)
				=> IsSimpleBooleanType(semanticModel.GetTypeInfo(syntax, cancellationToken).Type);

			static bool IsSimpleBooleanType(ITypeSymbol type) => type.SpecialType == SpecialType.System_Boolean;
		}
	}
}
