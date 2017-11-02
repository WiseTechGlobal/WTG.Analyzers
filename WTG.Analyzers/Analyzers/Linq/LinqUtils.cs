using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class LinqUtils
	{
		/// <summary>
		/// This can only work if you have already ruled out the possibility that you might be invoking a delegate.
		/// </summary>
		public static ExpressionSyntax GetInstanceExpression(InvocationExpressionSyntax invoke)
		{
			var inner = invoke.Expression;
			var kind = inner.Kind();

			if (kind == SyntaxKind.SimpleMemberAccessExpression)
			{
				return ((MemberAccessExpressionSyntax)inner).Expression;
			}

			return null;
		}

		public static LinqResolution GetResolution(SemanticModel model, InvocationExpressionSyntax invoke)
		{
			var symbol = (IMethodSymbol)model.GetSymbolInfo(invoke).Symbol;

			var sourceExpression = symbol.IsStatic
				? invoke.ArgumentList.Arguments[0].Expression
				: GetInstanceExpression(invoke);

			var sourceType = model.GetTypeInfo(sourceExpression).Type;
			return LinqMethod.Find(symbol.Name).GetResolution(sourceType.OriginalDefinition);
		}
	}
}
