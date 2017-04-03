using System;
using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class BooleanComparisonAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotCompareBoolToAConstantValueRule,
			Rules.DoNotCompareBoolToAConstantValueInAnExpressionRule);

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
				SyntaxKind.EqualsExpression,
				SyntaxKind.NotEqualsExpression);
		}

		static void Analyze(SyntaxNodeAnalysisContext context, FileDetailCache cache)
		{
			if (cache.IsGenerated(context.SemanticModel.SyntaxTree, context.CancellationToken))
			{
				return;
			}

			var expression = (BinaryExpressionSyntax)context.Node;
			ExpressionSyntax additional;

			if (BoolLiteralVisitor.Instance.Visit(expression.Right).HasValue && IsBoolValue(context.SemanticModel, expression.Left, context.CancellationToken))
			{
				additional = expression.Right;
			}
			else if (BoolLiteralVisitor.Instance.Visit(expression.Left).HasValue && IsBoolValue(context.SemanticModel, expression.Right, context.CancellationToken))
			{
				additional = expression.Left;
			}
			else
			{
				return;
			}

			context.ReportDiagnostic(
				Diagnostic.Create(
					IsContainedInExpressionTree(context.SemanticModel, expression, context.CancellationToken)
						? Rules.DoNotCompareBoolToAConstantValueInAnExpressionRule
						: Rules.DoNotCompareBoolToAConstantValueRule,
					CombineLocations(
						expression.OperatorToken.GetLocation(),
						additional.GetLocation())));
		}

		static bool IsBoolValue(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var type = model.GetTypeInfo(expression, cancellationToken).Type;
			return type != null && type.SpecialType == SpecialType.System_Boolean;
		}

		static bool IsContainedInExpressionTree(SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			var lambda = FindContainingLambdaExpression(expression);

			if (lambda == null)
			{
				return false;
			}

			var type = model.GetTypeInfo(lambda, cancellationToken).ConvertedType;
			return type != null && type.IsMatch("System.Core", "System.Linq.Expressions.Expression`1");
		}

		static LambdaExpressionSyntax FindContainingLambdaExpression(SyntaxNode node)
		{
			while (node != null)
			{
				switch (node.Kind())
				{
					case SyntaxKind.ParenthesizedLambdaExpression:
					case SyntaxKind.SimpleLambdaExpression:
						return (LambdaExpressionSyntax)node;

					case SyntaxKind.AddAccessorDeclaration:
					case SyntaxKind.AnonymousMethodExpression:
					case SyntaxKind.GetAccessorDeclaration:
					case SyntaxKind.MethodDeclaration:
					case SyntaxKind.RemoveAccessorDeclaration:
					case SyntaxKind.SetAccessorDeclaration:
					case SyntaxKind.UnknownAccessorDeclaration:
						return null;
				}

				node = node.Parent;
			}

			return null;
		}

		static Location CombineLocations(Location location1, Location location2)
		{
			return Location.Create(
				location1.SourceTree,
				TextSpan.FromBounds(
					Math.Min(location1.SourceSpan.Start, location2.SourceSpan.Start),
					Math.Max(location1.SourceSpan.End, location2.SourceSpan.End)));
		}
	}
}
