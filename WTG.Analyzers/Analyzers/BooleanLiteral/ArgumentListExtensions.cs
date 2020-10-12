using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class ArgumentListExtensions
	{
		public static IParameterSymbol? TryFindCorrespondingParameterSymbol(this ArgumentListSyntax argumentList, SemanticModel semanticModel, int index, CancellationToken cancellationToken)
		{
			if (!argumentList.Parent.IsKind(SyntaxKind.InvocationExpression))
			{
				return null;
			}

			var methodInvocation = (InvocationExpressionSyntax)argumentList.Parent;
			if (methodInvocation is null)
			{
				return null;
			}

			var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(methodInvocation, cancellationToken).Symbol;
			if (methodSymbol is null)
			{
				return null;
			}

			var parameterSymbols = methodSymbol.Parameters;

			if (index >= parameterSymbols.Length)
			{
				var lastParameter = parameterSymbols[parameterSymbols.Length - 1];

				if (lastParameter.IsParams)
				{
					return lastParameter;
				}
			}

			var argumentSymbol = parameterSymbols[index];
			return argumentSymbol;
		}
	}
}
