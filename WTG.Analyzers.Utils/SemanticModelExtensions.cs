using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class SemanticModelExtensions
	{
		public static bool IsConstantZero(this SemanticModel model, ExpressionSyntax expression, CancellationToken cancellationToken)
		{
			if (!expression.Accept(IsLiteralVisitor.Instance))
			{
				return false;
			}

			var constant = model.GetConstantValue(expression, cancellationToken);

			return constant.HasValue && IsZeroLiteral(constant.Value);
		}

		static bool IsZeroLiteral(object value)
		{
			switch (value)
			{
				case int s:
					return s == 0;

				case uint s:
					return s == 0;

				case long s:
					return s == 0;

				case ulong s:
					return s == 0;

				case byte s:
					return s == 0;

				case short s:
					return s == 0;

				case ushort s:
					return s == 0;

				case sbyte s:
					return s == 0;

				case char s:
					return s == 0;

				default:
					return false;
			}
		}

		sealed class IsLiteralVisitor : CSharpSyntaxVisitor<bool>
		{
			public static IsLiteralVisitor Instance { get; } = new IsLiteralVisitor();

			IsLiteralVisitor()
			{
			}

			public override bool DefaultVisit(SyntaxNode node) => false;
			public override bool VisitParenthesizedExpression(ParenthesizedExpressionSyntax node) => node.Expression.Accept(this);
			public override bool VisitCastExpression(CastExpressionSyntax node) => node.Expression.Accept(this);

			public override bool VisitLiteralExpression(LiteralExpressionSyntax node)
			{
				switch (node.Kind())
				{
					case SyntaxKind.NumericLiteralExpression:
					case SyntaxKind.CharacterLiteralExpression:
						return true;
				}

				return false;
			}

			public override bool VisitPrefixUnaryExpression(PrefixUnaryExpressionSyntax node)
			{
				switch (node.Kind())
				{
					case SyntaxKind.UnaryPlusExpression:
					case SyntaxKind.UnaryMinusExpression:
						return node.Operand.Accept(this);
				}

				return false;
			}
		}
	}
}
