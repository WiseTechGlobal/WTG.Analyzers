using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	[DiagnosticAnalyzer(LanguageNames.CSharp)]
	public sealed partial class TypeComparisonAnalyzer : DiagnosticAnalyzer
	{
		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(
			Rules.DoNotCompareGetTypeToANullableValueTypeRule);

		public override void Initialize(AnalysisContext context)
		{
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
			InvocationExpressionSyntax getTypeExpression;
			TypeOfExpressionSyntax typeofExpression;

			if ((typeofExpression = TypeLiteralVisitor.Instance.Visit(expression.Left)) != null)
			{
				getTypeExpression = GetTypeVisitor.Instance.Visit(expression.Right);
			}
			else if ((typeofExpression = TypeLiteralVisitor.Instance.Visit(expression.Right)) != null)
			{
				getTypeExpression = GetTypeVisitor.Instance.Visit(expression.Left);
			}
			else
			{
				return;
			}

			if (getTypeExpression != null &&
				IsGetTypeMethod(context.SemanticModel, getTypeExpression) &&
				IsNullableType(context.SemanticModel, typeofExpression))
			{
				context.ReportDiagnostic(
					Diagnostic.Create(
						Rules.DoNotCompareGetTypeToANullableValueTypeRule,
						expression.GetLocation()));
			}
		}

		static bool IsNullableType(SemanticModel model, TypeOfExpressionSyntax expression)
		{
			if (expression.Type.IsKind(SyntaxKind.NullableType))
			{
				return true;
			}

			var literalType = (ITypeSymbol)model.GetSymbolInfo(expression).Symbol;
			return literalType != null && literalType.SpecialType == SpecialType.System_Nullable_T;
		}

		static bool IsGetTypeMethod(SemanticModel model, InvocationExpressionSyntax expression)
		{
			var symbol = model.GetSymbolInfo(expression).Symbol;
			return symbol.Kind == SymbolKind.Method && ((IMethodSymbol)symbol).IsMatch("System.Object", "GetType");
		}

		sealed class GetTypeVisitor : CSharpSyntaxVisitor<InvocationExpressionSyntax>
		{
			public static GetTypeVisitor Instance { get; } = new GetTypeVisitor();

			GetTypeVisitor()
			{
			}

			public override InvocationExpressionSyntax DefaultVisit(SyntaxNode node) => null;
			public override InvocationExpressionSyntax VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => Visit(node.Expression);

			public override InvocationExpressionSyntax VisitInvocationExpression(InvocationExpressionSyntax node)
			{
				if (node.ArgumentList.Arguments.Count == 0)
				{
					var innerExpression = node.Expression;

					if (innerExpression.IsKind(SyntaxKind.SimpleMemberAccessExpression))
					{
						var member = (MemberAccessExpressionSyntax)innerExpression;

						if (member.Name.Identifier.Text == nameof(object.GetType))
						{
							return node;
						}
					}
				}

				return null;
			}
		}

		sealed class TypeLiteralVisitor : CSharpSyntaxVisitor<TypeOfExpressionSyntax>
		{
			public static TypeLiteralVisitor Instance { get; } = new TypeLiteralVisitor();

			TypeLiteralVisitor()
			{
			}

			public override TypeOfExpressionSyntax DefaultVisit(SyntaxNode node) => null;
			public override TypeOfExpressionSyntax VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => Visit(node.Expression);
			public override TypeOfExpressionSyntax VisitTypeOfExpression(TypeOfExpressionSyntax node) => node;
		}
	}
}
