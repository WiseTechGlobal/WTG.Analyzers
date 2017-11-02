using System;
using System.Collections.Immutable;
using System.Linq;
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
					ExpressionHelper.IsContainedInExpressionTree(context.SemanticModel, expression, context.CancellationToken)
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
