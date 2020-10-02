using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace WTG.Analyzers
{
	static class ArgumentListExtensions
	{
		public static IParameterSymbol? TryFindCorrespondingParameterSymbol(this ArgumentListSyntax argumentList, SemanticModel semanticModel, int index, CancellationToken cancellationToken)
		{
			var methodInvocation = argumentList.FirstAncestorOrSelf<InvocationExpressionSyntax>();
			if (methodInvocation is null)
			{
				return null;
			}

			var methodSymbol = (IMethodSymbol)semanticModel.GetSymbolInfo(methodInvocation, cancellationToken).Symbol;
			if (methodSymbol is null)
			{
				return null;
			}

			var argumentSymbol = methodSymbol.Parameters[index];
			return argumentSymbol;
		}
	}
}
