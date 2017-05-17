using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NUnit.Framework;

namespace WTG.Analyzers.Utils.Test
{
	[TestFixture]
	class ExpressionHelperTest
	{
		[TestCase("Method()", ExpectedResult = "Method")]
		[TestCase("Instance.MethodName(42)", ExpectedResult = "MethodName")]
		[TestCase("Value?.Foo()", ExpectedResult = "Foo")]
		[TestCase("Value?.Bar<T>()", ExpectedResult = "Bar")]
		[TestCase("global::Name()", ExpectedResult = "Name")]
		public string GetMethodName(string expression)
		{
			var syntax = SyntaxFactory.ParseExpression(expression);
			InvocationExpressionSyntax invoke;

			switch (syntax.Kind())
			{
				case SyntaxKind.InvocationExpression:
					invoke = (InvocationExpressionSyntax)syntax;
					break;

				case SyntaxKind.ConditionalAccessExpression:
					invoke = (InvocationExpressionSyntax)((ConditionalAccessExpressionSyntax)syntax).WhenNotNull;
					break;

				default:
					throw new ArgumentException("Invalid expression.", nameof(expression));
			}

			return ExpressionHelper.GetMethodName(invoke).Identifier.Text;
		}
	}
}
