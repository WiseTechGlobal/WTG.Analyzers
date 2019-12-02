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
			return value switch
			{
				int s => s == 0,
				uint s => s == 0,
				long s => s == 0,
				ulong s => s == 0,
				byte s => s == 0,
				short s => s == 0,
				ushort s => s == 0,
				sbyte s => s == 0,
				char s => s == 0,
				_ => false,
			};
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
