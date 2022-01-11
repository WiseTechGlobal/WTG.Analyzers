using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	abstract class LinqResolution
	{
		protected LinqResolution()
		{
		}

		public abstract Diagnostic CreateDiagnostic(InvocationExpressionSyntax invoke, ITypeSymbol sourceType, bool isInAnExpression);
		public abstract ExpressionSyntax ApplyFix(InvocationExpressionSyntax invoke);

		protected static ExpressionSyntax CreatePropertyAccessExpression(InvocationExpressionSyntax invoke, string propertyName)
		{
			var arguments = invoke.ArgumentList.Arguments;

			var sourceExpression = arguments.Count > 0
				? arguments[0].Expression
				: LinqUtils.GetInstanceExpression(invoke);

			if (sourceExpression == null)
			{
				return invoke;
			}

			return SyntaxFactory.MemberAccessExpression(
				SyntaxKind.SimpleMemberAccessExpression,
				sourceExpression,
				SyntaxFactory.IdentifierName(propertyName));
		}
	}
}
