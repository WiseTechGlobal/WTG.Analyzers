using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace WTG.Analyzers.Analyzers.LinqEnumerable
{
	public static class LinqEnumerableUtils
	{
		public static SyntaxNode? FixMemberAccessExpression(MemberAccessExpressionSyntax memberAccessExpression, Diagnostic diagnostic)
		{
			return diagnostic.Id switch
			{
				Rules.DontUseConcatWhenAppendingSingleElementToEnumerablesDiagnosticID => AppendFix(memberAccessExpression),
				Rules.DontUseConcatWhenPrependingSingleElementToEnumerablesDiagnosticID => PrependFix(memberAccessExpression),
				Rules.DontConcatTwoCollectionsDefinedWithLiteralsDiagnosticID => JoinFix(memberAccessExpression),
				_ => null,
			};
		}

		public static InitializerExpressionSyntax? GetInitializer(ExpressionSyntax e)
		{
			return e.Kind() switch
			{
				SyntaxKind.ObjectCreationExpression => ((ObjectCreationExpressionSyntax)e).Initializer!,
				SyntaxKind.ArrayCreationExpression => ((ArrayCreationExpressionSyntax)e).Initializer!,
				SyntaxKind.ImplicitArrayCreationExpression => ((ImplicitArrayCreationExpressionSyntax)e).Initializer!,
				_ => null,
			};
		}

		public static ExpressionSyntax? GetValue(ExpressionSyntax e)
		{
			var initializer = GetInitializer(e);
			if (initializer == null)
			{
				return null;
			}

			return initializer.Expressions.First();
		}

		public static SyntaxNode AppendFix(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax)m.Parent!;

			var arguments = new List<SyntaxNodeOrToken>();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					arguments.Add(Argument(GetValue(invocation.ArgumentList.Arguments[0].Expression)!));
					break;
				case 2:
					arguments.Add(invocation.ArgumentList.Arguments[0]);
					arguments.Add(Token(SyntaxKind.CommaToken));
					arguments.Add(Argument(GetValue(invocation.ArgumentList.Arguments[1].Expression)!));
					break;
			}

			return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							IdentifierName(m.Expression.ToString()),
							IdentifierName("Append")))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(arguments)))
					.WithLeadingTrivia(m.Parent!.GetLeadingTrivia())
					.WithTrailingTrivia(m.Parent!.GetTrailingTrivia());
		}

		public static SyntaxNode? PrependFix(MemberAccessExpressionSyntax m)
		{
			var invocation = (InvocationExpressionSyntax)m.Parent!;

			var arguments = new List<SyntaxNodeOrToken>();

			string identifier = m.Expression.ToString();

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					var expression = m.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)m.Expression).Expression : m.Expression;
					identifier = invocation.ArgumentList.Arguments.First().ToString();
					arguments.Add(Argument(GetValue(expression)!));
					break;
				case 2:
					arguments.Add(invocation.ArgumentList.Arguments[1]);
					arguments.Add(Token(SyntaxKind.CommaToken));
					arguments.Add(Argument(GetValue(invocation.ArgumentList.Arguments[0].Expression)!));
					break;
			}

			return InvocationExpression(
						MemberAccessExpression(
							SyntaxKind.SimpleMemberAccessExpression,
							IdentifierName(identifier),
							IdentifierName("Prepend")))
					.WithArgumentList(
						ArgumentList(
							SeparatedList<ArgumentSyntax>(arguments)))
					.WithLeadingTrivia(invocation.GetLeadingTrivia())
					.WithTrailingTrivia(invocation.GetTrailingTrivia());
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

					var firstArgument = invocation.ArgumentList.Arguments.First().Expression;
					var secondArgument = invocation.ArgumentList.Arguments.Last().Expression;

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
				initializer = GetInitializer(a!)!.Expressions.Insert(0, GetValue(b!)!);
			}
			else
			{
				initializer = GetInitializer(a!)!.Expressions.Add(GetValue(b!)!);
			}

			SyntaxKind syntaxKind = a!.Kind() switch
			{
				SyntaxKind.ImplicitArrayCreationExpression => SyntaxKind.ArrayInitializerExpression,
				SyntaxKind.ArrayCreationExpression => SyntaxKind.ArrayInitializerExpression,
				SyntaxKind.ObjectCreationExpression => SyntaxKind.ObjectInitializerExpression,
				_ => SyntaxKind.ArrayInitializerExpression,
			};

			return a!.ReplaceNode(GetInitializer(a!)!, InitializerExpression(syntaxKind, initializer))
					.WithLeadingTrivia(m.Parent!.GetLeadingTrivia())
					.WithTrailingTrivia(m.Parent!.GetTrailingTrivia());
		}
	}
}
