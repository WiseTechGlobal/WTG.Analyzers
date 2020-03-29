using System;
using System.Globalization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using WTG.Analyzers.Utils;

namespace WTG.Analyzers
{
	sealed class EmitConversionVisitor : CSharpSyntaxVisitor<ExpressionSyntax>
	{
		public EmitConversionVisitor(OpCodeOperand operandType)
		{
			this.operandType = operandType;
		}

		public override ExpressionSyntax DefaultVisit(SyntaxNode node)
		{
			return SyntaxFactory.CastExpression(
				GetCastType(),
				SyntaxFactory.ParenthesizedExpression((ExpressionSyntax)node)
					.WithAdditionalAnnotations(Simplifier.Annotation));
		}

		public override ExpressionSyntax VisitLiteralExpression(LiteralExpressionSyntax node)
		{
			if (node.Kind() == SyntaxKind.NumericLiteralExpression)
			{
				try
				{
					switch (operandType)
					{
						case OpCodeOperand.InlineI:
							return ExpressionSyntaxFactory.CreateLiteral(Convert.ToInt32(node.Token.Value, CultureInfo.InvariantCulture));
						case OpCodeOperand.InlineI8:
							return ExpressionSyntaxFactory.CreateLiteral(Convert.ToInt64(node.Token.Value, CultureInfo.InvariantCulture));
						case OpCodeOperand.ShortInlineR:
							return ExpressionSyntaxFactory.CreateLiteral(Convert.ToSingle(node.Token.Value, CultureInfo.InvariantCulture));
						case OpCodeOperand.InlineR:
							return ExpressionSyntaxFactory.CreateLiteral(Convert.ToDouble(node.Token.Value, CultureInfo.InvariantCulture));
					}
				}
				catch (OverflowException)
				{
				}
			}

			return base.VisitLiteralExpression(node);
		}

		public override ExpressionSyntax VisitCastExpression(CastExpressionSyntax node) => node.Expression.Accept(this);

		TypeSyntax? GetCastType()
		{
			return operandType switch
			{
				OpCodeOperand.ShortInlineI => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.SByteKeyword)),
				OpCodeOperand.ShortInlineVar => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ByteKeyword)),

				OpCodeOperand.InlineI => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword)),
				OpCodeOperand.InlineVar => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.ShortKeyword)),

				OpCodeOperand.InlineI8 => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.LongKeyword)),

				OpCodeOperand.InlineR => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.DoubleKeyword)),
				OpCodeOperand.ShortInlineR => SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.FloatKeyword)),

				_ => null,
			};
		}

		readonly OpCodeOperand operandType;
	}
}
