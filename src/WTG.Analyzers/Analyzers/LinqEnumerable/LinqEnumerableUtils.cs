using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	public static class LinqEnumerableUtils
	{
		public static InitializerExpressionSyntax? GetInitializer(ExpressionSyntax e)
		{
			return e.Kind() switch
			{
				SyntaxKind.ObjectCreationExpression => ((ObjectCreationExpressionSyntax)e).Initializer,
				SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)e).Initializer,
				SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)e).Initializer,
				_ => null,
			};
		}

		public static ExpressionSyntax? GetFirstValue(ExpressionSyntax e)
		{
			var initializer = GetInitializer(e);
			var expression = initializer?.Expressions[0];

			if (expression.IsKind(SyntaxKind.ComplexElementInitializerExpression))
			{
				initializer = (InitializerExpressionSyntax)expression;
				return initializer.Expressions[0];
			}

			return expression;
		}

		public static IEnumerable<ExpressionSyntax> GetValues(ExpressionSyntax e)
		{
			var initializer = GetInitializer(e);
			if (initializer is null)
			{
				return Enumerable.Empty<ExpressionSyntax>();
			}

			var values = new List<ExpressionSyntax>();

			foreach (var expression in initializer.Expressions)
			{
				if (expression.IsKind(SyntaxKind.ComplexElementInitializerExpression))
				{
					var inner = (InitializerExpressionSyntax)expression;
					values.Add(inner.Expressions[0]);
				}
				else
				{
					values.Add(expression);
				}
			}

			return values;
		}

		public static ExpressionSyntax TryGetExpressionFromParenthesizedExpression (this ExpressionSyntax expression)
		{
			while (expression.IsKind(SyntaxKind.ParenthesizedExpression))
			{
				expression = ((ParenthesizedExpressionSyntax)expression).Expression;
			}

			return expression;
		}
	}
}
