using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class ExpressionSyntaxFactory
	{
		public static IdentifierNameSyntax Nameof
		{
			get
			{
				if (nameofSyntax == null)
				{
					// This may seem stupid (lets face it, it is) but the identifier produced by SyntaxFactory.IdentifierName
					// has the wrong kind of magic for the compiler to recognise it as a contextual keyword.
					//
					// Error CS0103: The name 'nameof' does not exist in the current context
					var invoke = (InvocationExpressionSyntax)SyntaxFactory.ParseExpression("nameof(A)");
					var identifier = (IdentifierNameSyntax)invoke.Expression.WithoutTrivia();
					Interlocked.CompareExchange(ref nameofSyntax, identifier, null);
				}

				return nameofSyntax;
			}
		}

		public static ExpressionSyntax LogicalNot(ExpressionSyntax expression)
		{
			switch (expression.Kind())
			{
				case SyntaxKind.EqualsExpression:
					return ReplaceBinaryOperator(SyntaxKind.ExclamationEqualsToken);

				case SyntaxKind.NotEqualsExpression:
					return ReplaceBinaryOperator(SyntaxKind.EqualsEqualsToken);

				case SyntaxKind.LessThanExpression:
					return ReplaceBinaryOperator(SyntaxKind.GreaterThanEqualsToken);

				case SyntaxKind.GreaterThanExpression:
					return ReplaceBinaryOperator(SyntaxKind.LessThanEqualsToken);

				case SyntaxKind.LessThanOrEqualExpression:
					return ReplaceBinaryOperator(SyntaxKind.GreaterThanToken);

				case SyntaxKind.GreaterThanOrEqualExpression:
					return ReplaceBinaryOperator(SyntaxKind.LessThanToken);

				case SyntaxKind.LogicalNotExpression:
					return RemoveBang();
			}

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

			ExpressionSyntax ReplaceBinaryOperator(SyntaxKind operatorKind)
			{
				var b = (BinaryExpressionSyntax)expression;
				return b.WithOperatorToken(SyntaxFactory.Token(operatorKind).WithTriviaFrom(b.OperatorToken));
			}

			ExpressionSyntax RemoveBang()
			{
				var p = (PrefixUnaryExpressionSyntax)expression;
				return p.Operand.WithLeadingTrivia(p.GetLeadingTrivia());
			}
		}

		public static ExpressionSyntax CreateElementAccessExpression(ExpressionSyntax sourceExpression, ExpressionSyntax index)
		{
			return SyntaxFactory.ElementAccessExpression(
				sourceExpression,
				SyntaxFactory.BracketedArgumentList(
					SyntaxFactory.SingletonSeparatedList(
						SyntaxFactory.Argument(index))));
		}

		public static ExpressionSyntax CreateLiteral(int value)
		{
			return SyntaxFactory.LiteralExpression(
				SyntaxKind.NumericLiteralExpression,
				SyntaxFactory.Literal(value));
		}

		public static ExpressionSyntax CreateLiteral(string value)
		{
			return SyntaxFactory.LiteralExpression(
				SyntaxKind.StringLiteralExpression,
				SyntaxFactory.Literal(value));
		}

		public static InvocationExpressionSyntax CreateNameof(ExpressionSyntax argument)
		{
			return SyntaxFactory.InvocationExpression(
				Nameof,
				SyntaxFactory.ArgumentList(
					SyntaxFactory.SeparatedList(
						new[] { SyntaxFactory.Argument(argument) })));
		}

		[SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms")]
		public static ExpressionSyntax CreateSingleBitFlag(int index)
		{
			return SyntaxFactory.BinaryExpression(
				SyntaxKind.LeftShiftExpression,
				CreateLiteral(1)
					.WithTrailingTrivia(SyntaxFactory.Space),
				CreateLiteral(index)
					.WithLeadingTrivia(SyntaxFactory.Space));
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
				case SyntaxKind.BitwiseNotExpression:
				case SyntaxKind.LogicalNotExpression:
					return true;

				default:
					return false;
			}
		}

		static SyntaxTriviaList ConcatTrivia(SyntaxTriviaList triviaList1, SyntaxTriviaList triviaList2)
		{
			if (triviaList1.Count == 0)
			{
				return triviaList2;
			}
			else if (triviaList2.Count == 0)
			{
				return triviaList1;
			}
			else
			{
				return triviaList1.AddRange(new[] { SyntaxFactory.ElasticMarker }.Concat(triviaList2));
			}
		}

		static IdentifierNameSyntax nameofSyntax;
	}
}
