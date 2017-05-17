using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	sealed class AnyLinqMethod : LinqMethod
	{
		public override bool IsMatch(IMethodSymbol method) => IsEnumerableLinqMethod(method, nameof(Enumerable.Any), 1);
		public override LinqResolution GetResolution(ITypeSymbol sourceType)
		{
			if (sourceType.Kind == SymbolKind.ArrayType)
			{
				return Resolution.UseLength;
			}
			else if (HasCountProperty(sourceType))
			{
				return Resolution.UseCount;
			}
			else if (HasLengthProperty(sourceType))
			{
				return Resolution.UseLength;
			}

			return null;
		}

		sealed class Resolution : LinqResolution
		{
			public static Resolution UseLength { get; } = new Resolution("Length");
			public static Resolution UseCount { get; } = new Resolution("Count");

			Resolution(string name)
			{
				this.name = name;
			}

			public override Diagnostic CreateDiagnostic(InvocationExpressionSyntax invoke, ITypeSymbol sourceType, bool isInAnExpression)
			{
				return CreatePropertyDiagnostic(invoke, sourceType, nameof(Enumerable.Any) + "()", name, isInAnExpression);
			}

			public override ExpressionSyntax ApplyFix(InvocationExpressionSyntax invoke)
			{
				var result = SyntaxFactory.BinaryExpression(
					SyntaxKind.GreaterThanExpression,
					CreatePropertyAccessExpression(invoke, name),
					SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, SyntaxFactory.Literal(0)));

				if (ShouldParenthesize(invoke))
				{
					return SyntaxFactory.ParenthesizedExpression(result.WithoutTrivia())
						.WithLeadingTrivia(result.GetLeadingTrivia())
						.WithTrailingTrivia(result.GetTrailingTrivia());
				}

				return result;
			}

			static bool ShouldParenthesize(InvocationExpressionSyntax invoke)
			{
				var parent = invoke.Parent;

				if (parent == null)
				{
					return false;
				}

				switch (parent.Kind())
				{
					case SyntaxKind.Argument:
					case SyntaxKind.ArrowExpressionClause:
					case SyntaxKind.IfStatement:
					case SyntaxKind.LetClause:
					case SyntaxKind.LogicalAndExpression:
					case SyntaxKind.LogicalOrExpression:
					case SyntaxKind.ParenthesizedExpression:
					case SyntaxKind.ParenthesizedLambdaExpression:
					case SyntaxKind.ReturnStatement:
					case SyntaxKind.SelectClause:
					case SyntaxKind.SimpleLambdaExpression:
					case SyntaxKind.WhereClause:
					case SyntaxKind.WhileStatement:
						return false;
				}

				return true;
			}

			readonly string name;
		}
	}
}
