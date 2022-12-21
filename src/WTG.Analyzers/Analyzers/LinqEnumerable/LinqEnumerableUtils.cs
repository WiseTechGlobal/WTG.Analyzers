using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Workspaces;
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

		public static SyntaxNode FixConcatWithNewCollection(MemberAccessExpressionSyntax m, SemanticModel? semanticModel)
		{
			var invocation = (InvocationExpressionSyntax?)m.Parent;

			// we should never get here - this would be insane
			if (invocation == null)
			{
				return m;
			}

			if (semanticModel == null)
			{
				return invocation;
			}

			LiteralExpressionSyntax a, b;

			switch (invocation.ArgumentList.Arguments.Count)
			{
				case 1:
					var expression = m.Expression.IsKind(SyntaxKind.ParenthesizedExpression) ? ((ParenthesizedExpressionSyntax)m.Expression).GetExpression() : m.Expression;

					a = (LiteralExpressionSyntax)GetValue(expression)!;
					b = (LiteralExpressionSyntax)GetValue(invocation.ArgumentList.Arguments[0].Expression)!;
					break;
				case 2:
					a = (LiteralExpressionSyntax)GetValue(invocation.ArgumentList.Arguments[0].Expression)!;
					b = (LiteralExpressionSyntax)GetValue(invocation.ArgumentList.Arguments[1].Expression)!;
					break;
				default:
					return invocation;
			}

			//ITypeSymbol? typeA = semanticModel.GetTypeInfo(a).Type, typeB = semanticModel.GetTypeInfo(b).Type;

			/*if (a.IsKind(SyntaxKind.NullLiteralExpression))
			{
				return ArrayCreationExpression(
						Token(SyntaxKind.NewKeyword),
						ArrayType(
							NullableType(
								GetTypeSyntax(typeA))),
						InitializerExpression(
							SyntaxKind.ArrayCreationExpression,
							SeparatedList<ExpressionSyntax>(
								new SyntaxNodeOrToken[] { a, b })));
			}

			if (b.IsKind(SyntaxKind.NullLiteralExpression))
			{
				return ArrayCreationExpression(
						Token(SyntaxKind.NewKeyword),
						ArrayType(
							NullableType(
								GetTypeSyntax(typeB))),
						InitializerExpression(
							SyntaxKind.ArrayCreationExpression,
							SeparatedList<ExpressionSyntax>(
								new SyntaxNodeOrToken[] { a, b })));
			}

			if (SymbolEqualityComparer.Default.Equals(typeA, typeB))
			{
				return ImplicitArrayCreationExpression(
						InitializerExpression(
							SyntaxKind.ImplicitArrayCreationExpression,
							SeparatedList<ExpressionSyntax>(
								new SyntaxNodeOrToken[] { a, b })));
			}

			return invocation;*/

			return ImplicitArrayCreationExpression(
					InitializerExpression(
						SyntaxKind.ArrayInitializerExpression,
						SeparatedList<ExpressionSyntax>(
							new List<SyntaxNodeOrToken>() { a, Token(SyntaxKind.CommaToken), b })));
		}

		public static TypeSyntax GetTypeSyntax (ITypeSymbol? type)
		{
			switch (type?.SpecialType)
			{
				case SpecialType.System_Boolean:
					return PredefinedType(Token(SyntaxKind.BoolKeyword));
				case SpecialType.System_Char:
					return PredefinedType(Token(SyntaxKind.CharKeyword));
				case SpecialType.System_Int16:
					return PredefinedType(Token(SyntaxKind.ShortKeyword));
				case SpecialType.System_Int32:
					return PredefinedType(Token(SyntaxKind.IntKeyword));
				case SpecialType.System_Byte:
					return PredefinedType(Token(SyntaxKind.ByteKeyword));
				case SpecialType.System_UInt16:
					return PredefinedType(Token(SyntaxKind.UShortKeyword));
				case SpecialType.System_UInt32:
					return PredefinedType(Token(SyntaxKind.UIntKeyword));
				case SpecialType.System_UInt64:
					return PredefinedType(Token(SyntaxKind.ULongKeyword));
				case SpecialType.System_Object:
					return PredefinedType(Token(SyntaxKind.ObjectKeyword));
				case SpecialType.System_String:
					return PredefinedType(Token(SyntaxKind.StringKeyword));
				case SpecialType.System_Single:
					return PredefinedType(Token(SyntaxKind.FloatKeyword));
				case SpecialType.System_Double:
					return PredefinedType(Token(SyntaxKind.DoubleKeyword));
				case SpecialType.System_Decimal:
					return PredefinedType(Token(SyntaxKind.DecimalKeyword));
				default:
					return PredefinedType(Token(SyntaxKind.ObjectKeyword));
			}
		}
	}
}
