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
			switch (expression.Kind())
			{
				case SyntaxKind.CharacterLiteralExpression:
				case SyntaxKind.NumericLiteralExpression:
				case SyntaxKind.CastExpression:
				case SyntaxKind.UnaryMinusExpression:
				case SyntaxKind.UnaryPlusExpression:
					break;

				default:
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
	}
}
