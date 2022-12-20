using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WTG.Analyzers.Utils;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

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
				_ => throw new NotImplementedException(), // this should never happen - the analyzer should only look for lists, strongly typed arrays, and implicitly typed arrays
			};
		}

		public static ExpressionSyntax? GetValue(ExpressionSyntax? e)
		{
			var initializer = GetInitializer(e);

			return initializer?.Expressions.First();
		}

		public static SyntaxNode FixConcatWithAppendMethod(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax)m.Parent!;

			var listOfArgumentsAndSeparators = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					listOfArgumentsAndSeparators.Add(Argument(GetValue(invocation.ArgumentList.Arguments[0].Expression)!));
					break;
				case 2:
					listOfArgumentsAndSeparators.Add(invocation.ArgumentList.Arguments[0]);
					listOfArgumentsAndSeparators.Add(Token(SyntaxKind.CommaToken));
					listOfArgumentsAndSeparators.Add(Argument(GetValue(invocation.ArgumentList.Arguments[1].Expression)!));
					break;
			}

			return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							IdentifierName(m.Expression.ToString()),
							IdentifierName(nameof(Enumerable.Append))))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(listOfArgumentsAndSeparators)))
					.WithLeadingTrivia(invocation.GetLeadingTrivia())
					.WithTrailingTrivia(invocation.GetTrailingTrivia());
		}

		public static SyntaxNode? FixConcatWithPrependMethod(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;

			if (invocation == null)
			{
				return m;
			}

			var arguments = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					var expression = m.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)m.Expression).GetExpression() : m.Expression;

					arguments.Add(Argument(GetValue(expression)!));

					IdentifierNameSyntax identifier = (IdentifierNameSyntax)invocation.ArgumentList.Arguments[0].Expression;

					return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							identifier,
							IdentifierName(nameof(Enumerable.Prepend))))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(arguments)))
					.WithLeadingTrivia(invocation.GetLeadingTrivia())
					.WithTrailingTrivia(invocation.GetTrailingTrivia());
				case 2:

					var value = GetValue(invocation.ArgumentList.Arguments[0].Expression);

					if (value == null)
					{
						return invocation;
					}	

					arguments.Add(invocation.ArgumentList.Arguments[1]);
					arguments.Add(Token(SyntaxKind.CommaToken));
					arguments.Add(Argument(value));

					return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							IdentifierName(nameof(Enumerable)),
							IdentifierName(nameof(Enumerable.Prepend))))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(arguments)))
					.WithLeadingTrivia(invocation.GetLeadingTrivia())
					.WithTrailingTrivia(invocation.GetTrailingTrivia());
				default:
					return invocation;
			}
		}

		public static SyntaxNode FixConcatWithNewCollection(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;

			if (invocation == null)
			{
				return m;
			}

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					break;
				case 2:
					break;
				default:
					return invocation;
			}
		}
	}
}
