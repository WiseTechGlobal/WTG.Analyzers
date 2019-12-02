using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	public class ExpressionHelperTest
	{
		[TestCase("Method()", ExpectedResult = "Method")]
		[TestCase("Instance.MethodName(42)", ExpectedResult = "MethodName")]
		[TestCase("Value?.Foo()", ExpectedResult = "Foo")]
		[TestCase("Value?.Bar<T>()", ExpectedResult = "Bar")]
		[TestCase("global::Name()", ExpectedResult = "Name")]
		public string GetMethodName(string expression)
		{
			var syntax = SyntaxFactory.ParseExpression(expression);

			var invoke = syntax.Kind() switch
			{
				SyntaxKind.InvocationExpression => (InvocationExpressionSyntax)syntax,
				SyntaxKind.ConditionalAccessExpression => (InvocationExpressionSyntax)((ConditionalAccessExpressionSyntax)syntax).WhenNotNull,
				_ => throw new ArgumentException("Invalid expression.", nameof(expression)),
			};

			return ExpressionHelper.GetMethodName(invoke).Identifier.Text;
		}
	}
}
