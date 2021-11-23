using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using NUnit.Framework;
using WTG.Analyzers.TestFramework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class ExpressionSyntaxFactoryTest
	{
		[TestCase("42", ExpectedResult = "!42")]
		[TestCase("!42", ExpectedResult = "42")]
		[TestCase("identifier", ExpectedResult = "!identifier")]
		[TestCase("identifier1.identifier2", ExpectedResult = "!identifier1.identifier2")]
		[TestCase("a && b", ExpectedResult = "!(a && b)")]
		[TestCase("a || b", ExpectedResult = "!(a || b)")]
		[TestCase("a == b", ExpectedResult = "a != b")]
		[TestCase("a != b", ExpectedResult = "a == b")]
		[TestCase("a > b", ExpectedResult = "a <= b")]
		[TestCase("a < b", ExpectedResult = "a >= b")]
		[TestCase("a >= b", ExpectedResult = "a < b")]
		[TestCase("a <= b", ExpectedResult = "a > b")]
		[TestCase("true", ExpectedResult = "false")]
		[TestCase("false", ExpectedResult = "true")]
		public string InvertBoolExpression(string expressionString)
		{
			var baseExpression = SyntaxFactory.ParseExpression(expressionString);
			var expression = ExpressionSyntaxFactory.InvertBoolExpression(baseExpression);
			return expression.ToFullString();
		}

		[TestCase("42", ExpectedResult = "!42")]
		[TestCase("~42", ExpectedResult = "!~42")]
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
		public string CreateLiteralInt(int value)
		{
			return ExpressionSyntaxFactory.CreateLiteral(value).ToString();
		}

		[TestCase("", ExpectedResult = "\"\"")]
		[TestCase("Foo", ExpectedResult = "\"Foo\"")]
		[TestCase("\t", ExpectedResult = "\"\\t\"")]
		public string CreateLiteralString(string value)
		{
			return ExpressionSyntaxFactory.CreateLiteral(value).ToString();
		}

		[TestCase(0, ExpectedResult = "0L")]
		[TestCase(1, ExpectedResult = "1L")]
		[TestCase(-1, ExpectedResult = "-1L")]
		[TestCase(65536, ExpectedResult = "65536L")]
		public string CreateLiteralLong(long value)
		{
			return ExpressionSyntaxFactory.CreateLiteral(value).ToString();
		}

		[TestCase(0, ExpectedResult = "0f")]
		[TestCase(1, ExpectedResult = "1f")]
		[TestCase(-1, ExpectedResult = "-1f")]
		[TestCase(23.1406f, ExpectedResult = "23.1406f")]
		[TestCase(-23.1406f, ExpectedResult = "-23.1406f")]
		public string CreateLiteralFloat(float value)
		{
			return ExpressionSyntaxFactory.CreateLiteral(value).ToString();
		}

		[TestCase(0, ExpectedResult = "0d")]
		[TestCase(1, ExpectedResult = "1d")]
		[TestCase(-1, ExpectedResult = "-1d")]
		[TestCase(23.1406d, ExpectedResult = "23.1406d")]
		[TestCase(-23.1406d, ExpectedResult = "-23.1406d")]
		public string CreateLiteralDouble(double value)
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
			var annotation = new SyntaxAnnotation("Marker");
			var inner = SyntaxFactory.IdentifierName("Identifier").WithAdditionalAnnotations(annotation);
			var expression = ExpressionSyntaxFactory.CreateNameof(inner);

			Assert.That(expression.ToString(), Is.EqualTo("nameof(Identifier)"));
			Assert.That(expression.ArgumentList.Arguments[0].Expression.HasAnnotation(annotation), Is.True, "Annotations should survive the process.");
		}

		[Test]
		public void Nameof_DontHangOnToDeadWeight()
		{
			Assert.That(ExpressionSyntaxFactory.Nameof.Parent, Is.Null);
		}

		[Test]
		public async Task Nameof_Compilable()
		{
			var source =
@"class Bob
{
	public string Foo => ""X"";
}
";
			var document = ModelUtils.CreateDocument(source);
			var tree = await document.GetSyntaxTreeAsync().ConfigureAwait(false);
			var root = await tree.GetRootAsync().ConfigureAwait(false);
			var stringExpression = root.DescendantNodes().Single(x => x.IsKind(SyntaxKind.StringLiteralExpression));

			document = document.WithSyntaxRoot(
				root.ReplaceNode(
					stringExpression,
					ExpressionSyntaxFactory.CreateNameof(
						SyntaxFactory.IdentifierName("Bob"))));

			var compilation = await document.Project.GetCompilationAsync().ConfigureAwait(false);
			Assert.That(compilation.GetDiagnostics(), Is.Empty);
		}

		[Test]
		public void Nameof_Cached()
		{
			var nameof = ExpressionSyntaxFactory.Nameof;
			Assert.That(nameof, Is.SameAs(ExpressionSyntaxFactory.Nameof));
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
