using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers.Utils
{
	public static class AttributeUtils
	{
		public static ExpressionSyntax GetArgumentValue(this AttributeSyntax att, int index)
		{
			var arguments = att.ArgumentList.Arguments;

			if (index < 0 || index >= arguments.Count)
			{
				return null;
			}

			var arg = arguments[index];

			if (arg.NameEquals != null)
			{
				return null;
			}

			return arg.Expression;
		}

		public static ExpressionSyntax GetPropertyValue(this AttributeSyntax att, string name)
		{
			var arguments = att.ArgumentList.Arguments;

			foreach (var arg in arguments)
			{
				if (arg?.NameEquals?.Name?.Identifier.Text == name)
				{
					return arg.Expression;
				}
			}

			return null;
		}
	}
}
