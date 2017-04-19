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

			var visitor = new IsExpressionVisitor(context.SemanticModel, context.CancellationToken);

			context.ReportDiagnostic(
				Diagnostic.Create(
					IsContainedInExpressionTree(visitor, expression)
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

		static bool IsContainedInExpressionTree(IsExpressionVisitor visitor, ExpressionSyntax expression)
		{
			for (SyntaxNode node = expression; node != null; node = node.Parent)
			{
				var result = visitor.Visit(node);

				if (result.HasValue)
				{
					return result.Value;
				}
			}

			return false;
		}

		static Location CombineLocations(Location location1, Location location2)
		{
			return Location.Create(
				location1.SourceTree,
				TextSpan.FromBounds(
					Math.Min(location1.SourceSpan.Start, location2.SourceSpan.Start),
					Math.Max(location1.SourceSpan.End, location2.SourceSpan.End)));
		}

		sealed class IsExpressionVisitor : CSharpSyntaxVisitor<bool?>
		{
			public IsExpressionVisitor(SemanticModel model, CancellationToken cancellationToken)
			{
				this.model = model;
				this.cancellationToken = cancellationToken;
			}

			public override bool? VisitAccessorDeclaration(AccessorDeclarationSyntax node) => false;
			public override bool? VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node) => false;
			public override bool? VisitMethodDeclaration(MethodDeclarationSyntax node) => false;

			public override bool? VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node) => VisitLambdaExpressionSyntax(node);
			public override bool? VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node) => VisitLambdaExpressionSyntax(node);

			public override bool? VisitWhereClause(WhereClauseSyntax node) => IsExpressionFromQueryClauseSymbol(node);
			public override bool? VisitOrdering(OrderingSyntax node) => IsExpressionFromNodeSymbol(node);
			public override bool? VisitLetClause(LetClauseSyntax node) => IsExpressionFromQueryClauseSymbol(node);
			public override bool? VisitGroupClause(GroupClauseSyntax node) => IsExpressionFromNodeSymbol(node);
			public override bool? VisitSelectClause(SelectClauseSyntax node) => IsExpressionFromNodeSymbol(node);

			bool IsExpressionFromQueryClauseSymbol(QueryClauseSyntax node)
			{
				var clauseInfo = model.GetQueryClauseInfo(node, cancellationToken);
				var method = (IMethodSymbol)clauseInfo.OperationInfo.Symbol;
				return method != null && IsExpressionFromLinqMethod(method);
			}

			bool IsExpressionFromNodeSymbol(SyntaxNode node)
			{
				var method = (IMethodSymbol)model.GetSymbolInfo(node).Symbol;
				return method != null && IsExpressionFromLinqMethod(method);
			}

			bool? VisitLambdaExpressionSyntax(LambdaExpressionSyntax node)
			{
				var type = model.GetTypeInfo(node, cancellationToken).ConvertedType;
				return type != null && IsExpression(type);
			}

			static bool IsExpressionFromLinqMethod(IMethodSymbol method)
			{
				return IsExpression(method.Parameters[method.IsStatic ? 1 : 0].Type);
			}

			static bool IsExpression(ITypeSymbol type) => type.IsMatch("System.Core", "System.Linq.Expressions.Expression`1");

			readonly SemanticModel model;
			readonly CancellationToken cancellationToken;
		}
	}
}
