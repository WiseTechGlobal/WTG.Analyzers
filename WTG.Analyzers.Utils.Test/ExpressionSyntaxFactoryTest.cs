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

		[TestCase(0, ExpectedResult = "0")]
		[TestCase(1, ExpectedResult = "1")]
		[TestCase(-1, ExpectedResult = "-1")]
		[TestCase(65536, ExpectedResult = "65536")]
		public string CreateLiteral(int value)
		{
			return ExpressionSyntaxFactory.CreateLiteral(value).ToString();
		}

		[TestCase(0, ExpectedResult = "1 << 0")]
		[TestCase(1, ExpectedResult = "1 << 1")]
		[TestCase(32, ExpectedResult = "1 << 32")]
		public string CreateSingleBitFlag(int index)
		{
			return ExpressionSyntaxFactory.CreateSingleBitFlag(index).ToString();
		}

		[Test]
		public void Nameof()
		{
			var expression = ExpressionSyntaxFactory.CreateNameof(SyntaxFactory.IdentifierName("Identifier"));

			Assert.That(expression.ToString(), Is.EqualTo("nameof(Identifier)"));
		}

		[Test]
		public void CreateElementAccess()
		{
			var collectionExpression = SyntaxFactory.IdentifierName("collection");
			var indexExpression = SyntaxFactory.IdentifierName("index");
			var result = ExpressionSyntaxFactory.CreateElementAccessExpression(collectionExpression, indexExpression);
			Assert.That(result.ToString(), Is.EqualTo("collection[index]"));
		}
	}
}
