using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Analyzers.LinqEnumerable
{
	public static class LinqEnumerableUtils
	{
		public static InitializerExpressionSyntax? GetInitializer(ExpressionSyntax? e)
		{
			return e?.Kind() switch
			{
				SyntaxKind.ObjectCreationExpression => ((ObjectCreationExpressionSyntax)e).Initializer,
				SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)e).Initializer,
				SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)e).Initializer,
				_ => null,
			};
		}

		public static ExpressionSyntax? GetValue(ExpressionSyntax? e)
		{
			var initializer = GetInitializer(e);

			return initializer?.Expressions[0];
		}

		public static ExpressionSyntax TryGetExpressionFromParenthesizedExpression (this ExpressionSyntax expression)
		{
			if (!expression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				return expression;
			}

			var unwrappedExpression = ((ParenthesizedExpressionSyntax)expression).Expression;

			while (unwrappedExpression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				unwrappedExpression = ((ParenthesizedExpressionSyntax)unwrappedExpression).Expression;
			}

			return unwrappedExpression;
		}
	}
}
