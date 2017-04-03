using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	class ExpressionSyntaxFactoryTest
	{
		[TestCase("42", ExpectedResult = "!42")]
		[TestCase("identifier", ExpectedResult = "!identifier")]
		[TestCase("identifier1.identifier2", ExpectedResult = "!identifier1.identifier2")]
		[TestCase("21 * 2", ExpectedResult = "!(21 * 2)")]
		[TestCase("40 + 2", ExpectedResult = "!(40 + 2)")]
		public string LogicalNot(string expressionString)
		{
			var baseExpression = SyntaxFactory.ParseExpression(expressionString);
			var expression = ExpressionSyntaxFactory.LogicalNot(baseExpression);
			return expression.ToFullString();
		}
	}
}
