using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	internal static class ExpressionSyntaxExtensions
	{
		public static bool TryGetStringValue(this ExpressionSyntax expression, out string value)
		{
			var literal = expression as LiteralExpressionSyntax;

			if (literal != null)
			{
				value = (string)literal.Token.Value;
				return true;
			}

			value = null;
			return false;
		}
	}
}
