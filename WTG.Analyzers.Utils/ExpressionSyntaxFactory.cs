using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class ExpressionSyntaxFactory
	{
		public static ExpressionSyntax LogicalNot(ExpressionSyntax expression)
		{
			if (HasPrimaryOrUnaryPrecedence(expression))
			{
				return SyntaxFactory.PrefixUnaryExpression(
					SyntaxKind.LogicalNotExpression,
					expression.WithoutLeadingTrivia())
					.WithLeadingTrivia(expression.GetLeadingTrivia());
			}
			else
			{
				return SyntaxFactory.PrefixUnaryExpression(
					SyntaxKind.LogicalNotExpression,
					SyntaxFactory.ParenthesizedExpression(
						expression.WithoutTrivia()))
					.WithTriviaFrom(expression);
			}
		}

		static bool HasPrimaryOrUnaryPrecedence(ExpressionSyntax expression)
		{
			switch (expression.Kind())
			{
				// Primary
				case SyntaxKind.BaseExpression:
				case SyntaxKind.CharacterLiteralExpression:
				case SyntaxKind.CheckedExpression:
				case SyntaxKind.DefaultExpression:
				case SyntaxKind.ElementAccessExpression:
				case SyntaxKind.FalseLiteralExpression:
				case SyntaxKind.IdentifierName:
				case SyntaxKind.InterpolatedStringExpression:
				case SyntaxKind.InvocationExpression:
				case SyntaxKind.NullLiteralExpression:
				case SyntaxKind.NumericLiteralExpression:
				case SyntaxKind.NumericLiteralToken:
				case SyntaxKind.ObjectCreationExpression:
				case SyntaxKind.ObjectInitializerExpression:
				case SyntaxKind.ParenthesizedExpression:
				case SyntaxKind.PointerMemberAccessExpression:
				case SyntaxKind.PostDecrementExpression:
				case SyntaxKind.PostIncrementExpression:
				case SyntaxKind.SimpleMemberAccessExpression:
				case SyntaxKind.SizeOfExpression:
				case SyntaxKind.StringLiteralExpression:
				case SyntaxKind.ThisExpression:
				case SyntaxKind.TrueLiteralExpression:
				case SyntaxKind.TypeOfExpression:
				case SyntaxKind.UncheckedExpression:

				// Unary
				case SyntaxKind.AddressOfExpression:
				case SyntaxKind.CastExpression:
				case SyntaxKind.PointerIndirectionExpression:
				case SyntaxKind.PreDecrementExpression:
				case SyntaxKind.PreIncrementExpression:
				case SyntaxKind.UnaryMinusExpression:
				case SyntaxKind.UnaryPlusExpression:
					return true;

				default:
					return false;
			}
		}
	}
}
