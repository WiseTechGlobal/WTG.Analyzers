using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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

		public static SyntaxNode? PrependFix(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax)m.Parent!;

			var arguments = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					ExpressionSyntax expression = m.Expression;

					while (expression.IsKind(SyntaxKind.ParenthesizedExpression))
					{
						expression = ((ParenthesizedExpressionSyntax)expression).Expression;
					}

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

		// this function exploits the fact that ImplicitArrayCreationExpression, ArrayCreationExpression,
		// and ObjectCreationExpression are already ordered appropriately in the SyntaxKind enum
		// ImplicitArrayCreationExpression = 8652
		// ArrayCreationExpression = 8651
		// ObjectCreationExpression = 8649
		public static SyntaxNode JoinFix(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax)m.Parent!;

			ExpressionSyntax? a = null, b = null;

			var prepend = false;

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:

					var expression = m.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)m.Expression).Expression : m.Expression;
					var argument = invocation.ArgumentList.Arguments.First().Expression;

					if (expression.RawKind < argument.RawKind)
					{
						(a, b) = (argument, expression);
						prepend = true;
					}
					else
					{
						(a, b) = (expression, argument);
					}

					break;
				case 2:

					var firstArgument = invocation.ArgumentList.Arguments[0].Expression;
					var secondArgument = invocation.ArgumentList.Arguments[1].Expression;

					if (firstArgument.RawKind < secondArgument.RawKind)
					{
						(a, b) = (secondArgument, firstArgument);
						prepend = true;
					}
					else
					{
						(a, b) = (firstArgument, secondArgument);
					}

					break;
			}

			SeparatedSyntaxList<ExpressionSyntax> initializer;

			if (prepend)
			{
				initializer = GetInitializer(a)!.Expressions.Insert(0, GetValue(b)!);
			}
			else
			{
				initializer = GetInitializer(a)!.Expressions.Add(GetValue(b)!);
			}

			SyntaxKind syntaxKind = a!.Kind() switch
			{
				SyntaxKind.ImplicitArrayCreationExpression => SyntaxKind.ArrayInitializerExpression,
				SyntaxKind.ArrayCreationExpression => SyntaxKind.ArrayInitializerExpression,
				SyntaxKind.ObjectCreationExpression => SyntaxKind.ObjectInitializerExpression,
				_ => SyntaxKind.ArrayInitializerExpression,
			};

			return a!.ReplaceNode(GetInitializer(a)!, InitializerExpression(syntaxKind, initializer))
					.WithLeadingTrivia(invocation.GetLeadingTrivia())
					.WithTrailingTrivia(invocation.GetTrailingTrivia());
		}
	}
}
